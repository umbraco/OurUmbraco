using Examine;
using OurUmbraco.Community.BlogPosts;
using Skybrud.Essentials.Json;

namespace OurUmbraco.Search {

    public class BlogItemSearchResult {

        public SearchResult Result { get; private set; }

        public BlogRssItem Item { get; private set; }

        public BlogInfo Blog { get; private set; }

        public BlogItemSearchResult(SearchResult result, BlogInfo blog)
        {

            Result = result;
            Blog = blog;
            
            // Get the "data" field from the result
            string rawData;
            if (!result.Fields.TryGetValue("data", out rawData)) return;

            // Parse the JSON from the search result
            Item = JsonUtils.ParseJsonObject<BlogRssItem>(rawData);

            // Change the logo URL to the downloaded version
            Blog.LogoUrl = $"/media/blogs/{Blog.Id}{BlogUtils.GetFileExtension(Blog.LogoUrl)}";

        }

    }

}