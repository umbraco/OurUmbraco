using System;

namespace OurUmbraco.Community.BlogPosts {
    
    public class BlogCachedRssItem {

        public BlogInfo Blog { get; private set; }

        public BlogRssItem Item { get; private set; }

        public string Title {
            get { return Item.Title; }
        }

        public string Link {
            get { return Item.Link; }
        }

        public DateTimeOffset PublishedDate {
            get { return Item.PublishedDate; }
        }

        public BlogRssChannel Channel {
            get { return Item.Channel; }
        }

        public BlogCachedRssItem(BlogInfo blog, BlogRssItem item) {
            Blog = blog;
            Item = item;
        }

    }

}