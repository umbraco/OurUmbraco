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
using Marketplace.Providers.License;
using Marketplace.Providers.Helpers;
using Marketplace.Helpers;
using Marketplace.Providers.Accounting;
using System.Text.RegularExpressions;

namespace Marketplace.usercontrols.Deli.Package.Steps
{
    public partial class Licenses : System.Web.UI.UserControl
    {
        private Guid _projectGuid;


        private bool _editMode
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["projectId"]))
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


        private void ReBindLicenses()
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var licenses = licenseProvider.GetProjectLicenses(_projectGuid);
            rp_licenses.DataSource = licenses;
            rp_licenses.Visible = (licenses.Count() > 0);
            rp_licenses.DataBind();
        }

        protected void EditLicense(object sender, CommandEventArgs e)
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var license = licenseProvider.GetLicense(int.Parse(e.CommandArgument.ToString()));
            EditForm.Visible = true;
            CreateForm.Visible = false;
            EditLicenseType.Text = license.LicenseType.ToString();
            EditLicensePrice.Text = license.Price.ToString();
            UpdateLicense.CommandArgument = license.Id.ToString();
        }

        protected void UpdateLicense_Click(object sender, EventArgs e)
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var updateButton = sender as Button;
            var licenseId = Int32.Parse(updateButton.CommandArgument);

            var license = licenseProvider.GetLicense(licenseId);

            license.Price = Double.Parse(EditLicensePrice.Text);
            licenseProvider.SaveOrUpdate(license);
            EditForm.Visible = false;
            CreateForm.Visible = true;

            ReBindLicenses();

        }


        protected void DeleteLicense(object sender, CommandEventArgs e)
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var license = licenseProvider.GetLicense(int.Parse(e.CommandArgument.ToString()));

            var memberLicenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
            if (memberLicenseProvider.GetLicensesByLicenseTypeId(license.Id).Count() == 0)
            {
                licenseProvider.Remove(license);
            }

            //var mem = Member.GetCurrentMember();
            //if (license.CreatedBy == mem.Id || Utills.IsProjectContributor(mem.Id, (int)ProjectId))



            ReBindLicenses();
        }

        protected void DisableEnableLicense(object sender, CommandEventArgs e)
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            var license = licenseProvider.GetLicense(int.Parse(e.CommandArgument.ToString()));
            
            license.IsActive = (license.IsActive) ? false : true;

            licenseProvider.SaveOrUpdate(license);

            ReBindLicenses();
        }

        protected void OnLicenseBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {

                ILicense lic = (ILicense)e.Item.DataItem;
                Button _delete = (Button)e.Item.FindControl("bt_delete");
                Button _edit = (Button)e.Item.FindControl("bt_edit");
                Button _disable = (Button)e.Item.FindControl("bt_disableEnable");
                Literal _type = (Literal)e.Item.FindControl("lt_type");
                Literal _price = (Literal)e.Item.FindControl("lt_price");

                switch (lic.LicenseType)
                {
                    case LicenseType.Domain:
                        _type.Text = "Domain";
                        break;
                    case LicenseType.IP:
                        _type.Text = "IP";
                        break;
                    case LicenseType.Unlimited:
                        _type.Text = "Unlimited";
                        break;
                    case LicenseType.SourceCode:
                        _type.Text = "Source Code";
                        break;
                    default:
                        break;
                }

                _price.Text = DeliCurrency.NiceMoney((decimal)lic.Price, DeliCurrency.FromSymbol("EUR"));

                _delete.CommandArgument = lic.Id.ToString();
                _disable.CommandArgument = lic.Id.ToString();
                _edit.CommandArgument = lic.Id.ToString();

                if (lic.IsActive)
                {
                    _disable.Text = "Disable";
                }
                else
                {
                    _disable.Text = "Enable";
                }

                var memberLicenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
                if (memberLicenseProvider.GetLicensesByLicenseTypeId(lic.Id).Count() == 0)
                {
                    _delete.Visible = true;
                }

            }
        }



        protected void SaveLicense_Click(object sender, EventArgs e)
        {
            var lic = new Marketplace.Providers.License.License();
            lic.LicenseType = (LicenseType)Enum.Parse(typeof(LicenseType), (string)licenseTypes.SelectedValue, true);
            lic.Price = double.Parse(price.Text);
            lic.ProjectGuid = _projectGuid;

            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            licenseProvider.SaveOrUpdate(lic);

            ReBindLicenses();

        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (umbraco.library.IsLoggedOn() && ProjectId != null)
            {
                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                Member mem = Member.GetCurrentMember();
                IListingItem project = provider.GetListing((int)ProjectId);

                DownloadKeyPlaceHolder.Visible = !string.IsNullOrEmpty(project.LicenseKey);
                GenKeyPlaceHolder.Visible = string.IsNullOrEmpty(project.LicenseKey);

                _projectGuid = project.ProjectGuid;

                if ((project.Vendor.Member.Id == mem.Id) ||
                    Utills.IsProjectContributor(mem.Id, (int)ProjectId))
                {
                    holder.Visible = true;
                    ReBindLicenses();
                    uniqueId.Text = _projectGuid.ToString();
                }
            }
        }

        protected void GenKey_Click(object sender, EventArgs e)
        {
            GenKeyPlaceHolder.Visible = false;
            DownloadKeyPlaceHolder.Visible = true;

            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var project = provider.GetListing(_projectGuid);

            var licenseGenerator = new Marketplace.Providers.MemberLicense.MemberLicenseGenerator();
            var projectName = Regex.Replace(project.Vendor.VendorCompanyName, @"[\W]", "")
                            + Regex.Replace(project.Name, @"[\W]", "");
            project.LicenseKey = licenseGenerator.GenerateXMLKey(projectName.ToLower(), project.ProjectGuid.ToString());
            provider.SaveOrUpdate(project);
        }

        protected void DownloadKey_Click(object sender, EventArgs e)
        {
            var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var project = provider.GetListing(_projectGuid);

            if (!string.IsNullOrEmpty(project.LicenseKey))
            {
                var licenseGenerator = new Marketplace.Providers.MemberLicense.MemberLicenseGenerator();
                var licenseKey = licenseGenerator.GenerateVendorKey(project.LicenseKey);

                // the license key file name 
                var projectName = Regex.Replace(project.Vendor.VendorCompanyName, @"[\W]", "")
                                + Regex.Replace(project.Name, @"[\W]", "");

                HttpContext context = HttpContext.Current;
                context.Response.Clear();
                context.Response.Write(licenseKey);
                context.Response.ContentType = "text/xml";
                context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + projectName.ToLower() + ".ils");
                context.Response.End();
            }
        }

        protected void SaveStep(object sender, EventArgs e)
        {
            //move to the license step
            ProjectCreatorHelper.MoveToNextStep(this, (int)ProjectId);
        }

        protected void MoveLast(object sender, EventArgs e)
        {
            //move to the screenshots step
            ProjectCreatorHelper.MoveToPreviousStep(this, (int)ProjectId);
        }
    }
}