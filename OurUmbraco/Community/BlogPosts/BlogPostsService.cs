using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        public async Task<BlogRssItem[]> GetBlogPosts(PerformContext context)
        {
            var posts = new List<BlogRssItem>();

            var progressBar = context.WriteProgressBar();
            var blogs = GetBlogs();

            foreach (var blog in blogs.WithProgress(progressBar, blogs.Length))
            {
                try
                {
                    context.WriteLine($"Processing blog {blog.Title}");
                    
                    var raw = await GetRawRssFeedResponse(context, blog.RssUrl);
                    if (string.IsNullOrWhiteSpace(raw))
                    {
                        // Go to the next item, the feed is empty indicating it couldn't be fetched
                        continue;
                    }
                    
                    // Parse the XML into a new instance of XElement
                    var feed = XElement.Parse(raw);

                    var channel = feed.Element("channel");
                    var channelTitle = channel.GetElementValue("title");
                    var channelLink = channel.GetElementValue("link");
                    
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

                    var logoPath = await GetBlogLogo(context, blog);
                }
                catch (Exception ex)
                {
                    context.SetTextColor(ConsoleTextColor.Red);
                    context.WriteLine($"Unable to get blog posts for: {blog.RssUrl} because of {ex.Message} {ex.StackTrace}");
                    context.ResetTextColor();
                }
            }

            return posts.OrderByDescending(x => x.PublishedDate).ToArray();
        }

        private static HttpClientHandler IgnoreTlsErrorsHandler()
        {
            if (ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12) == false)
            {
                ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;
            }
            
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };
            
            return handler;
        }

        private static async Task<string> GetRawRssFeedResponse(PerformContext context, string rssUrl)
        {
            using var client = new HttpClient(IgnoreTlsErrorsHandler());
            
            // Request XML response explicitly, otherwise you might get a prettified webpage
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            // Pretend to be the Edge browser, v99 because otherwise some RSS feed providers block you as a bot 
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.82 Safari/537.36 Edg/99.0.1150.36");
            
            using var result = await client.GetAsync(rssUrl);
            if (result.IsSuccessStatusCode == false)
            {
                context.SetTextColor(ConsoleTextColor.Red);
                context.WriteLine($"Getting {rssUrl} not successful, status: {result.StatusCode}, reason: {result.ReasonPhrase}");
                context.ResetTextColor();
                return string.Empty;
            }
            
            // Force read with UTF-8 encoding
            var buffer = await result.Content.ReadAsByteArrayAsync();
            var byteArray = buffer.ToArray();
            var raw = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
            raw = RemoveLeadingCharacters(raw).Replace("a10:updated", "pubDate");
            return raw;
        }

        private static async Task<string> GetBlogLogo(PerformContext context, BlogInfo blog)
        {
            // Get the avatar locally so that we can use ImageProcessor and serve it over https
            var baseLogoPath = HostingEnvironment.MapPath("~/media/blogs/");
            if (Directory.Exists(baseLogoPath) == false)
                Directory.CreateDirectory(baseLogoPath);
            var logoExtension = GetFileExtension(blog.LogoUrl);
            var logoPath = baseLogoPath + blog.Id + logoExtension;

            try
            {
                using var downloader = new HttpClient(IgnoreTlsErrorsHandler());
                var result = await downloader.GetStreamAsync(blog.LogoUrl);
            
                using var fs = new FileStream(logoPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await result.CopyToAsync(fs);
            }
            catch (Exception ex)
            {
                context.SetTextColor(ConsoleTextColor.Yellow);
                context.WriteLine($"Getting {blog.LogoUrl} not successful " + ex.Message, ex.StackTrace);
                context.ResetTextColor();
            }

            return logoPath;
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

        public async Task UpdateBlogPostsJsonFile(PerformContext context)
        {
            var service = new BlogPostsService();
            var posts = await service.GetBlogPosts(context);
            var rawJson = JsonConvert.SerializeObject(posts, Formatting.Indented);
            File.WriteAllText(JsonFile, rawJson, Encoding.UTF8);
        }
    }
}
