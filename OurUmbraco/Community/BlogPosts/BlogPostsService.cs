using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Hosting;
using System.Xml.Linq;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using OurUmbraco.Our.Examine;
using Skybrud.Essentials.Xml.Extensions;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Persistence;

namespace OurUmbraco.Community.BlogPosts
{

    public class BlogPostsService
    {
        
        private BlogPostsCache _cache;
        private BlogPostsWebClient _webClient;

        protected UmbracoDatabase Database { get; private set; }

        protected BlogPostsCache Cache => _cache ?? (_cache = new BlogPostsCache());

        protected BlogPostsWebClient WebClient => _webClient ?? (_webClient = new BlogPostsWebClient());

        public BlogPostsService()
        {
            Database = ApplicationContext.Current.DatabaseContext.Database;
        }


        public BlogInfo[] GetBlogs()
        {
            return Cache.GetBlogs();
        }


        public void UpdateBlogPosts(PerformContext context)
        {

            // Get an array of all blogs listed in the config file
            var blogs = GetBlogs();

            context.WriteLine("Loaded " + blogs.Length + " blogs from config file");

            // Get a list of all existing blog items and add them to a dictionary
            var sqæl = new Sql().Select("*").From(BlogDatabaseItem.TableName);
            var all = Database.Fetch<BlogDatabaseItem>(sqæl).ToDictionary(x => x.UniqueId);

            // Get the most recent items of the blogs that we know about
            BlogRssItem[] items = WebClient.GetBlogPosts(blogs, context);

            context.WriteLine("Fetched " + items.Length + " items from teh interwebz");
            context.WriteLine();

            // Iterate through the posts from the current RSS feeds
            foreach (var post in items)
            {

                // Get the ID of the blog
                string blogId = post.Channel.Id;

                // Calculate a unique ID for the item and the blog
                string itemUniqueId = Skybrud.Essentials.Security.SecurityUtils.GetMd5Hash(blogId + post.Guid);

                BlogDatabaseItem db;

                if (all.TryGetValue(itemUniqueId, out db))
                {

                    // TODO: can we use the ICanBeDirty interface here?
                    bool isDirty = false;

                    DateTime published = post.PublishedDate.ToUniversalTime().DateTime;
                    string dataRaw = db.DataRaw;

                    isDirty |= db.PublishedDate != published;
                    isDirty |= db.Title != post.Title;

                    db.PublishedDate = published;
                    db.Title = post.Title;
                    db.Data = post;

                    isDirty |= db.DataRaw != dataRaw;

                    if (isDirty)
                    {
                        Update(db);
                        all[db.UniqueId] = db;
                    }

                }
                else
                {

                    db = new BlogDatabaseItem
                    {
                        UniqueId = itemUniqueId,
                        BlogId = blogId,
                        PublishedDate = post.PublishedDate.ToUniversalTime().DateTime,
                        Title = post.Title,
                        Data = post
                    };

                    Insert(db);
                    all[db.UniqueId] = db;

                }

            }

            // Determine the path to the JSON file
            var jsonPath = IOHelper.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

            // Generate the raw JSON
            var rawJson = JsonConvert.SerializeObject(all.Values.Select(x => x.Data).OrderByDescending(x => x.PublishedDate), Formatting.Indented);

            // Save the JSON to disk
            File.WriteAllText(jsonPath, rawJson, Encoding.UTF8);

        }

        public BlogDatabaseItem[] GetAllBlogItemsFromDatabase()
        {

            // Get a list of all existing blog items and add them to a dictionary
            Sql sqæl = new Sql().Select("*").From(BlogDatabaseItem.TableName);

            return Database.Fetch<BlogDatabaseItem>(sqæl).ToArray();

        }

        /// <summary>
        /// Inserts the specified <paramref name="item"/> to the database and adds it to the Examine index.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Insert(BlogDatabaseItem item)
        {
            Database.Insert(item);
            BlogItemsIndexDataService.ReIndex(item);
        }

        /// <summary>
        /// Updates the specified <paramref name="item"/> in the database and updates it in the Examine index.
        /// </summary>
        /// <param name="item">The item to be updated.</param>
        public void Update(BlogDatabaseItem item)
        {
            Database.Update(item);
            BlogItemsIndexDataService.ReIndex(item);
        }

    }

}