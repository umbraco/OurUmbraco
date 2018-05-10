using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Xml.Linq;
using Hangfire.Console;
using Hangfire.Server;
using Skybrud.Essentials.Xml.Extensions;
using Umbraco.Core;

namespace OurUmbraco.Community.BlogPosts
{

    public class BlogPostsWebClient
    {

        /// <summary>
        /// Gets an array of the most recent RSS items of <paramref name="blogs"/> from the interwebz.
        /// </summary>
        /// <param name="blogs">Array of blogs.</param>
        /// <param name="context"></param>
        /// <returns>Array of <see cref="BlogRssItem"/>.</returns>
        public BlogRssItem[] GetBlogPosts(BlogInfo[] blogs, PerformContext context)
        {
            var posts = new List<BlogRssItem>();

            var progressBar = context.WriteProgressBar();

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
                        raw = BlogUtils.RemoveLeadingCharacters(raw).Replace("a10:updated", "pubDate");
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
                        Id = blog.Id.ToString(),
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

                        var guid = (string.IsNullOrEmpty(item.GetElementValue("guid"))
                                ? item.GetElementValue("link")
                                : item.GetElementValue("guid"))
                            .Trim();

                        var pubDate = BlogUtils.GetPublishDate(item);
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
                            Guid = guid,
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

                        var logoExtension = BlogUtils.GetFileExtension(blog.LogoUrl);
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

    }

}