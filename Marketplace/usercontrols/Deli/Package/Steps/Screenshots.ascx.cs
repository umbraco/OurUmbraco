using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using umbraco.cms.businesslogic.member;
using our;
using Marketplace.Umbraco.BusinessLogic;
using Marketplace.Providers.Helpers;
using uProject.Helpers;

namespace uProject.usercontrols.Deli.Package.Steps
{
    public partial class Screenshots : System.Web.UI.UserControl
    {

        public string MemberGuid = "";
        public string ProjectGuid = "";


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

        private string _defaultFile
        {
            get;
            set;
        }

        private void RebindFiles()
        {
            var fileProvider = (IMediaProvider)MarketplaceProviderManager.Providers["MediaProvider"];
            var files = fileProvider.GetMediaFilesByProjectId((int)ProjectId).Where(x=>x.FileType == FileType.screenshot);

            if (string.IsNullOrEmpty(_defaultFile))
            {
                var defaultFile = files.OrderByDescending(x => x.CreateDate).FirstOrDefault();

                if (defaultFile != null)
                {
                    MarkFileAsCurrent(defaultFile.Path);
                }
            }

            rp_screenshots.DataSource = files;
            rp_screenshots.Visible = (files.Count() > 0);
            rp_screenshots.DataBind();
        }


        protected void DeleteFile(object sender, CommandEventArgs e)
        {
            var fileProvider = (IMediaProvider)MarketplaceProviderManager.Providers["MediaProvider"];
            var f = fileProvider.GetFileById(int.Parse(e.CommandArgument.ToString()));
            _defaultFile = string.Empty;

            //update the project
            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            IListingItem project = provider.GetListing((int)ProjectId);
            project.DefaultScreenshot = _defaultFile;
            provider.SaveOrUpdate(project);



            var mem = Member.GetCurrentMember();

            if (f.CreatedBy == mem.Id || Utils.IsProjectContributor(mem.Id, (int)ProjectId))
                fileProvider.Remove(f);

            RebindFiles();
        }

        protected void ArchiveFile(object sender, CommandEventArgs e)
        {

            var fileProvider = (IMediaProvider)MarketplaceProviderManager.Providers["MediaProvider"];
            var f = fileProvider.GetFileById(int.Parse(e.CommandArgument.ToString()));

            if (e.CommandName == "Unarchive")
            {
                f.Archived = false;
            }
            else
            {
                f.Archived = true;
            }

            fileProvider.SaveOrUpdate(f);
            RebindFiles();
        }

        protected void OnFileBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                IMediaFile f = (IMediaFile)e.Item.DataItem;
                Image _image = (Image)e.Item.FindControl("img_image");
                Literal _date = (Literal)e.Item.FindControl("lt_date");
                Button _delete = (Button)e.Item.FindControl("bt_delete");
                Literal _defaultScreenshot = (Literal)e.Item.FindControl("lt_default");

                Button _defaultScreenshotButton = (Button)e.Item.FindControl("bt_default");

                    if (f.Path == _defaultFile)
                    {
                        _defaultScreenshotButton.Visible = false;
                        _defaultScreenshot.Visible = true;
                        _defaultScreenshot.Text = "Default";
                    }
                    else
                    {
                        _defaultScreenshotButton.Text = "Make Default";
                        _defaultScreenshotButton.CommandArgument = f.Path;
                        _defaultScreenshot.Visible = false;
                    }
                
                _image.ImageUrl = "/umbraco/imagegen.ashx?image=" + f.Path + "&height=100";
                _date.Text = f.CreateDate.ToShortDateString() + " - " + f.CreateDate.ToShortTimeString();
                _delete.CommandArgument = f.Id.ToString();

            }
        }

        protected void SetDefaultImage(object sender, CommandEventArgs e)
        {
            var defaultScreenshot = e.CommandArgument.ToString();
            MarkFileAsCurrent(defaultScreenshot);
            RebindFiles();
        }

        private void MarkFileAsCurrent(string defaultScreenshot)
        {
            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            IListingItem project = provider.GetListing((int)ProjectId);
            project.DefaultScreenshot = defaultScreenshot;
            provider.SaveOrUpdate(project);
            _defaultFile = project.DefaultScreenshot;
        }

        protected void Page_Load(object sender, EventArgs e)
        {


            if (umbraco.library.IsLoggedOn() && ProjectId != null)
            {
                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                Member mem = Member.GetCurrentMember();
                IListingItem project = provider.GetListing((int)ProjectId);
                _defaultFile = project.DefaultScreenshot;


                if ((project.Vendor.Member.Id == mem.Id) ||
                    Utils.IsProjectContributor(mem.Id, (int)ProjectId))
                {
                    holder.Visible = true;
                    RebindFiles();

                    umbraco.library.RegisterJavaScriptFile("swfUpload", "/scripts/swfupload/SWFUpload.js");
                    umbraco.library.RegisterJavaScriptFile("swfUpload_cb", "/scripts/swfupload/callbacks.js");
                    umbraco.library.RegisterJavaScriptFile("swfUpload_progress", "/scripts/swfupload/fileprogress.js");

                    MemberGuid = mem.UniqueId.ToString();
                    ProjectGuid = project.ProjectGuid.ToString();
                }
            }
        }

        protected void SaveStep(object sender, EventArgs e)
        {
            //move to the license step
            ProjectCreatorHelper.MoveToNextStep(this,(int)ProjectId);
        }

        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the files step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}