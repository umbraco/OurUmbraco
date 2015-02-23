using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using umbraco.cms.businesslogic.web;

namespace Marketplace.adhoc
{
    public partial class setalllive : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


                var projectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

                var projects = projectsProvider.GetAllListings(true, true).ToList();
                var count = 0;
                var liveCount = 0;

                var user = new umbraco.BusinessLogic.User(0);
                foreach (var p in projects)
                {

                    if (!p.Live)
                    {

                        count++;
                        Document d = new Document(p.Id);
                        d.getProperty("projectLive").Value = "1";
                        d.Publish(user);
                        umbraco.library.UpdateDocumentCache(d.Id);
                        Response.Write(d.Text + "+++++++:" + d.getProperty("projectLive").Value + "<br/>");
                    }
                       
                }

                Response.Write(count + " projects updated to be live of " + projects.Count() + "<br/>");
                Response.Write(liveCount + " are already live");

        }
    }
}