using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.IO;
using Marketplace.BusinessLogic.ListingCreator;
using Marketplace.BusinessLogic.ListingCreator.StepDefinitions;
using Marketplace.Providers;
using Marketplace.Interfaces;
using umbraco.cms.businesslogic.member;
using our;

namespace uProject.usercontrols.Deli.Package
{
    public partial class Editor : System.Web.UI.UserControl
    {
        private string _currentStep = "";
        private static IVendor _vendor;
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


        protected void Page_Load(object sender, System.EventArgs e)
        {
            StepNavigation.DataSource = ListingSteps().Values;
            StepNavigation.DataBind();
        }


        private void loadContent(ListingCreatorStep currentStep)
        {
            StepPlaceHolder.Controls.Clear();
            var controlToLoad = new System.Web.UI.UserControl().LoadControl(IOHelper.ResolveUrl(currentStep.UserControl));

            if (currentStep.Alias == "details")
                ((uProject.usercontrols.Deli.Package.Steps.Details)controlToLoad).IsDeliVendor = _vendor.Member.IsDeliVendor;

            StepPlaceHolder.Controls.Add(controlToLoad);
            Step.Value = currentStep.Alias;
            _currentStep = currentStep.Alias;
        }

        int stepCounter = 0;


        protected void bindStep(object sender, RepeaterItemEventArgs e)
        {

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                ListingCreatorStep i = (ListingCreatorStep)e.Item.DataItem;

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
            ListingCreatorStep _s = ListingSteps().GotoNextStep(currentStep);
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



            _vendor = ((IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"]).GetVendorById(Member.GetCurrentMember().Id);

            if (_projectId != null)
            {
                _project = ((IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"]).GetListing((int)_projectId);


                //check security to make sure the project belongs to the vendor
                if (!((_project.Vendor.Member.Id == _vendor.Member.Id) || Utils.IsProjectContributor(_vendor.Member.Id, (int)_projectId)))
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
            ListingCreatorStepCollection lcs = new ListingCreatorStepCollection();
            lcs.Add(new Details(), _vendor.Member.IsDeliVendor);
            lcs.Add(new Files(), _vendor.Member.IsDeliVendor);
            lcs.Add(new Screenshots(), _vendor.Member.IsDeliVendor);

            //only add the licensing step is the project is commercial
            if (_project != null)
                lcs.Add(new Licenses(), (_project.ListingType == ListingType.commercial && _vendor.Member.IsDeliVendor));

            lcs.Add(new Complete(), _vendor.Member.IsDeliVendor);
            return lcs;
        }
    }
}