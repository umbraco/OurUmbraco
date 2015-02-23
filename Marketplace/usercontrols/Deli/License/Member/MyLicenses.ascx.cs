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

namespace Marketplace.usercontrols.Deli.License.Member
{
    public partial class MyLicenses : System.Web.UI.UserControl
    {

        public int SupportFormLocation { get; set; }

        protected string supportFormUrl;
        private IMemberProvider _memberProvider;
        private IMemberLicenseProvider _memberLicenseProvider;
        private ILicenseProvider _licenseProvider;
        private IListingProvider _listingProvider;
        private IMember _m;



        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            RP_Licenses.ItemDataBound += new RepeaterItemEventHandler(OnLicenseBound);
            RP_Licenses.ItemCommand += new RepeaterCommandEventHandler(OnLicenseCommand);
            _listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            _licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            _memberLicenseProvider= (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
            _m = _memberProvider.GetCurrentMember();

            //setup the location of the support form
            supportFormUrl = umbraco.library.NiceUrl(SupportFormLocation);


        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);




                if (!IsPostBack)
                {
                    BindLicenses(string.Empty);
                }


        }

        private void BindLicenses(string term)
        {
            var licenses = (!string.IsNullOrEmpty(term))
                ? _memberLicenseProvider.GetAllLicensesContaining(_m.Id, term)
                : _memberLicenseProvider.GetAllLicensesByMember(_m.Id);
            

            var memberLicenses = licenses.Select(x => new 
            {
                Id = x.Id,
                Product = GetProduct(x.LicenseId),
                LicType = _licenseProvider.GetLicense(x.LicenseId).LicenseType.LicenseTypeAsString(),
                DevConfig= x.DevConfig,
                StagingConfig = x.StagingConfig,
                ProductionConfig = x.ProductionConfig,
                Lic = x
            });

            RP_Licenses.DataSource = memberLicenses;
            RP_Licenses.DataBind();
        }


        private IListingItem GetProduct(int p)
        {
            var product = _listingProvider.GetListing(_licenseProvider.GetLicense(p).ProjectGuid, false);
            return product;
        }

        protected void OnLicenseBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                dynamic row = e.Item.DataItem as dynamic;
                var lic = row.Lic;
                Button _action = (Button)e.Item.FindControl("BT_Action");
                LinkButton _download = (LinkButton)e.Item.FindControl("LNK_Download");

                _download.CommandArgument = lic.Id.ToString();
                _download.CommandName = "Download";


                //set up the configure and download buttons
                _action.CommandArgument = lic.Id.ToString();

                if (lic.DateGenerated != null)
                {
                    _action.Visible = false;
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

                    var lic = _memberLicenseProvider.GetLicenseById(Int32.Parse(licenseId));
                    var licType = _licenseProvider.GetLicense(lic.LicenseId);
                    var licenseTypeProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
                    switch (e.CommandName)
                    {


                        case "Configure":
                            Create_LicenseError.Visible = false;
                            ConfigurationForm.Visible = true;


                            ConfigurationLicenseType.Text = licType.LicenseType.ToString() + " License ";
                            ConfigurationMemberName.Text = lic.Member.Name;
                            ConfigurationProjectName.Text = GetProduct(lic.LicenseId).Name;

                            if (licType.LicenseType != LicenseType.Unlimited)
                            {
                                Configure_DevConfig.Text = lic.DevConfig;
                                Configure_StagingConfig.Text = lic.StagingConfig;
                                Configure_ProductionConfig.Text = lic.ProductionConfig;

                                //set the validation class for the fields to validate correct setup.
                                switch (licType.LicenseType)
                                {
                                    case LicenseType.Domain:
                                        Configure_DevConfig.CssClass += " onlyDomainName";
                                        Configure_StagingConfig.CssClass += " onlyDomainName";
                                        Configure_ProductionConfig.CssClass += " onlyDomainName";
                                        break;
                                    case LicenseType.IP:
                                        Configure_DevConfig.CssClass += " onlyIpAddress";
                                        Configure_StagingConfig.CssClass += " onlyIpAddress";
                                        Configure_ProductionConfig.CssClass += " onlyIpAddress";
                                        break;
                                    case LicenseType.Unlimited:
                                        break;
                                    case LicenseType.SourceCode:
                                        break;
                                    default:
                                        break;
                                }

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


                        case "Download":
                            ConfigurationForm.Visible = false;
                            Create_LicenseError.Visible = false;
                            var project = _listingProvider.GetListing(_licenseProvider.GetLicense(lic.LicenseId).ProjectGuid);
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


        protected void UpdateLicense_Click(object server, EventArgs e)
        {
            Create_LicenseError.Visible = false;

            if (Configure_DevConfig.Text.Contains("*") || Configure_StagingConfig.Text.Contains("*") || Configure_ProductionConfig.Text.Contains("*"))
            {
                LicenseErrorMessage.Text = " Wildcards are not allowed in domains, they are automatically added; e.g. *.mydomain.com.  Please remove any '*' characters are retry";
                Create_LicenseError.Visible = true;
            }
            else
            {

                var licenseId = Int32.Parse(Configure_Id.Value);
                var license = _memberLicenseProvider.GetLicenseById(licenseId);

                var licenseType = _licenseProvider.GetLicense(license.LicenseId);

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

                _memberLicenseProvider.SaveOrUpdate(license);
                _memberLicenseProvider.GenerateLicenseFile(license);

                if (string.IsNullOrEmpty(license.GeneratedLicense))
                {
                    LicenseErrorMessage.Text = license.GeneratedLicense + " Please contact the license vendor for assistance";
                    Create_LicenseError.Visible = true;
                }
                else
                {
                    Response.Redirect(Request.Url.OriginalString);
                }
            }
        }

        protected void Find_LicenseSubmit_Click(object sender, EventArgs e)
        {
            var term = Find_LicenseTerm.Text;
            Find_Filter.Text = Find_LicenseTerm.Text;
            FilteringByMessage.Visible = true;
            Create_LicenseError.Visible = false;
            ConfigurationForm.Visible = false;
            BindLicenses(term);
            Find_RecordsFound.Text = RP_Licenses.Items.Count.ToString();

        }
    }
}