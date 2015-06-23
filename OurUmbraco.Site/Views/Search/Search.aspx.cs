using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using our.Examine;
using our.Models;
using Umbraco.Web.Models;
using Umbraco.Web.UI.Pages;

namespace OurUmbraco.Site.Views.Search
{
    public partial class Search : BasePage
    {
        protected SearchResultContentModel Model { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                var umbracoPage = UmbracoContext.PublishedContentRequest.PublishedContent;

                var ourSearcher = new OurSearcher(Request.QueryString["q"], maxResults: 100);

                var results = ourSearcher.Search();

                Model = new SearchResultContentModel(umbracoPage, results);

                SearchText.Text = Model.Results.SearchTerm;
            }
            else
            {
                Response.Redirect("/search?q=" + SearchText.Text);
            }
            
        }
    }
}