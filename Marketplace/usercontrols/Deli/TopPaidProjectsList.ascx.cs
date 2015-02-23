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
    public partial class TopPaidProjectsList : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var ProjectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var Projects = ProjectsProvider.GetTopPaidListings(0,5, true);



            TopRepeater.DataSource = Projects;
            TopRepeater.DataBind();
        }
    }
}