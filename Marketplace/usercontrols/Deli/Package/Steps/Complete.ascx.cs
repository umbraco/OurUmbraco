using System;
using System.Web.UI;
using OurUmbraco.MarketPlace.NodeListing;
using uProject.Helpers;

namespace uProject.usercontrols.Deli.Package.Steps
{
    public partial class Complete : UserControl
    {
        public string NotificationClass;

        private int? _projectId;
        public int? ProjectId
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["id"]))
                {
                    _projectId = int.Parse(Request["id"]);
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
                var nodeListingProvider = new NodeListingProvider();

                var project = nodeListingProvider.GetListing((int)ProjectId);
                ProjectName.Text = project.Name;

                Live.Checked = project.Live;
            }


        }

        protected void Complete_Click(object sender, EventArgs e)
        {
            var nodeListingProvider = new NodeListingProvider();
            var project = nodeListingProvider.GetListing((int)ProjectId);
            project.Live = Live.Checked;
            nodeListingProvider.SaveOrUpdate(project);
            Response.Redirect(project.NiceUrl);
        }

        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the license / screenshot step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}