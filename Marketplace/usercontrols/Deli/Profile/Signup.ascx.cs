using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.member;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Members;
using System.Web.UI.HtmlControls;

namespace Marketplace.usercontrols.Deli.Profile
{
    public partial class Signup : System.Web.UI.UserControl {

        public string Group { get; set; }
        public string memberType { get; set; }
        public int NextPage { get; set; }

        private IMemberProvider mp;
        private IMember m;

        protected void Page_Init(object sender,EventArgs e){
             mp = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
             m = mp.GetCurrentMember();

             if (m == null || !m.IsDeliVendor)
             {
                 ProfileNavigation.Visible = false;
             }
        }


        protected void Page_Load(object sender, EventArgs e) {
            //lazyloading the needed javascript for validation. (addded it to the master template as our ahah forms need it aswel)
            //umbraco.library.RegisterJavaScriptFile("jquery.validation", "/scripts/jquery.validation.js");

            if(!Page.IsPostBack){
                ((HtmlGenericControl)profileNav.Parent).Attributes.Add("class","current");
            }


            if (!Page.IsPostBack && m != null) {
                tb_name.Text = m.Name;
                tb_email.Text = m.Email;

                //make sure that it is not required to enter the password..
                tb_password.CssClass = "title";

                //hack on the save button...
                bt_submit.Text = "Save";

                //treshold and newsletter

                cb_bugMeNot.Checked = m.BugMeNot;


                tb_treshold.Text = m.Threshold.ToString();

                //optional.. 
                tb_twitter.Text = m.Twitter;
                tb_flickr.Text = m.Flickr;
                tb_company.Text = m.Company;
                tb_bio.Text = m.Profile;

                //Location
                tb_lat.Value = m.Latitude;
                tb_lng.Value = m.Longitude;
                tb_location.Text = m.Location;

                //billing details
                tb_companyAddress.Text = m.CompanyAddress;
                dd_companyCountry.SelectedValue = m.CompanyCountry;
                tb_companyBillingEmail.Text = m.CompanyInvoiceEmail;
                tb_companyVatNumber.Text = m.CompanyVATNumber;

                if (m.IsDeliVendor)
                {
                    var vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
                    var vendor = vendorProvider.GetVendorById(m.Id);


                    tb_vendorCompany.Text = vendor.VendorCompanyName;
                    dd_vendorCountry.SelectedValue = vendor.VendorCountry;
                    tb_vendorDescription.Text = vendor.VendorDescription;

                    tb_vendorUrl.Text = vendor.VendorUrl;
                    tb_vendorSupportUrl.Text = vendor.SupportUrl;
                    tb_vendorBillingEmail.Text = vendor.BillingContactEmail;
                    tb_vendorSupportEmail.Text = vendor.SupportContactEmail;
                    tb_vendorIban.Text = vendor.IBAN;
                    tb_vendorSwift.Text = vendor.SWIFT;
                    tb_vendorBsb.Text = vendor.BSB;
                    tb_vendorAccount.Text = vendor.AccountNumber;
                    tb_vendorPayPal.Text = vendor.PayPalAccount;
                    tb_vendorBaseCurrency.Text = vendor.BaseCurrency;
                    tb_vendorTaxId.Text = vendor.TaxId;
                    tb_vendorVatNumber.Text = vendor.VATNumber;

                    //check to see if the box is checked.  If it is dont allow it to be rechecked
                    if (vendor.DeliTermsAgreementDate != DateTime.MinValue && vendor.DeliTermsAgreementDate != null)
                    {
                        cb_vendorTerms.Checked = true;
                        cb_vendorTerms.Enabled = false;
                    }

                }

            }

        }

        protected void createMember(object sender, EventArgs e) {

            //Member is already logged in, and we just need to save his new data...
            if (m != null) {
                m.Name = tb_name.Text;
                m.Email = tb_email.Text;
                
                if (tb_password.Text != "")
                    m.Password = tb_password.Text;

                //optional.. 
                m.Twitter = tb_twitter.Text;
                m.Flickr = tb_flickr.Text;
                m.Company = tb_company.Text;
                m.Profile = tb_bio.Text;

                //location
                m.Location = tb_location.Text;
                m.Latitude = tb_lat.Value;
                m.Longitude = tb_lng.Value;

                //billing info
                m.CompanyAddress = tb_companyAddress.Text;
                m.CompanyCountry = dd_companyCountry.SelectedValue;
                m.CompanyInvoiceEmail = tb_companyBillingEmail.Text;
                m.CompanyVATNumber = tb_companyVatNumber.Text;

                var cartProvider = (ICartProvider)MarketplaceProviderManager.Providers["CartProvider"];
                if (m.CompanyCountry.ToLower() != null)
                {
                    switch (m.CompanyCountry)
                    {
                        case "dk":
                            cartProvider.SetCurrency(Currency.DKK);
                            break;

                        case "us":
                        case "ca":
                            cartProvider.SetCurrency(Currency.USD);
                            break;

                        default:
                            cartProvider.SetCurrency(Currency.EUR);
                            break;
                    }
                }
                

                //treshold + newsletter
                m.Threshold = Int32.Parse(tb_treshold.Text);
                m.BugMeNot = cb_bugMeNot.Checked;

                mp.SaveOrUpdate(m);

                Response.Redirect(umbraco.library.NiceUrl(NextPage));

            } else {
                if (tb_email.Text != "") {
                    m = mp.GetMemberByEmail(tb_email.Text);
                    if (m == null) {

                        m = new DeliMember();

                        m.Name = tb_name.Text;
                        m.Email = tb_email.Text;
                        m.Password = tb_password.Text;


                        //Location
                        m.Location = tb_location.Text;
                        m.Latitude = tb_lat.Value;
                        m.Longitude = tb_lng.Value;

                        //billing info
                        m.CompanyAddress = tb_companyAddress.Text;
                        m.CompanyCountry = dd_companyCountry.SelectedValue;
                        m.CompanyInvoiceEmail = tb_companyBillingEmail.Text;
                        m.CompanyVATNumber = tb_companyVatNumber.Text;


                        var cartProvider = (ICartProvider)MarketplaceProviderManager.Providers["CartProvider"];
                        if (m.CompanyCountry.ToLower() != null)
                        {
                            switch (m.CompanyCountry)
                            {
                                case "dk":
                                    cartProvider.SetCurrency(Currency.DKK);
                                    break;

                                case "us":
                                case "ca":
                                    cartProvider.SetCurrency(Currency.USD);
                                    break;

                                default:
                                    cartProvider.SetCurrency(Currency.EUR);
                                    break;
                            }
                        }

                        //optional.. 
                        m.Twitter = tb_twitter.Text;
                        m.Flickr = tb_flickr.Text;
                        m.Company = tb_company.Text;
                        m.Profile = tb_bio.Text;
                        
                        //treshold + newsletter
                        m.Threshold = Int32.Parse(tb_treshold.Text);
                        m.BugMeNot = cb_bugMeNot.Checked;
                        
                        //Standard values
                        m.ReputationTotal = 20;
                        m.ReputationCurrent = 20;
                        m.ForumPosts = 0;

                        //add the group
                        m.Groups = new string[] { Group };

                        mp.SaveOrUpdate(m);

                        Response.Redirect(umbraco.library.NiceUrl(NextPage));
                    }
                }
            }
        }

        protected void updateVendor(object sender, EventArgs e)
        {
            var vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            var vendor = vendorProvider.GetVendorById(m.Id);

            vendor.VendorCompanyName = tb_vendorCompany.Text;
            vendor.VendorCountry = dd_vendorCountry.SelectedValue;
            vendor.VendorDescription = tb_vendorDescription.Text;

            vendor.VendorUrl = tb_vendorUrl.Text;
            vendor.SupportUrl = tb_vendorSupportUrl.Text;
            vendor.BillingContactEmail = tb_vendorBillingEmail.Text;
            vendor.SupportContactEmail = tb_vendorSupportEmail.Text;
            vendor.IBAN = tb_vendorIban.Text;
            vendor.SWIFT = tb_vendorSwift.Text;
            vendor.BSB = tb_vendorBsb.Text;
            vendor.AccountNumber = tb_vendorAccount.Text;
            vendor.PayPalAccount = tb_vendorPayPal.Text;
            vendor.BaseCurrency = tb_vendorBaseCurrency.Text;
            vendor.TaxId = tb_vendorTaxId.Text;
            vendor.VATNumber = tb_vendorVatNumber.Text;

            if (cb_vendorTerms.Enabled && cb_vendorTerms.Checked)
            {
                vendor.DeliTermsAgreementDate = DateTime.Now.ToUniversalTime();
            }

            vendorProvider.SaveOrUpdate(vendor);

            Response.Redirect(umbraco.library.NiceUrl(NextPage));
        }

        protected void ChangeProfile(object sender, EventArgs e)
        {
            var b = sender as LinkButton;
            var parentControl = b.Parent as HtmlGenericControl;

            switch (b.CommandArgument)
            {
                case "Basic":

                    SignupBasicProfile.Visible = true;
                    VendorProfile.Visible = false;
                    parentControl.Attributes.Add("class", "current");
                    ((HtmlGenericControl)vendorNav.Parent).Attributes.Add("class", "");
                    break;
                case "Vendor":
                    SignupBasicProfile.Visible = false;
                    VendorProfile.Visible = true;
                    parentControl.Attributes.Add("class", "current");
                    ((HtmlGenericControl)profileNav.Parent).Attributes.Add("class", "");
                    break;
                default:
                    break;
            }
        }

    }
}