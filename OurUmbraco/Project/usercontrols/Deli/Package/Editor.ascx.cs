using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using OurUmbraco.MarketPlace.BusinessLogic.ListingCreator;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.Our;
using OurUmbraco.Project.usercontrols.Deli.Package.Steps;
using umbraco.IO;
using Umbraco.Web.UI.Controls;
using Complete = OurUmbraco.MarketPlace.BusinessLogic.ListingCreator.StepDefinitions.Complete;
using Files = OurUmbraco.MarketPlace.BusinessLogic.ListingCreator.StepDefinitions.Files;
using Screenshots = OurUmbraco.MarketPlace.BusinessLogic.ListingCreator.StepDefinitions.Screenshots;

namespace OurUmbraco.Project.usercontrols.Deli.Package
{
    public partial class Editor : UmbracoUserControl
    {
        private string _currentStep = "";
        private static IListingItem _project;
        public string _currentStepClass = "current";

        private int? _projectId
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["id"]))
                {
                    return Int32.Parse(Request["id"]);
                }
                return null;
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            StepNavigation.DataSource = ListingSteps().Values;
            StepNavigation.DataBind();
        }


        private void loadContent(ListingCreatorStep currentStep)
        {
            StepPlaceHolder.Controls.Clear();
            var controlToLoad = new UserControl().LoadControl(IOHelper.ResolveUrl(currentStep.UserControl));

            //if (currentStep.Alias == "details")
            //    ((Details)controlToLoad).IsDeliVendor = false;

            StepPlaceHolder.Controls.Add(controlToLoad);
            Step.Value = currentStep.Alias;
            _currentStep = currentStep.Alias;
        }

        int stepCounter = 0;


        protected void bindStep(object sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var i = (ListingCreatorStep)e.Item.DataItem;

                if (!i.HideFromNavigation)
                {
                    Literal _class = (Literal)e.Item.FindControl("lt_class");
                    Literal _name = (Literal)e.Item.FindControl("lt_name");

                    if (i.Alias == _currentStep)
                        _class.Text = _currentStepClass;


                    stepCounter++;
                    _name.Text = i.Name;


                    //setup links to move back and forward through steps
                    if (_projectId != null)
                    {
                        _name.Text = string.Format("<a href=\"?editorStep=" + i.Alias + "&id=" + _projectId + "\">{0}</a>", _name.Text);
                    }
                }
                else
                    e.Item.Visible = false;
            }
        }


        public void GotoNextStep(string currentStep, int projectId)
        {
            ListingSteps().Get(currentStep).Completed = true;
            var _s = ListingSteps().GotoNextStep(currentStep);
            Response.Redirect("edit?editorStep=" + _s.Alias + "&id=" + projectId);
        }

        public void GoToPreviousStep(string currentStep, int projectId)
        {
            ListingCreatorStep _s = ListingSteps().GotoPreviousStep(currentStep);
            Response.Redirect("edit?editorStep=" + _s.Alias + "&id=" + projectId);
        }


        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            if (_projectId != null)
            {
                var nodeListingProvider = new NodeListingProvider();
                _project = nodeListingProvider.GetListing((int)_projectId);

                //check security to make sure the project belongs to the vendor
                var currentMemberId = Members.GetCurrentMemberId();
                var vendorIsCurrentMember = (_project.VendorId == currentMemberId);
                var isProjectContributor = Utils.IsProjectContributor(currentMemberId, (int)_projectId);
                if ((vendorIsCurrentMember || isProjectContributor) == false)
                {
                    //this project does not belong to this member so kick them back to the project list in their profile.
                    Response.Redirect("~/member/profile/projects/");
                }
            }

            _currentStep = umbraco.helper.Request("editorStep");

            ListingCreatorStep _s;

            if (string.IsNullOrEmpty(_currentStep))
                _s = ListingSteps()["details"];
            else
                _s = ListingSteps()[_currentStep];

            loadContent(_s);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion
        
        private static ListingCreatorStepCollection ListingSteps()
        {
            var lcs = new ListingCreatorStepCollection
            {
                { new OurUmbraco.MarketPlace.BusinessLogic.ListingCreator.StepDefinitions.Details(), false },
                { new Files(), false },
                { new Screenshots(), false },
                { new Complete(), false }
            };
            return lcs;
        }
    }
}