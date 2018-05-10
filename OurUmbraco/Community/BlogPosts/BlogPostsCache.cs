using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skybrud.Essentials.Json;
using Skybrud.Essentials.Json.Extensions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace OurUmbraco.Community.BlogPosts
{

    public class BlogPostsCache
    {

        public static readonly string BlogsJsonFile = IOHelper.MapPath("~/config/CommunityBlogs.json");

        public static readonly string PostsJsonFile = IOHelper.MapPath("~/App_Data/TEMP/CommunityBlogPosts.json");

        /// <summary>
        /// Gets an array with information about the blogs stored in the <c>~/config/CommunityBlogs.json</c> configuration file.
        /// </summary>
        /// <returns>Array of <see cref="BlogInfo"/>.</returns>
        public BlogInfo[] GetBlogs()
        {

            // Check whether the config file exists
            var configPath = BlogsJsonFile;
            if (File.Exists(BlogsJsonFile) == false)
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
        public BlogCachedRssItem[] GetCachedBlogPosts(int take, int numberOfPostsPerBlog)
        {
            
            // Return an empty array as the file doesn't exist
            if (File.Exists(PostsJsonFile) == false) return new BlogCachedRssItem[0];

            var blogs = GetBlogs().ToDictionary(x => x.Id.ToString());

            try
            {
                var blogPosts = new List<BlogCachedRssItem>();

                foreach (var item in JsonUtils.LoadJsonArray(PostsJsonFile).Select(token => token.ToObject<BlogRssItem>()))
                {
                    if (blogs.TryGetValue(item.Channel.Id, out var blog) == false)
                        continue;

                    blog.LogoUrl = $"/media/blogs/{blog.Id}{BlogUtils.GetFileExtension(blog.LogoUrl)}";
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

    }

}