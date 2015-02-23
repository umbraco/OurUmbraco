using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;

namespace Marketplace.usercontrols.Deli
{
    public partial class FeaturedProject : System.Web.UI.UserControl
    {
        public int projectId { get; set; }
        public string FeatureImage { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (projectId != 0)
            {
                var projectList = new List<IListingItem>();


                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                var project = provider.GetListing(projectId);
                projectList.Add(project);

                ProjectList.DataSource = projectList;
                ProjectList.DataBind();
            }
        }
    }
}