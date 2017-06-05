using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Xml.Extensions;
using umbraco;
using Umbraco.Core.Logging;

namespace OurUmbraco.Community.BlogPosts {

    public class BlogPostsService {

        public BlogInfo[] GetBlogs() {

            // Determine the path to the config file
            string configPath = HostingEnvironment.MapPath("~/config/CommunityBlogs.json");
            if (!System.IO.File.Exists(configPath)) {
                LogHelper.Warn<BlogPostsService>("Config file was not found: " + configPath);
                return new BlogInfo[0];
            }

            // Attempt to load information about each blog
            try {
                JObject root = JsonUtils.LoadJsonObject(configPath);
                return root.GetArrayItems("blogs", BlogInfo.Parse);
            } catch (Exception ex) {
                LogHelper.Error<BlogPostsService>("Unable to parse config file", ex);
                return new BlogInfo[0];
            }

        }

        public BlogRssItem[] GetBlogPosts() {

            List<BlogRssItem> posts = new List<BlogRssItem>();

            foreach (BlogInfo blog in GetBlogs()) {

                try {

                    // Initialize a new web client (with the encoding specified for the blog)
                    WebClient wc = new WebClient {
                        Encoding = blog.Encoding
                    };

                    // Download the raw XML
                    string raw = wc.DownloadString(blog.RssUrl);

                    // Parse the XML into a new instance of XElement
                    XElement feed = XElement.Parse(raw);

                    XElement channel = feed.Element("channel");
                    
                    string channelTitle = channel.GetElementValue("title");
                    string channelLink = channel.GetElementValue("link");
                    string channelDescription = channel.GetElementValue("description");
                    string channelLastBuildDate = channel.GetElementValue("lastBuildDate");
                    string channelLangauge= channel.GetElementValue("language");

                    BlogRssChannel rssChannel = new BlogRssChannel {
                        Id = blog.Id,
                        Title = channelTitle,
                        Link = channelLink
                    };

                    posts.AddRange(channel.GetElements("item").Select(item => new BlogRssItem {
                        Channel = rssChannel,
                        Title = item.GetElementValue("title"),
                        Link = item.GetElementValue("link"),
                        PublishedDate = item.GetElementValue("pubDate", BlogRssUtils.ParseRfc822DateTime)
                    }));
                
                } catch (Exception ex) {
                    LogHelper.Error<BlogPostsService>("Unable to get blog posts for: " + blog.RssUrl, ex);
                }

            }
            
            return posts.OrderByDescending(x => x.PublishedDate).ToArray();

        }

        public BlogCachedRssItem[] GetCachedBlogPosts() {

            // Determine the path to the JSON file
            string jsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

            // Return an empty array as the file doesn't exist
            if (!File.Exists(jsonPath)) return new BlogCachedRssItem[0];

            Dictionary<string, BlogInfo> blogs = GetBlogs().ToDictionary(x => x.Id);

            try {

                List<BlogCachedRssItem> temp = new List<BlogCachedRssItem>();

                foreach (var item in JsonUtils.LoadJsonArray(jsonPath).Select(token => token.ToObject<BlogRssItem>())) {
                    BlogInfo blog;
                    if (!blogs.TryGetValue(item.Channel.Id, out blog)) continue;
                    temp.Add(new BlogCachedRssItem(blog, item));
                }

                return temp.ToArray();
            } catch (Exception ex) {
                LogHelper.Error<BlogPostsService>("Unable to load blog posts from JSON file", ex);
                return new BlogCachedRssItem[0];
            }

        }

        public void UpdateBlogPostsJsonFile() {

            // Initialize a new service
            BlogPostsService service = new BlogPostsService();

            // Determine the path to the JSON file
            string jsonPath = HostingEnvironment.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

            // Generate the raw JSON
            string rawJson = JsonConvert.SerializeObject(service.GetBlogPosts(), Formatting.Indented);

            // Save the JSON to disk
            File.WriteAllText(jsonPath, rawJson, Encoding.UTF8);

        }

    }
}