using System.Web;
using System.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Our.CustomHandlers
{
    /// <summary>
    /// Custom route handler for the search page
    /// </summary>
    public class SearchPageRouteHandler : UmbracoVirtualNodeByIdRouteHandler, IRouteHandler
    {
        public SearchPageRouteHandler(int realNodeId)
            : base(realNodeId)
        {
        }

        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            //This wires umbraco bits up
            var mvcHandler = base.GetHttpHandler(requestContext);
            
            //use the page route handler to render webforms
            var pageHandler = new PageRouteHandler("~/Views/Search/Search.aspx", false);
            return pageHandler.GetHttpHandler(requestContext);
        }

        /// <summary>
        /// We're going to just return the content item by the id passed in, however we are going to manually change a couple of things like the 
        /// Doc type alias since that is the body css that is rendered
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="umbracoContext"></param>
        /// <param name="baseContent"></param>
        /// <returns></returns>
        protected override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext, IPublishedContent baseContent)
        {
            return new CustomContent(baseContent);
        }

        private class CustomContent : PublishedContentWrapped
        {
            public CustomContent(IPublishedContent content)
                : base(content)
            {
            }

            public override string DocumentTypeAlias
            {
                get { return "SearchResults"; }
            }
        }
    }
}