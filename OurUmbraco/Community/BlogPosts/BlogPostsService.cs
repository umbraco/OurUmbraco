using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Linq;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Xml.Extensions;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace OurUmbraco.Community.BlogPosts
{

    public class BlogPostsService
    {
        private static readonly string JsonFile = HostingEnvironment.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

        public BlogInfo[] GetBlogs()
        {

            // Determine the path to the config file
            var configPath = HostingEnvironment.MapPath("~/config/CommunityBlogs.json");
            if (File.Exists(configPath) == false)
            {
                LogHelper.Warn<BlogPostsService>("Config file was not found: " + configPath);
                return new BlogInfo[0];
            }

            // Attempt to load information about each blog
            try
            {
                var root = JsonUtils.LoadJsonObject(configPath);
                return root.GetArrayItems("blogs", BlogInfo.Parse);
            }
            catch (Exception ex)
            {
                LogHelper.Error<BlogPostsService>("Unable to parse config file", ex);
                return new BlogInfo[0];
            }
        }

        public BlogRssItem[] GetBlogPosts(PerformContext context)
        {
            var posts = new List<BlogRssItem>();

            var progressBar = context.WriteProgressBar();
            var blogs = GetBlogs();

            foreach (var blog in blogs.WithProgress(progressBar, blogs.Length))
            {
                try
                {
                    string raw;
                    const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3393.4 Safari/537.36";
                    context.WriteLine($"Processing blog {blog.Title}");
                    // Initialize a new web client (with the encoding specified for the blog)
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
                        wc.Encoding = blog.Encoding;

                        // Download the raw XML
                        raw = wc.DownloadString(blog.RssUrl);
                        raw = RemoveLeadingCharacters(raw).Replace("a10:updated", "pubDate");
                    }
                    // Parse the XML into a new instance of XElement
                    var feed = XElement.Parse(raw);

                    var channel = feed.Element("channel");
                    var channelTitle = channel.GetElementValue("title");
                    var channelLink = channel.GetElementValue("link");
                    var channelDescription = channel.GetElementValue("description");
                    var channelLastBuildDate = channel.GetElementValue("lastBuildDate");
                    var channelLangauge = channel.GetElementValue("language");

                    var rssChannel = new BlogRssChannel
                    {
                        Id = blog.Id,
                        Title = channelTitle,
                        Link = channelLink
                    };

                    var items = channel.GetElements("item");
                    foreach (var item in items)
                    {
                        var title = item.GetElementValue("title");
                        var link = (string.IsNullOrEmpty(item.GetElementValue("link"))
                            ? item.GetElementValue("guid")
                            : item.GetElementValue("link"))
                                .Trim();

                        var pubDate = GetPublishDate(item);
                        if (pubDate == default(DateTimeOffset))
                            continue;

                        var approvedCategories = new List<string> { "umbraco", "codegarden", "articulate", "examine" };
                        var categories = item.GetElements("category");
                        if (categories.Any())
                        {
                            var includeItem = title.ToLowerInvariant().ContainsAny(approvedCategories);
                            foreach (var category in categories)
                            {
                                // no need to check more if the item is already approved
                                if (includeItem)
                                    continue;

                                foreach (var approvedCategory in approvedCategories)
                                    if (category.Value.ToLowerInvariant().Contains(approvedCategory.ToLowerInvariant()))
                                        includeItem = true;
                            }

                            if (includeItem == false)
                            {
                                var allCategories = string.Join(",", categories.Select(i => i.Value));
                                context.SetTextColor(ConsoleTextColor.DarkYellow);
                                context.WriteLine($"Not including post titled {title} because it was not in an approved category. The categories it was found in: {allCategories}. [{link}]");
                                context.ResetTextColor();
                                continue;
                            }
                        }

                        // Blog has no category info and posts things unrelated to Umbraco, check there's related keywords in the title
                        if (blog.CheckTitles)
                        {
                            var includeItem = false;
                            foreach (var approvedCategory in approvedCategories)
                                if (title.ToLowerInvariant().Contains(approvedCategory.ToLowerInvariant()))
                                    includeItem = true;

                            // Blog post seems unrelated to Umbraco, skip it
                            if (includeItem == false)
                                continue;
                        }

                        var blogPost = new BlogRssItem
                        {
                            Channel = rssChannel,
                            Title = title,
                            // some sites store the link in the <guid/> element 
                            Link = link,
                            PublishedDate = pubDate
                        };

                        posts.Add(blogPost);
                    }

                    // Get the avatar locally so that we can use ImageProcessor and serve it over https
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
                        var baseLogoPath = HostingEnvironment.MapPath("~/media/blogs/");
                        if (Directory.Exists(baseLogoPath) == false)
                            Directory.CreateDirectory(baseLogoPath);
                        
                        var logoExtension = GetFileExtension(blog.LogoUrl);
                        var logoPath = baseLogoPath + blog.Id + logoExtension;
                        
                        wc.DownloadFile(blog.LogoUrl, logoPath);
                    }
                }
                catch (Exception ex)
                {
                    context.SetTextColor(ConsoleTextColor.Red);
                    context.WriteLine("Unable to get blog posts for: " + blog.RssUrl, ex);
                    context.ResetTextColor();
                }
            }

            return posts.OrderByDescending(x => x.PublishedDate).ToArray();
        }

        public async Task<IEnumerable<BlogRssItem>> GetUprofileBlogPosts()
        {
            var runtimeCache = ApplicationContext.Current.ApplicationCache.RuntimeCache;

            var cached = (IEnumerable<BlogRssItem>) runtimeCache.GetCacheItem("CommunityUProfileBlogPosts");
            if (cached != null) return cached;

            try
            {
                using (var client = new HttpClient())
                {
                    var result = await client.GetStringAsync("https://umbraco.com/blog/rss-uprofile-feed/");
                    if (result == null) return null;

                    var feed = XElement.Parse(result);
                    var posts = new List<BlogRssItem>();

                    var channel = feed.Element("channel");

                    var items = channel.GetElements("item");
                    foreach (var post in items)
                    {
                        var title = post.GetElementValue<string>("title")?.Trim();
                        var link = post.GetElementValue<string>("link")?.Trim();
                        var description = post.GetElementValue<string>("description")?.Trim();

                        var image = post.Element("image");
                        var thumbnail = image?.GetElementValue<string>("url")?.Trim();

                        var item = new BlogRssItem()
                        {
                            Title = title,
                            Link = link,
                            Thumbnail = thumbnail,
                            Description = description
                        };

                        posts.Add(item);
                    }

                    runtimeCache.InsertCacheItem("CommunityUProfileBlogPosts", () => posts, TimeSpan.FromHours(2));
                    return posts;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<BlogPostsService>("Unable to fetch uprofile posts", ex);
            }

            return null;
        }

        private static string GetFileExtension(string blogLogoUrl)
        {
            var extension = ".png";
            var url = blogLogoUrl;
            if (url.Contains("?"))
                url = blogLogoUrl.Substring(0, blogLogoUrl.IndexOf("?", StringComparison.Ordinal));

            var lastUrlSlug = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal));
            if (lastUrlSlug.Contains("."))
                extension = lastUrlSlug.Substring(lastUrlSlug.LastIndexOf(".", StringComparison.Ordinal));
            return extension;
        }

        private static string RemoveLeadingCharacters(string inString)
        {
            if (inString == null)
                return null;

            var substringStart = 0;
            for (var i = substringStart; i < inString.Length; i++)
            {
                var character = inString[i];
                if (character != '<')
                    continue;

                // As soon as we find the XML opening tag, break and return the rest of the string, else XElement.Parse fails
                substringStart = i;
                break;
            }

            return inString.Substring(substringStart);
        }

        private static DateTimeOffset GetPublishDate(XElement item)
        {
            var publishDate = item.GetElementValue("pubDate");
            DateTimeOffset pubDate;
            try
            {
                return Skybrud.Essentials.Time.TimeUtils.Rfc822ToDateTimeOffset(publishDate);
            }
            catch (Exception e)
            {
                // special dateformat, try normal C# date parser
            }

            DateTimeOffset.TryParse(publishDate, out pubDate);
            return pubDate;
        }

        public BlogCachedRssItem[] GetCachedBlogPosts(int take, int numberOfPostsPerBlog)
        {
            // Return an empty array as the file doesn't exist
            if (File.Exists(JsonFile) == false)
                return new BlogCachedRssItem[0];

            var blogs = GetBlogs().ToDictionary(x => x.Id);

            try
            {
                var blogPosts = new List<BlogCachedRssItem>();

                foreach (var item in JsonUtils.LoadJsonArray(JsonFile).Select(token => token.ToObject<BlogRssItem>()))
                {
                    if (blogs.TryGetValue(item.Channel.Id, out var blog) == false)
                        continue;

                    blog.LogoUrl = $"/media/blogs/{blog.Id}{GetFileExtension(blog.LogoUrl)}";
                    blogPosts.Add(new BlogCachedRssItem(blog, item));
                }

                var filteredBlogPosts = new List<BlogCachedRssItem>();
                foreach (var item in blogPosts)
                    if (filteredBlogPosts.Count(b => b.Blog.Id == item.Blog.Id) < numberOfPostsPerBlog)
                        filteredBlogPosts.Add(item);

                return filteredBlogPosts.Take(take).OrderByDescending(x => x.PublishedDate).ToArray();
            }
            catch (Exception ex)
            {
                LogHelper.Error<BlogPostsService>("Unable to load blog posts from JSON file", ex);
                return new BlogCachedRssItem[0];
            }
        }

        public void UpdateBlogPostsJsonFile()
        {
            // Initialize a new service
            var service = new BlogPostsService();

            // Generate the raw JSON
            var rawJson = JsonConvert.SerializeObject(service.GetBlogPosts(null), Formatting.Indented);

            // Save the JSON to disk
            File.WriteAllText(JsonFile, rawJson, Encoding.UTF8);
        }
    }
}
