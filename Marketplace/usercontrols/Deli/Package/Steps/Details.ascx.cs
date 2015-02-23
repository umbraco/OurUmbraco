using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Providers.Helpers;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.ListingItem;
using Marketplace.BusinessLogic.ListingCreator;
using umbraco.cms.businesslogic.member;
using Marketplace.BusinessLogic;
using Marketplace.Helpers;
using umbraco.NodeFactory;
using System.Text.RegularExpressions;

namespace Marketplace.usercontrols.Deli.Package.Steps
{
    public partial class Details : System.Web.UI.UserControl
    {

        public bool IsDeliVendor { get; set; }

        private bool _editMode
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["id"]))
                {
                    return true;
                }
                return false;
            }
        }


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

        protected override void OnInit(EventArgs e)
        {
            ((umbraco.UmbracoDefault)this.Page).ValidateRequest = false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            umbraco.library.RegisterJavaScriptFile("tinyMce", "/scripts/tiny_mce/tiny_mce_src.js");
            if (!IsPostBack)
            {
                SetupTags();
                BindCategories(); 

                //if the user is a registered deli vendor show them the commercial options
                CommercialOption.Visible = IsDeliVendor;


                if (_editMode)
                {
                    MoveNext.Text = "Save";
                    LoadProject();
                }
                else
                {
                    MoveNext.Text = "Next";
                }

              
            }


        }

        private void SetupTags()
        {
            string taglist = string.Empty;
            var tagProvider = ((IProjectTagProvider)MarketplaceProviderManager.Providers["TagProvider"]);
            var tags = tagProvider.GetAllTags();
            foreach(var t in tags)
            {
                taglist += "\"" + t.Text + "\",";
            }

            ScriptManager.RegisterStartupScript(
                        this,
                        this.GetType(),
                        "inittagsuggest",
                        " $(document).ready(function() { $('#projecttagger').autocomplete([" + taglist + "],{max: 8,scroll: true,scrollHeight: 300}); enableTagger();});",
                        true);
        }


        private void BindCategories()
        {
            var Categories = ((ICategoryProvider)MarketplaceProviderManager.Providers["CategoryProvider"]).GetAllCategories().Where(x => !x.HQOnly);
            Category.DataSource = Categories;
            Category.DataValueField = "Id";
            Category.DataTextField = "Name";
            Category.DataBind();
        }

        protected void LoadProject()
        {
            var ProjectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var project = ProjectsProvider.GetListing((int)ProjectId);

            Title.Text = project.Name;
            Description.Text = project.Description;
            CurrentVersion.Text = project.CurrentVersion;
            DevStatus.Text = project.DevelopmentStatus;
            Stable.Checked = project.Stable;
            CommercialOrFree.SelectedValue = project.ListingType.ListingTypeAsString();
            LicenseName.Text = project.LicenseName;
            LicenseUrl.Text = project.LicenseUrl;
            ProjectUrl.Text = project.ProjectUrl;
            SourceCodeUrl.Text = project.SourceCodeUrl;
            DemoUrl.Text = project.DemonstrationUrl;
            Category.SelectedValue = project.CategoryId.ToString();
            SupportUrl.Text = project.SupportUrl;
            GaCode.Text = project.GACode;
            Collab.Checked = project.OpenForCollab;
            Terms.Checked = project.TermsAgreementDate != new DateTime();



            var tagProvider = ((IProjectTagProvider)MarketplaceProviderManager.Providers["TagProvider"]);
            var projecttags = tagProvider.GetTagsByProjectId((int)ProjectId);

            if (projecttags.Count() > 0)
            {
                string stags = string.Empty;
                foreach (var tag in projecttags)
                {
                    stags += tag.Text + ",";
                }

                stags = stags.Substring(0, stags.Length - 1);

                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "inittags",
                    " $(document).ready(function() {$('#projecttagger').addTag('" + stags + "');});",
                    true);
            }

            

        }


        protected void SaveStep(object sender, EventArgs e)
        {
            var ProjectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var project = (_editMode) ? ProjectsProvider.GetListing((int)ProjectId) : new ListingItem(null,null,null);

            project.Name = Title.Text;
            project.Description = Description.Text;
            project.CurrentVersion = CurrentVersion.Text;
            project.DevelopmentStatus = DevStatus.Text;
            project.Stable = Stable.Checked;
            project.LicenseName = LicenseName.Text;
            project.LicenseUrl = LicenseUrl.Text;
            project.ProjectUrl = ProjectUrl.Text;
            project.SourceCodeUrl = SourceCodeUrl.Text;
            project.DemonstrationUrl = DemoUrl.Text;
            project.CategoryId = Int32.Parse(Category.SelectedValue);
            project.SupportUrl = SupportUrl.Text;

            if (!_editMode)
            {
                project.Vendor = ((IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"]).GetVendorById(Member.GetCurrentMember().Id);
            }

            project.OpenForCollab = Collab.Checked;
            project.GACode = GaCode.Text;
            project.ProjectGuid = (project.ProjectGuid == Guid.Empty)?Guid.NewGuid():project.ProjectGuid; //this is used as the Unique project ID.
            project.ListingType = (CommercialOption.Visible) ? (ListingType)Enum.Parse(typeof(ListingType), (string)CommercialOrFree.SelectedValue, true) : ListingType.free;

            project.TermsAgreementDate = DateTime.Now.ToUniversalTime();

            ProjectsProvider.SaveOrUpdate(project);
            ProjectId = project.Id;

            if (Request["projecttags[]"] != null)
            {
                var tagProvider = ((IProjectTagProvider)MarketplaceProviderManager.Providers["TagProvider"]);
                tagProvider.SetTags(project.Id, Request["projecttags[]"].ToString());
                project.Tags = tagProvider.GetTagsByProjectId(project.Id);
                ProjectsProvider.SaveOrUpdate(project);
            }

            //move to the file upload step
            ProjectCreatorHelper.MoveToNextStep(this, (int)ProjectId);


        }

        protected string[] GetSelected(ListBox lb)
        {
            var strArr = "";
            foreach(ListItem li in lb.Items){
               if(li.Selected) strArr+=li.Value + ",";
            }

            return strArr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        } 
    }
}