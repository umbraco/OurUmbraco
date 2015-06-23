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
using Marketplace.Providers.MediaFile;
using System.Text;
using Marketplace.Data;
using Marketplace.uVersion;


namespace Marketplace.usercontrols.Deli
{


    public partial class ProjectView : System.Web.UI.UserControl
    {

        public IListingItem Project { get; set; }
        private IListingProvider projectProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

        private Currency _currency = Currency.EUR;

        protected void Page_Load(object sender, EventArgs e)
        {
            Project = projectProvider.GetCurrentListing();
            if (!IsPostBack)
            {
                projectProvider.IncrementProjectViews(Project);
                SetSecurables();
                SetupProject();
            }
            SetupAnalytics();
        }

        private void SetupProject()
        {

            #region Memberbadge

            MemberPosts.Text = Project.Vendor.Member.ForumPosts.ToString();
            MemberKarma.Text = Project.Vendor.Member.ReputationCurrent.ToString();

            #endregion

            #region Standard Features

            ProjectName.Text = Project.Name;
            ProjectCreateDate.Text = String.Format("{0:D}", Project.CreateDate);
            ProjectCurrentVersion.Text = Project.CurrentVersion;
            VendorLink.Text = Project.Vendor.Member.Name;
            VendorLink.NavigateUrl = "/member/" + Project.Vendor.Member.Id;

            //description
            if (Project.Description.Contains('<'))
                ProjectDescrition.Text = uForum.Library.Xslt.ResolveLinks(Project.Description);
            else
                ProjectDescrition.Text = uForum.Library.Xslt.ResolveLinks(umbraco.library.ReplaceLineBreaks(Project.Description));

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
                ScreenshotRepeater.Visible = false;
            }

            #endregion

            #region Resources

            //resources
            var resouceFormat = "<li><a href=\"{0}\">{1}</a></li>";

            if (!string.IsNullOrEmpty(Project.ProjectUrl))
                ProjectResources.Text += string.Format(resouceFormat, Project.ProjectUrl, "Project Website");

            if(!string.IsNullOrEmpty(Project.DemonstrationUrl))
                ProjectResources.Text += string.Format(resouceFormat, Project.DemonstrationUrl, "View a Demonstration");

            if (!string.IsNullOrEmpty(Project.SourceCodeUrl))
                ProjectResources.Text += string.Format(resouceFormat, Project.SourceCodeUrl, "Download Source Code");

            if (!string.IsNullOrEmpty(Project.SupportUrl))
                ProjectResources.Text += string.Format(resouceFormat, Project.SupportUrl, "Support &amp; Issues");

            if (!string.IsNullOrEmpty(Project.LicenseUrl))
                ProjectResources.Text += string.Format(resouceFormat, Project.LicenseUrl, "Read the License");

            #endregion

            #region Files
            //files

            var vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];

            var files = Project.PackageFile.Where(x => !x.Archived).OrderByDescending(x => x.CreateDate).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                FileType = x.FileType.FileTypeAsString(),
                Member = new Member(x.CreatedBy),
                UmbracoVersion = x.UmbVersion.ToVersionNameString(), //x.UmbVersion.Description,
                DotNetVersion = x.DotNetVersion,
                MediumTrust = (x.SupportsMediumTrust)?"Supports Medium Trust":"Full Trust Required",
                CreateDate = x.CreateDate.ToShortDateString()

            });

            if (files.Count() > 0)
            {
                ProjectFileRepeater.DataSource = files;
                ProjectFileRepeater.DataBind();
            }
            else
            {
                ProjectFileRepeater.Visible = false;
            }

            var hotFixes = Project.HotFixes.Where(x => !x.Archived).OrderByDescending(x => x.CreateDate).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                FileType = x.FileType.FileTypeAsString(),
                Member = new Member(x.CreatedBy),
                UmbracoVersion = x.UmbVersion.ToVersionNameString(), //x.UmbVersion.Description,
                DotNetVersion = x.DotNetVersion,
                MediumTrust = (x.SupportsMediumTrust) ? "Supports Medium Trust" : "Full Trust Required",
                CreateDate = x.CreateDate.ToShortDateString()

            });

            if (hotFixes.Count() > 0)
            {
                HotFixRepeater.DataSource = hotFixes;
                HotFixRepeater.DataBind();
            }
            else
            {
                HotFixRepeater.Visible = false;
            }

            var sourceFiles = Project.SourceFile.Where(x => !x.Archived).OrderByDescending(x => x.CreateDate).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                FileType = x.FileType.FileTypeAsString(),
                Member = new Member(x.CreatedBy),
                UmbracoVersion = x.UmbVersion.ToVersionNameString(), // "TBI",//x.UmbVersion.Description,
                DotNetVersion = x.DotNetVersion,
                MediumTrust = (x.SupportsMediumTrust) ? "Supports Medium Trust" : "Full Trust Required",
                CreateDate = x.CreateDate.ToShortDateString()

            });

            if (sourceFiles.Count() > 0)
            {
                SourceRepeater.DataSource = sourceFiles;
                SourceRepeater.DataBind();
            }
            else
            {
                SourceRepeater.Visible = false;
            }


            #endregion 

            #region Documentation
            //Documenation
            
            var docs = Project.DocumentationFile.Where(x => !x.Archived).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                FileType = x.FileType.FileTypeAsString(),
                CreateDate = x.CreateDate,
                Member = new Member(x.CreatedBy)
            });


            if (docs.Count() > 0)
            {
                ProjectDocRepeater.DataSource = docs;
                ProjectDocRepeater.DataBind();
            }
            else
            {
                ProjectDocRepeater.Visible = false;
            }

            #endregion

            #region Archive

            //Archived Files
            var archive = new List<IMediaFile>();

            var packageArchive = Project.PackageFile.Where(x => x.Archived);
            var docArchive = Project.DocumentationFile.Where(x => x.Archived);

            archive.AddRange(packageArchive);
            archive.AddRange(docArchive);

            if (archive.Count() > 0)
            {
                ArchiveRepeater.DataSource = archive.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    FileType = x.FileType.FileTypeAsString(),
                    Member = new Member(x.CreatedBy),
                    UmbracoVersion = x.UmbVersion.ToVersionString(), //x.UmbVersion.Description,
                    DotNetVersion = x.DotNetVersion,
                    MediumTrust = (x.SupportsMediumTrust) ? "Supports Medium Trust" : "Full Trust Required",
                    CreateDate = x.CreateDate.ToShortDateString()
                });

                ArchiveRepeater.DataBind();
            }
            else
            {
                ArchiveRepeater.Visible = false;
            }

            #endregion

            #region Sidebar

            //download box
            if (!string.IsNullOrEmpty(Project.CurrentReleaseFile))
            {
                //ProjectNameDownload.Text = Project.Name;
                //ProjectCurrentVersionDownload.Text = Project.CurrentVersion;

                var file = Project.PackageFile.Where(x => x.Id == Int32.Parse(Project.CurrentReleaseFile)).FirstOrDefault();
                if (file != null)
                {

                    var uVersions = UVersion.GetAllVersions();

                    using (var ctx = new MarketplaceDataContext())
                    {
                        var count = 0;
                        var verCount = 0;
                        var ct = "This project has been reported to be compatible with ";

                        foreach (var ver in uVersions)
                        {
                            var reports = ctx.DeliVersionCompatibilities.Where(x => x.version == ver.Name && x.projectId == Project.Id);

                            if (reports.Count() > 0)
                            {
                                float compats = reports.Where(x => x.isCompatible).Count();
                                float numReps = reports.Count();
                                var perc = Convert.ToInt32(((compats / numReps) * 100));

                                if (perc >= 80)
                                {
                                    verCount++;
                                    if (count < 2)
                                    {
                                        count++;
                                        ct += ver.Name + ", ";
                                    }
                                }
                            }

                        }

                        if (count == 0)
                            ct = "No compatible versions have been reported, be the first!";
                        else
                        {
                            if (verCount > 2)
                            {
                                ct += "and " + (verCount - 2) + " other versions.";
                            }
                            else
                                ct = ct.Trim().TrimEnd(',');
                        }
                        ProjectCompatitbleWithUmbraco.Text = ct;
                    }


                    // file.UmbVersion.Description;

                    ////////////////////////////////////////////////////////////////
                    // moved to versionCompatibilityReport.cshtml for AJAXifCation//
                    ////////////////////////////////////////////////////////////////

                    //var compatDetails = "This project is compatible with the following versions as reported by community members who have downloaded this package:<br/><br/>";

                    //var uVersions = UVersion.GetAllVersions();

                    //using (var ctx = new MarketplaceDataContext())
                    //{
                    //    foreach (var ver in uVersions)
                    //    {
                    //        var reports = ctx.DeliVersionCompatibilities.Where(x => x.version == ver.Name && x.projectId == Project.Id);

                    //        if (reports.Count() > 0)
                    //        {
                    //            float compats = reports.Where(x => x.isCompatible).Count();
                    //            float numReps = reports.Count();
                    //            var perc = Convert.ToInt32(((compats / numReps) * 100));

                    //            var smiley = "unhappy";

                    //            if (perc >= 95)
                    //                smiley = "joyous";
                    //            else if (perc < 95 && perc >= 80)
                    //                smiley = "happy";
                    //            else if (perc < 80 && perc >= 65)
                    //                smiley = "neutral";
                    //            else if (perc < 65 && perc >= 50)
                    //                smiley = "unhappy";
                    //            else
                    //                smiley = "superUnhappy";

                    //            compatDetails += "<span class=\"smiley " + smiley + "\">" + ver.Name + " (" + perc + "%)</span>";
                    //        }
                    //        else
                    //            compatDetails += "<span class=\"smiley untested\">" + ver.Name + " (untested)</span>";
                    //    }
                    //}

                    //compatDetails += "<br/>";


                    var compatDetails = string.Empty;
                    if (!string.IsNullOrEmpty(file.DotNetVersion))
                        compatDetails += "<strong>.NET Version:</strong>" + file.DotNetVersion + "<br/>";

                    compatDetails += "<strong>Supports Medium Trust:</strong>";
                    compatDetails += (file.SupportsMediumTrust) ? "Yes" : "No";
                    compatDetails += "<br/>";
                    ProjectCompatibileDetails.Text = compatDetails;

                }

            }
            else
            {
                HasReports.Visible = false;
                DownloadPanel.Visible = false;
            }


            
            //Commercial options
            if (Project.ListingType != ListingType.commercial)
            {
                CommercePanel.Visible = false;
            }
            else
            {
                var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
                var licenses = licenseProvider.GetProjectLicenses(Project.ProjectGuid).Where(x => x.IsActive);
                ProjectPurchaseRepeater.DataSource = licenses;
                ProjectPurchaseRepeater.DataBind();
            }

            //Collab
            CollabPanel.Visible = Project.OpenForCollab;



            //package info panel

            //project contributors

            using (var ctx = new MarketplaceDataContext())
            {
                var contributors = ctx.projectContributors.Where(x => x.projectId == Project.Id)
                    .Select(x => new Member(x.memberId)).ToList();


                if (contributors.Count() > 0)
                {
                    ContribRepeater.DataSource = contributors;
                    ContribRepeater.DataBind();
                }
            }

            //Tags

            TagRepeater.DataSource = Project.Tags;
            TagRepeater.DataBind();

            if (TagEditPanel.Visible)
            {
                TagEditRepeater.DataSource = Project.Tags;
                TagEditRepeater.DataBind();

                string taglist = string.Empty;
                var tagProvider = ((IProjectTagProvider)MarketplaceProviderManager.Providers["TagProvider"]);
                var tags = tagProvider.GetAllTags();
                foreach (var t in tags)
                {
                    taglist += "\"" + t.Text + "\",";
                }

                TagStringArray.Text = taglist;
            }

            //Download Counter

            DownloadCount.Text = Project.Downloads.ToString();

            #endregion

        }

        protected void SetupAnalytics()
        {
            if (!string.IsNullOrEmpty(Project.GACode))
            {

                var eventCategory = "_" + Project.Name.ToLower().Replace(" ", "");
                var analyticsSB = new StringBuilder();

                analyticsSB.Append("<script type=\"text/javascript\">");
                analyticsSB.Append("var vendorTracker = _gat._getTracker(\"" + Project.GACode + "\");");
                analyticsSB.Append("vendorTracker._initData();");
                if (!IsPostBack)
                {
                    analyticsSB.Append("vendorTracker._trackPageview();");
                }
                //analyticsSB.Append("vendorTracker._trackEvent('" + eventCategory + "', 'View', '" +  Project.Name + "');");
                analyticsSB.Append("</script>");


                // need a better way to do this.   For some reason have to walk to the top of the tree then down to find it.
                Literal holder = Page.Master.Master.Master.Controls[0].FindControl("EndScripts").FindControl("EndScriptsLit") as Literal;
                holder.Text = analyticsSB.ToString();
                
            }
        }

        protected void LicenseBound(Object sender, RepeaterItemEventArgs e)
        {
            var cartProvider = (ICartProvider)MarketplaceProviderManager.Providers["CartProvider"];
            ICart _cart = cartProvider.GetCurrentCart();
            _currency = _cart.Currency;

            Literal _name = (Literal)e.Item.FindControl("LicenseType");
            Literal _price = (Literal)e.Item.FindControl("LicensePrice");
            ImageButton _addToCart = (ImageButton)e.Item.FindControl("LicenseAddToCart");

            var License = (ILicense)e.Item.DataItem;

            switch (License.LicenseType)
                {
                    case LicenseType.Domain:
                        _name.Text = "Domain";
                        break;
                    case LicenseType.IP:
                        _name.Text = "IP";
                        break;
                    case LicenseType.Unlimited:
                        _name.Text = "Unlimited";
                        break;
                    case LicenseType.SourceCode:
                        _name.Text = "Source Code";
                        break;
                    default:
                        break;
                }

            _price.Text = DeliCurrency.FriendlyConvertFromEuro((decimal)License.Price, DeliCurrency.FromSymbol(_currency.ToString()));
            _addToCart.CommandArgument = License.Id.ToString();

            if (!String.IsNullOrEmpty(Project.GACode))
            {
                var eventCategory = "_" + Project.Name.ToLower().Replace(" ", "");
                _addToCart.OnClientClick = "vendorTracker._trackEvent('" + eventCategory + "', 'Add To Cart - " + _name.Text + "', '" + Project.Name + " " + _name.Text + "'," + License.Price + ");";
            }


        }

        protected void _addToCart_Click(object sender, EventArgs e)
        {
            var button = (ImageButton)sender;
            var license =  ((ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"]).GetLicense(Int32.Parse(button.CommandArgument));
            var cartProvider = (ICartProvider)MarketplaceProviderManager.Providers["CartProvider"];

            var cartItem = new CartItem()
            {
                LicenseId = Int32.Parse(button.CommandArgument),
                ListingItem = Project,
                Quantity = 1,
                GrossPrice = license.Price

            };
            
            cartProvider.AddItem(cartItem);

            //redirect to the cart after adding item.
            Response.Redirect("~/deli/cart?returnUrl=" + Server.UrlEncode(Request.RawUrl));
            
        }

        private void SetSecurables()
        {
            var memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            var mem = memberProvider.GetCurrentMember();
            var isVendorAuthenticated = umbraco.library.IsLoggedOn() && (Project.Vendor.Member.Id == mem.Id || our.Utills.IsProjectContributor(mem.Id,Project.Id));
            EditOption.Visible = isVendorAuthenticated;
            TagEditPanel.Visible = isVendorAuthenticated;


            //ReportVersionOptions.Visible = !isVendorAuthenticated && umbraco.library.IsLoggedOn() && library.HasDownloaded(mem.Id, Project.Id);
            VotingOptions.Visible = !isVendorAuthenticated && umbraco.library.IsLoggedOn() && uPowers.Library.Xslt.YourVote(mem.Id,Project.Id,"powersProject") == 0; // show voting only if the member is logged on and is not associated with the project in some way;

            if (mem != null)
            {
                AdminVotingOptions.Visible = (mem.Groups.Contains("admin") && Project.Karma < 15);
            }
            else
            {
                AdminVotingOptions.Visible = false;
            }


        }

        protected string IsVoteable()
        {
            var mem = Member.GetCurrentMember();
            
            if (mem == null || (int)mem.getProperty("reputationCurrent").Value <= 69)
            {
                return " noVote";
            }
            return "";
        }
    }
}