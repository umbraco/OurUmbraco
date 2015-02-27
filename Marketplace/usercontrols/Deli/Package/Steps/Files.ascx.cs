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
using Marketplace.BusinessLogic;
using Marketplace.Umbraco.BusinessLogic;
using Marketplace.Providers.Helpers;
using Marketplace.Helpers;
using Marketplace.Providers.MediaFile;

namespace Marketplace.usercontrols.Deli.Package.Steps
{
    public partial class Files : System.Web.UI.UserControl
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
            var fileProvider = (IMediaProvider)MarketplaceProviderManager.Providers["MediaProvider"];
            var files = fileProvider.GetMediaFilesByProjectId((int)ProjectId).Where(x => x.FileType != FileType.screenshot);
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


        protected void DeleteFile(object sender, CommandEventArgs e)
        {
            var fileProvider = (IMediaProvider)MarketplaceProviderManager.Providers["MediaProvider"];
            var f = fileProvider.GetFileById(int.Parse(e.CommandArgument.ToString()));
           
            var mem = Member.GetCurrentMember();

            if (f.CreatedBy == mem.Id || Utils.IsProjectContributor(mem.Id, (int)ProjectId))

                //if the file is the default file we need to clear it out of the system to stop it showing as the default download
                if (f.Id.ToString() == _defaultFile)
                {
                    _defaultFile = string.Empty;
                    var listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                    var project = listingProvider.GetListing((int)ProjectId);
                    project.CurrentReleaseFile = _defaultFile;
                    listingProvider.SaveOrUpdate(project);

                }
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
                Literal _name = (Literal)e.Item.FindControl("lt_name");
                Literal _date = (Literal)e.Item.FindControl("lt_date");
                Button _delete = (Button)e.Item.FindControl("bt_delete");
                Literal _type = (Literal)e.Item.FindControl("lt_type");
                Literal _version = (Literal)e.Item.FindControl("lt_version");
                Literal _dotNetVersion = (Literal)e.Item.FindControl("lt_dotNetVersion");
                Literal _trustLevel = (Literal)e.Item.FindControl("lt_trustlevel");
                Literal _currentRelease = (Literal)e.Item.FindControl("lt_currentRelease");

                Button _defaultPackageFile = (Button)e.Item.FindControl("bt_default");
                if (f.FileType == FileType.package)
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

                if (f.FileType == FileType.screenshot)
                {
                    _archive.Visible = false;
                }

                if (f.UmbVersion != null)
                    _version.Text =  f.UmbVersion.ToVersionString();

                if (f.DotNetVersion != null)
                    _dotNetVersion.Text = f.DotNetVersion;

                _trustLevel.Text = (f.SupportsMediumTrust)?"Medium":"Full";


                switch (f.FileType)
                {
                    case FileType.screenshot:
                        _type.Text = "Screenshot";
                        break;
                    case FileType.package:
                        _type.Text = "Package";
                        break;
                    case FileType.hotfix:
                        _type.Text = "Hot Fix";
                        break;
                    case FileType.docs:
                        _type.Text = "Document";
                        break;
                    case FileType.source:
                        _type.Text = "Source";
                        break;
                    default:
                        break;
                }

                _name.Text = "<a href='" + f.Path + "'>" + f.Name + "</a>";
                _date.Text = f.CreateDate.ToShortDateString() + " - " + f.CreateDate.ToShortTimeString();
                _delete.CommandArgument = f.Id.ToString();

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
            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            IListingItem project = provider.GetListing((int)ProjectId);
            project.CurrentReleaseFile = releaseFile;
            provider.SaveOrUpdate(project);
            _defaultFile = project.CurrentReleaseFile;
        }

        protected void Page_Load(object sender, EventArgs e)
        {


            if (umbraco.library.IsLoggedOn() && ProjectId != null)
            {
                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                Member mem = Member.GetCurrentMember();
                IListingItem project = provider.GetListing((int)ProjectId);
                _defaultFile = project.CurrentReleaseFile;




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

                    string defaultVersion = UmbracoVersion.DefaultVersion().Version;
                    string umboptions = "";

                    foreach (UmbracoVersion uv in UmbracoVersion.AvailableVersions().Values)
                    {
                        string selected = "checked='true'";
                        if (uv.Version != defaultVersion)
                            selected = "";
                        umboptions += string.Format("<input type='checkbox' name='wiki_version' value='{0}' {2}/><span> {1}</span></br>", uv.Version, uv.Name, selected);
                    }

                    lt_versions.Text = umboptions;

                    
                    string[] dotnetversions = {"2.0","3.5","4.0"};
                    string dotnetoptions = string.Empty;

                    foreach (var opt in dotnetversions)
                    {
                        string selected = "selected='true'";
                        if (opt != "4.0")
                            selected = "";
                        dotnetoptions += string.Format("<option value='{0}' {2}>{1}</option>", opt, opt, selected);

                    }

                    lt_dotnetversions.Text = dotnetoptions;


                    string[] trustlevels = { "Full", "Medium" };
                    string trustoptions = string.Empty;

                    foreach (var opt in trustlevels)
                    {
                        string selected = "selected='true'";
                        if (opt != "Full")
                            selected = "";
                        trustoptions += string.Format("<option value='{0}' {2}>{1}</option>", opt, opt, selected);

                    }

                    lt_trustlevels.Text = trustoptions;


                    
                }


            }

        }

        protected void SaveStep(object sender, EventArgs e)
        {
            //move to the file upload step
            ProjectCreatorHelper.MoveToNextStep(this,(int)ProjectId);
        }
        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the details step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}