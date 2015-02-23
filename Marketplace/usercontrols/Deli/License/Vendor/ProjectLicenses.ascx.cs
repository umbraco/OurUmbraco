using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using umbraco.cms.businesslogic.member;
using Marketplace.Providers.Helpers;
using our;
using Marketplace.Providers.MemberLicense;
using System.Text.RegularExpressions;

namespace Marketplace.usercontrols.Deli.License.Vendor
{
    public partial class ProjectLicenses : System.Web.UI.UserControl
    {
        private Guid _projectGuid;

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
            base.OnInit(e);

            RP_Licenses.ItemDataBound += new RepeaterItemEventHandler(OnLicenseBound);
            RP_Licenses.ItemCommand += new RepeaterCommandEventHandler(OnLicenseCommand);

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (umbraco.library.IsLoggedOn() && ProjectId != null)
            {
                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                var memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
                IMember mem = memberProvider.GetCurrentMember();
                IListingItem project = provider.GetListing((int)ProjectId);

                ProjectName.Text = project.Name;
                _projectGuid = project.ProjectGuid;

                if ((project.Vendor.Member.Id == mem.Id) ||
                    Utills.IsProjectContributor(mem.Id, (int)ProjectId))
                {
                    if (!IsPostBack)
                    {
                        BindLicenses(string.Empty);
                        BindDropdowns();
                    }
                }
                else
                {
                    //this project does not belong to this member so kick them back to the project list in their profile.
                    Response.Redirect("~/member/profile/projects/");
                }
            }
        }

        private void BindLicenses(string term)
        {
            var licenseTypeProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var licenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];

            var licenses = (!string.IsNullOrEmpty(term))
                ?licenseProvider.GetAllLicensesContaining(term, (int)ProjectId)
                :licenseProvider.GetAllLicensesByProject(_projectGuid);
            
            var memberLicenses = licenses.Select(x => new 
            {
                Id = x.Id,
                LicType = licenseTypeProvider.GetLicense(x.LicenseId).LicenseType.LicenseTypeAsString(),
                DevConfig= x.DevConfig,
                StagingConfig = x.StagingConfig,
                ProductionConfig = x.ProductionConfig,
                MemberName = x.Member.Name + " (" + x.Member.Email + ")",
                Lic = x
            });

            RP_Licenses.DataSource = memberLicenses;
            RP_Licenses.DataBind();
        }

        protected void OnLicenseBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                dynamic row = e.Item.DataItem as dynamic;
                var lic = row.Lic;
                Button _action = (Button)e.Item.FindControl("BT_Action");
                Button _enable = (Button)e.Item.FindControl("BT_EnableDisable");
                LinkButton _download = (LinkButton)e.Item.FindControl("LNK_Download");

                _download.CommandArgument = lic.Id.ToString();
                _download.CommandName = "Download";

                //setup the enable disable button
                _enable.CommandArgument = lic.Id.ToString();
                _enable.CommandName = "EnableDisable";
                _enable.Text = lic.IsActive ? "Disable" : "Enable";


                //set up the configure and download buttons
                _action.CommandArgument = lic.Id.ToString();

                if (lic.DateGenerated != null)
                {
                    _action.CommandName = "Configure";
                    _action.CommandArgument = lic.Id.ToString();
                    _action.Text = "Configure";
                }
                else
                {
                    _action.CommandName = "Configure";
                    _action.Text = "Configure";
                    _action.CommandArgument = lic.Id.ToString();
                    _download.Visible = false;
                }

            }
        }

        protected void OnLicenseCommand(object sender, RepeaterCommandEventArgs e)
        {                    
            //load the license
                    string licenseId = e.CommandArgument.ToString();
                    var licenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
                    var lic = licenseProvider.GetLicenseById(Int32.Parse(licenseId));

                    var licenseTypeProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
                    switch (e.CommandName)
                    {


                        case "Configure":



                            //get its type to set the headings 
                            var licenseType = licenseTypeProvider.GetLicense(lic.LicenseId);
                            var licType = licenseType.LicenseType.LicenseTypeAsString();

                            Create_LicenseError.Visible = false;
                            CreateNewForm.Visible = false;
                            ConfigurationForm.Visible = true;
                            CreateNew.Visible = true;


                            ConfigurationLicenseType.Text = licType + " License ";
                            ConfigurationMemberName.Text = lic.Member.Name;
                            ConfigurationProjectName.Text = ProjectName.Text;

                            if (licenseType.LicenseType != LicenseType.Unlimited)
                            {
                                Configure_DevConfig.Text = lic.DevConfig;
                                Configure_StagingConfig.Text = lic.StagingConfig;
                                Configure_ProductionConfig.Text = lic.ProductionConfig;
                                DomainIpConfig.Visible = true;
                                UnlimitedConfig.Visible = false;
                            }
                            else
                            {
                                DomainIpConfig.Visible = false;
                                UnlimitedConfig.Visible = true;
                            }
                            Configure_Id.Value = licenseId;
                            break;

                        case "EnableDisable":

                            ConfigurationForm.Visible = false;
                            Create_LicenseError.Visible = false;
                            CreateNewForm.Visible = false;
                            CreateNew.Visible = true;

                            lic.IsActive = lic.IsActive ? false : true;
                            licenseProvider.SaveOrUpdate(lic);
                            break;

                        case "Download":

                            ConfigurationForm.Visible = false;
                            Create_LicenseError.Visible = false;
                            CreateNewForm.Visible = false;
                            CreateNew.Visible = true;

                            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                            var project = provider.GetListing(licenseTypeProvider.GetLicense(lic.LicenseId).ProjectGuid);
                            var filename = Regex.Replace(project.Vendor.VendorCompanyName, @"[\W]", "");
                            filename += Regex.Replace(project.Name, @"[\W]", "");
                            HttpContext context = HttpContext.Current;
                            context.Response.Clear();
                            context.Response.Write(lic.GeneratedLicense);
                            context.Response.ContentType = "application/octet-steam";
                            context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + filename + ".lic");
                            context.Response.End();
                            break;

                        default:
                            break;
                    }
        }


        private void BindDropdowns()
        {
            var licenseTypeProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            var licTypes = licenseTypeProvider.GetProjectLicenses(_projectGuid).Select(x => new
            {
                Text = x.LicenseType.LicenseTypeAsString(),
                Value = x.Id
            });

            Create_LicenseType.DataTextField = "Text";
            Create_LicenseType.DataValueField = "Value";
            Create_LicenseType.DataSource = licTypes;
            Create_LicenseType.DataBind();


        }

        protected void CreateNew_Click(object sender, EventArgs e)
        {
            CreateNew.Visible = false;
            CreateNewForm.Visible = true;
            ConfigurationForm.Visible = false;
            if (Create_LicenseType.SelectedItem.Text.ToLower() == "unlimited" || Create_LicenseType.SelectedItem.Text.ToLower() == "sourcecode")
            {
                CreateUnlimitedConfig.Visible = true;
                CreateDomainIpConfig.Visible = false;
            }
            else
            {
                CreateUnlimitedConfig.Visible = false;
                CreateDomainIpConfig.Visible = true;
            }
        }


        protected void UpdateLicense_Click(object server, EventArgs e)
        {
            Create_LicenseError.Visible = false;
            CreateNew.Visible = true;

            var licenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
            var licenseId = Int32.Parse(Configure_Id.Value);
            var license = licenseProvider.GetLicenseById(licenseId);

            var licenseTypeProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var licenseType = licenseTypeProvider.GetLicense(license.LicenseId);

            if (licenseType.LicenseType != LicenseType.Unlimited)
            {
                license.DevConfig = Configure_DevConfig.Text;
                license.StagingConfig = Configure_StagingConfig.Text;
                license.ProductionConfig = Configure_ProductionConfig.Text;
            }
            else
            {
                license.DevConfig = "Unlimited";
                license.StagingConfig = "Unlimited";
                license.ProductionConfig = "Unlimited";
            }
            license.IsActive = true;

            licenseProvider.SaveOrUpdate(license);
            licenseProvider.GenerateLicenseFile(license);

            if (string.IsNullOrEmpty(license.GeneratedLicense))
            {
                LicenseErrorMessage.Text = license.GeneratedLicense + " Please contact support for assistance";
                Create_LicenseError.Visible = true;
            }
            else
            {
                Response.Redirect(Request.Url.OriginalString);
            }
        }

        protected void CreateTypeChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ddl.SelectedItem.Text.ToLower() == "unlimited" || ddl.SelectedItem.Text.ToLower() == "sourcecode")
            {
                CreateUnlimitedConfig.Visible = true;
                CreateDomainIpConfig.Visible = false;
            }
            else
            {
                CreateUnlimitedConfig.Visible = false;
                CreateDomainIpConfig.Visible = true;
            }
        }

        protected void CreateLicense_Click(object server, EventArgs e)
        {
            Create_LicenseError.Visible = false;

            var memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            var licenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
            var licenseTypeProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            var license = new MemberLicense();

            //bit nasty but I cant think of a better way to catch an invalid member.
            try { 
            var member = memberProvider.GetMemberByEmail(Create_MemberEmail.Text);
                license.Member = member;
                license.LicenseId = Convert.ToInt32(Create_LicenseType.SelectedValue);
                license.ListingItemId = (int)ProjectId;

                var licenseType = licenseTypeProvider.GetLicense(license.LicenseId);

                if (licenseType.LicenseType != LicenseType.Unlimited)
                {
                    license.DevConfig = Create_DevConfig.Text;
                    license.StagingConfig = Create_StagingConfig.Text;
                    license.ProductionConfig = Create_ProductionConfig.Text;
                }
                else
                {
                    license.DevConfig = "Unlimited";
                    license.StagingConfig = "Unlimited";
                    license.ProductionConfig = "Unlimited";
                }
                license.CreateDate = DateTime.Now.ToUniversalTime();
                license.IsActive = true;

                licenseProvider.SaveOrUpdate(license);
                licenseProvider.GenerateLicenseFile(license);
                if (string.IsNullOrEmpty(license.GeneratedLicense))
                {
                    LicenseErrorMessage.Text = license.GeneratedLicense + " Please contact support for assistance";
                    Create_LicenseError.Visible = true;
                }
                else
                {
                    Create_LicenseError.Visible = false;
                    Create_InvalidMember.Visible = false;
                    Response.Redirect(Request.Url.OriginalString);
                }
            }
            catch
            {
                Create_InvalidMember.Visible = true;
            }
        }

        protected void Find_LicenseSubmit_Click(object sender, EventArgs e)
        {
            var term = Find_LicenseTerm.Text;
            Find_Filter.Text = Find_LicenseTerm.Text;
            FilteringByMessage.Visible = true;
            Create_LicenseError.Visible = false;
            CreateNew.Visible = true;
            ConfigurationForm.Visible = false;
            CreateNewForm.Visible = false;
            BindLicenses(term);
            Find_RecordsFound.Text = RP_Licenses.Items.Count.ToString();

        }
    }
}