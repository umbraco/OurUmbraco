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
    public partial class HQStaffPickProjects : System.Web.UI.UserControl
    {
        public string PickedProjects { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(PickedProjects))
            {
                var idArray = PickedProjects.Split(',');
                var listProject = new List<IListingItem>();
                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

                foreach (var id in idArray)
                {
                    try
                    {
                        var listing = provider.GetListing(Int32.Parse(id),true);
                        listProject.Add(listing);

                    }
                    catch
                    {
                        // do nothing the item doesnt exist;
                    }
                }

                
                Listing.DataSource = listProject;
                Listing.DataBind();

            }
        }
    }
}