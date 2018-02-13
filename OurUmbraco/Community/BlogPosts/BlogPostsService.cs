using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using Newtonsoft.Json;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Xml.Extensions;
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

        public BlogRssItem[] GetBlogPosts()
        {
            var posts = new List<BlogRssItem>();

            foreach (var blog in GetBlogs())
            {
                try
                {
                    string raw;
                    
                    // Need to make sure we try TLS 1.2 first else the connection will just be closed in us 
                    // No other protocols allowed SSL * and TLS 1.0 are considered insecure
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
                    
                    // Initialize a new web client (with the encoding specified for the blog)
                    using (var wc = new WebClient())
                    {
                        wc.Encoding = blog.Encoding;

                        // Download the raw XML
                        raw = wc.DownloadString(blog.RssUrl);
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

                    posts.AddRange(channel.GetElements("item").Select(item => new BlogRssItem
                    {
                        Channel = rssChannel,
                        Title = item.GetElementValue("title"),
                        Link = item.GetElementValue("link"),
                        PublishedDate = Skybrud.Essentials.Time.TimeUtils.Rfc822ToDateTimeOffset(item.GetElementValue("pubDate"))
                    }));

                }
                catch (Exception ex)
                {
                    LogHelper.Error<BlogPostsService>("Unable to get blog posts for: " + blog.RssUrl, ex);
                }
            }

            return posts.OrderByDescending(x => x.PublishedDate).ToArray();
        }
        
        public BlogCachedRssItem[] GetCachedBlogPosts()
        {
            // Return an empty array as the file doesn't exist
            if (File.Exists(JsonFile) == false)
                return new BlogCachedRssItem[0];

            var blogs = GetBlogs().ToDictionary(x => x.Id);

            try
            {
                var temp = new List<BlogCachedRssItem>();

                foreach (var item in JsonUtils.LoadJsonArray(JsonFile).Select(token => token.ToObject<BlogRssItem>()))
                {
                    BlogInfo blog;
                    if (!blogs.TryGetValue(item.Channel.Id, out blog)) continue;
                    temp.Add(new BlogCachedRssItem(blog, item));
                }

                return temp.ToArray();
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
            var rawJson = JsonConvert.SerializeObject(service.GetBlogPosts(), Formatting.Indented);

            // Save the JSON to disk
            File.WriteAllText(JsonFile, rawJson, Encoding.UTF8);
        }
    }
}