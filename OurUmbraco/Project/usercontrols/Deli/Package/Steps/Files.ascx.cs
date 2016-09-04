using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.NodeListing;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Our;
using OurUmbraco.Project.Helpers;
using OurUmbraco.Wiki.BusinessLogic;
using OurUmbraco.Wiki.Extensions;
using umbraco;
using umbraco.cms.businesslogic.member;

namespace OurUmbraco.Project.usercontrols.Deli.Package.Steps
{
    public partial class Files : UserControl
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


        //called after upload.
        private void RebindFiles()
        {
            var mediaProvider = new MediaProvider();
            var files = mediaProvider.GetMediaFilesByProjectId((int)ProjectId).Where(x => x.FileType != FileType.screenshot.FileTypeAsString());
            if (string.IsNullOrEmpty(_defaultFile))
            {
                var defaultFile = files.OrderByDescending(x => x.CreateDate).FirstOrDefault();

                if (defaultFile != null)
                {
                    MarkFileAsCurrent(defaultFile.Id.ToString());
                }
            }


            rp_packagefiles.DataSource = files;
            rp_packagefiles.Visible = (files.Count() > 0);
            rp_packagefiles.DataBind();
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
                Literal _name = (Literal)e.Item.FindControl("lt_name");
                Literal _date = (Literal)e.Item.FindControl("lt_date");
                Literal _type = (Literal)e.Item.FindControl("lt_type");
                Literal _version = (Literal)e.Item.FindControl("lt_version");
                Literal _dotNetVersion = (Literal)e.Item.FindControl("lt_dotNetVersion");
                Literal _currentRelease = (Literal)e.Item.FindControl("lt_currentRelease");

                Button _defaultPackageFile = (Button)e.Item.FindControl("bt_default");
                if (f.FileType == FileType.package.FileTypeAsString())
                {
                    if (f.Id.ToString() == _defaultFile)
                    {
                        _defaultPackageFile.Visible = false;
                        _currentRelease.Visible = true;
                        _currentRelease.Text = "Current";
                    }
                    else
                    {
                        _defaultPackageFile.Text = "Make Current";
                        _defaultPackageFile.CommandArgument = f.Id.ToString();
                        _currentRelease.Visible = false;
                    }
                }
                else
                {
                    _defaultPackageFile.Visible = false;
                    _currentRelease.Visible = false;

                }


                Button _archive = (Button)e.Item.FindControl("bt_archive");
                _archive.CommandArgument = f.Id.ToString();


                if (f.Archived)
                {
                    _archive.Text = "Unarchive";
                    _archive.CommandName = "Unarchive";
                }
                else
                {
                    _archive.Text = "Archive";
                    _archive.CommandName = "Archive";
                }

                if (f.FileType == FileType.screenshot.FileTypeAsString())
                {
                    _archive.Visible = false;
                }

                if (f.Versions != null)
                    _version.Text = f.Versions.ToVersionString();

                if (f.DotNetVersion != null)
                    _dotNetVersion.Text = f.DotNetVersion;

                _type.Text = f.FileType;

                _name.Text = "<a href='" + f.Path + "'>" + f.Name + "</a>";
                _date.Text = f.CreateDate.ToShortDateString() + " - " + f.CreateDate.ToShortTimeString();
            }
        }

        protected void SetDefaultPackage(object sender, CommandEventArgs e)
        {
            var releaseFile = e.CommandArgument.ToString();
            MarkFileAsCurrent(releaseFile);
            RebindFiles();
        }

        private void MarkFileAsCurrent(string releaseFile)
        {
            var nodeListingProvider = new NodeListingProvider();
            IListingItem project = nodeListingProvider.GetListing((int)ProjectId);
            project.CurrentReleaseFile = releaseFile;
            nodeListingProvider.SaveOrUpdate(project);
            _defaultFile = project.CurrentReleaseFile;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (library.IsLoggedOn() && ProjectId != null)
            {
                var nodeListingProvider = new NodeListingProvider();
                Member mem = Member.GetCurrentMember();
                IListingItem project = nodeListingProvider.GetListing((int)ProjectId);
                _defaultFile = project.CurrentReleaseFile;




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
                    
                    string umboptions = "";

                    foreach (UmbracoVersion uv in UmbracoVersion.AvailableVersions().Values)
                    {
                        umboptions += string.Format("<input type='checkbox' name='wiki_version' value='{0}' /><span> {1}</span></br>", uv.Version, uv.Name);
                    }

                    lt_versions.Text = umboptions;


                    string[] dotnetversions = { "", "2.0", "3.5", "4.0", "4.5", "4.5.1", "4.5.2", "4.6.0", "4.6.1" };
                    string dotnetoptions = string.Empty;

                    foreach (var opt in dotnetversions)
                    {
                        dotnetoptions += string.Format("<option value='{0}'>{1}</option>", opt, opt);

                    }

                    lt_dotnetversions.Text = dotnetoptions;
                }


            }

        }

        protected void SaveStep(object sender, EventArgs e)
        {
            //move to the file upload step
            ProjectCreatorHelper.MoveToNextStep(this, (int)ProjectId);
        }
        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the details step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}