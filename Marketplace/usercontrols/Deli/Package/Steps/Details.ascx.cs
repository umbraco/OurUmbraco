using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.ListingItem;
using umbraco;
using uProject.Helpers;
using Umbraco.Core.Models;
using Umbraco.Web.UI.Controls;
using Member = umbraco.cms.businesslogic.member.Member;

namespace uProject.usercontrols.Deli.Package.Steps
{
    public partial class Details : UmbracoUserControl
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
            ((UmbracoDefault)this.Page).ValidateRequest = false;
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
            var projectTags = tagProvider.GetTagsByProjectId((int)ProjectId).ToList();

            if (projectTags.Any())
            {
                var tags = new List<string>();
                foreach (var tag in projectTags)
                {
                    if(tags.Any(x => string.Equals(x.Trim(), tag.Text.Trim(), StringComparison.InvariantCultureIgnoreCase)) == false)
                        tags.Add(tag.Text);
                }
                
                ScriptManager.RegisterStartupScript(
                    this,
                    this.GetType(),
                    "inittags",
                    " $(document).ready(function() {$('#projecttagger').addTag('" + string.Join(",", tags) + "');});",
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
                ProjectsProvider.SaveOrUpdate(project);
                
                var tags = new List<string>();
                foreach (var tag in Request["projecttags[]"].Split(','))
                {
                    if (tags.Any(x => string.Equals(x.Trim(), tag.Trim(), StringComparison.InvariantCultureIgnoreCase)) == false)
                        tags.Add(tag);
                }

                var contentService = Services.ContentService;
                var projectContent = contentService.GetById(project.Id);
                projectContent.SetTags("tags", tags, true, "project");
                contentService.Save(projectContent);
            }

            //move to the file upload step
            ProjectCreatorHelper.MoveToNextStep(this, (int)ProjectId);
        }

        protected string[] GetSelected(ListBox lb)
        {
            var strArr = "";
            foreach(ListItem li in lb.Items)
            {
               if(li.Selected) strArr+=li.Value + ",";
            }

            return strArr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        } 
    }
}