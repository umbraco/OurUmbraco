using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.BusinessLogic;
using uProject.Helpers;

namespace uProject.usercontrols.Deli.Package.Steps
{
    public partial class Complete : System.Web.UI.UserControl
    {
        public string NotificationClass;

        private int? _projectId;
        public int? ProjectId
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["id"]))
                {
                    _projectId = Int32.Parse(Request["id"]);
                }
                return _projectId;
            }
            set
            {
                _projectId = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

                IListingItem project = provider.GetListing((int)ProjectId);
                var eligible = Listing.CheckEligibility(project);
                NotificationMessage.Text = eligible.Message;
                NotificationClass = (eligible.IsEligible) ? "eligible" : "notEligible";

                ProjectName.Text = project.Name;

                Live.Checked = project.Live;


                if (!eligible.IsEligible)
                {
                    MoveNext.Enabled = false;
                }
            }


        }

        protected void Complete_Click(object sender, EventArgs e)
        {
            var ProjectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var project = ProjectsProvider.GetListing((int)ProjectId);
            project.Live = Live.Checked;
            ProjectsProvider.SaveOrUpdate(project);
            Response.Redirect(project.NiceUrl);
        }

        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the license / screenshot step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}