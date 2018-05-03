using Examine;
using OurUmbraco.Community.BlogPosts;

namespace OurUmbraco.Search {

    public class BlogItemSearchResult {

        public SearchResult Result { get; private set; }

        public BlogRssItem Item { get; private set; }

        public BlogInfo Blog { get; private set; }

        public BlogItemSearchResult(SearchResult result, BlogRssItem item, BlogInfo blog)
        {
            Result = result;
            Item = item;
            Blog = blog;
        }

    }

}