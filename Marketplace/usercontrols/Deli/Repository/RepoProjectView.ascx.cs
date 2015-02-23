using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using umbraco.cms.businesslogic.member;
using Marketplace.Helpers;
using Marketplace.Providers.Cart;
using Marketplace.Providers.Accounting;


namespace Marketplace.usercontrols.Deli
{

    public partial class RepoProjectView : System.Web.UI.UserControl
    {
        public IListingItem Project { get; set; }
        public string listingId;

        public string callback;
        public string version;
        public string useLegacySchema;


        protected void Page_Load(object sender, EventArgs e)
        {
            var projectProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

            version = Request.QueryString["version"];
            useLegacySchema = Request.QueryString["useLegacySchema"];
            callback = Request.QueryString["callback"];

            listingId = Request.QueryString["id"];


            int pId = 0;
            if (int.TryParse(listingId, out pId))
            {
                pId = int.Parse(listingId);
                Project = projectProvider.GetListing(pId);
            }
            else
            {
                Project = projectProvider.GetCurrentListing();
            }

            if (!IsPostBack && Project != null)
            {
                projectProvider.IncrementProjectViews(Project);
                SetupProject();
            }
            
        }

        private void SetupProject()
        {
            var vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            var archive = new List<IMediaFile>();

            #region Verified File Notice

            var package = Project.PackageFile.Where(x => x.Current).FirstOrDefault();

            if (package != null)
            {
                if (!package.Verified)
                {
                    NotVerifiedNotice.Visible = true;
                }
            }
 
            #endregion

            #region Standard Features

            ProjectName.Text = Project.Name;
            ProjectCreateDate.Text = String.Format("{0:D}", Project.CreateDate);
            ProjectCurrentVersion.Text = Project.CurrentVersion;
            VendorLink.Text = Project.Vendor.Member.Name;
            VendorLink.NavigateUrl = "/member/" + Project.Vendor.Member.Id;
            FullProjectLink.NavigateUrl = "http://deli.umbraco.org" + umbraco.library.NiceUrl(Project.Id);


            if (Project.Description.Contains('<'))
            {
                ProjectDescrition.Text = uForum.Library.Xslt.ResolveLinks(Project.Description.Substring(0, 200));
                LongProjectDescription.Text = uForum.Library.Xslt.ResolveLinks(Project.Description.Substring(200));
            }
            else
            {
                ProjectDescrition.Text = uForum.Library.Xslt.ResolveLinks(umbraco.library.ReplaceLineBreaks(Project.Description.Substring(0, 200)));
                LongProjectDescription.Text = uForum.Library.Xslt.ResolveLinks(umbraco.library.ReplaceLineBreaks(Project.Description.Substring(200)));
            }



            #endregion

            #region Screenshots
            //screenshots
            if (Project.ScreenShots.Count() > 0)
            {
                ScreenshotRepeater.DataSource = Project.ScreenShots;
                ScreenshotRepeater.DataBind();
            }
            else
            {
                ScreenshotsPanel.Visible = false;
            }

            #endregion

            #region Documentation
            //Documenation

            var docs = Project.DocumentationFile.Where(x => !x.Archived && x.FileType == FileType.docs).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                FileType = x.FileType.FileTypeAsString(),
                Vendor = vendorProvider.GetVendorById(x.CreatedBy),
            });

            ProjectDocRepeater.DataSource = docs;
            ProjectDocRepeater.DataBind();
            #endregion

            #region Sidebar

            //download box
            if (!string.IsNullOrEmpty(Project.CurrentReleaseFile))
            {
                var file = Project.PackageFile.Where(x => x.Id == Int32.Parse(Project.CurrentReleaseFile)).FirstOrDefault();
                if (file != null)
                {
                    ProjectCompatitbleWithUmbraco.Text = "TBI";//file.UmbVersion.Description;
                    ProjectCompatitbleWithDotNet.Text = file.DotNetVersion;
                    ProjectCompatitbleWithMediumTrust.Text = (file.SupportsMediumTrust) ? "Yes" : "No";
                }

            }

            DownloadCount.Text = Project.Downloads.ToString();
            
            //Commercial options
            PurchaseLink.NavigateUrl = "http://deli.umbraco.org" + umbraco.library.NiceUrl(Project.Id);

            if (Project.ListingType != ListingType.commercial)
            {
                CommercePanel.Visible = false;
            }


            }

            #endregion

        }



    }
