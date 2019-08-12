using OurUmbraco.Community.BlogPosts;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace OurUmbraco.Community.Models.ViewModels
{
    public class CommunityBlogPostViewModel : RenderModel
    {
        public CommunityBlogPostViewModel(IPublishedContent content) : base(content) { }

        public IEnumerable<BlogRssItem> Posts { get; set; }
    }
}
