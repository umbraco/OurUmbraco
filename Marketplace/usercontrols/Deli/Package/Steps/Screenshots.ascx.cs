using System;
using System.Linq;
using System.Web.UI.WebControls;
using our;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco;
using umbraco.cms.businesslogic.member;
using uProject.Helpers;
using Umbraco.Web.UI.Controls;

namespace uProject.usercontrols.Deli.Package.Steps
{
    public partial class Screenshots : UmbracoUserControl
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
            var mediaProvider = new MediaProvider();
            var files = mediaProvider.GetMediaFilesByProjectId((int)ProjectId).Where(x=>x.FileType == FileType.screenshot.FileTypeAsString());

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
            var mediaProvider = new MediaProvider();
            var f = mediaProvider.GetFileById(int.Parse(e.CommandArgument.ToString()));
            _defaultFile = string.Empty;

            //update the project
            var nodeListingProvider = new NodeListingProvider();
            IListingItem project = nodeListingProvider.GetListing((int)ProjectId);
            project.DefaultScreenshot = _defaultFile;
            nodeListingProvider.SaveOrUpdate(project);
            
            var mem = Member.GetCurrentMember();

            if (f.CreatedBy == mem.Id || Utils.IsProjectContributor(mem.Id, (int)ProjectId))
                mediaProvider.Remove(f);

            RebindFiles();
        }

        protected void ArchiveFile(object sender, CommandEventArgs e)
        {
            var mediaProvider = new MediaProvider();
            var f = mediaProvider.GetFileById(int.Parse(e.CommandArgument.ToString()));

            if (e.CommandName == "Unarchive")
            {
                f.Archived = false;
            }
            else
            {
                f.Archived = true;
            }

            mediaProvider.SaveOrUpdate(f);
            RebindFiles();
        }

        protected void OnFileBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                WikiFile f = (WikiFile)e.Item.DataItem;
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
            var nodeListingProvider = new NodeListingProvider();
            IListingItem project = nodeListingProvider.GetListing((int)ProjectId);
            project.DefaultScreenshot = defaultScreenshot;
            nodeListingProvider.SaveOrUpdate(project);
            _defaultFile = project.DefaultScreenshot;
        }

        protected void Page_Load(object sender, EventArgs e)
        {


            if (library.IsLoggedOn() && ProjectId != null)
            {
                var nodeListingProvider = new NodeListingProvider();
                Member mem = Member.GetCurrentMember();
                IListingItem project = nodeListingProvider.GetListing((int)ProjectId);
                _defaultFile = project.DefaultScreenshot;


                if ((project.VendorId == mem.Id) ||
                    Utils.IsProjectContributor(mem.Id, (int)ProjectId))
                {
                    holder.Visible = true;
                    RebindFiles();

                    library.RegisterJavaScriptFile("swfUpload", "/scripts/swfupload/SWFUpload.js");
                    library.RegisterJavaScriptFile("swfUpload_cb", "/scripts/swfupload/callbacks.js");
                    library.RegisterJavaScriptFile("swfUpload_progress", "/scripts/swfupload/fileprogress.js");

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