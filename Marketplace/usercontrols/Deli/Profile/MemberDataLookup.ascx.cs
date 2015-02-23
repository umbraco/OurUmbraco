using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using umbraco.cms.businesslogic.member;

namespace Marketplace.usercontrols.Deli.Profile
{
    public partial class MemberDataLookup : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void getMemberData_Click(object sender, EventArgs e)
        {
            try
            {
                lookupFailed.Visible = false;
                com.umbraco.MemberLookUp service = new com.umbraco.MemberLookUp();
                com.umbraco.MemberData md = service.GetMemberData(tb_login.Text, tb_password.Text);

                var memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];

                Member current = new Member(memberProvider.GetCurrentMember().Id);
                if (current != null)
                {
                    current.getProperty("umbracoDotComID").Value = md.Id;

                    if (!string.IsNullOrEmpty(md.Address))
                    {
                        lt_data.Text += "Company Address: " + md.Address + "<br/>";
                        current.getProperty("companyAddress").Value = md.Address;

                    }

                    if (!string.IsNullOrEmpty(md.Country))
                    {
                        lt_data.Text += "Company Country: " + md.Country+ "<br/>";
                        current.getProperty("companyCountry").Value = md.Country;
                    }

                    if (!string.IsNullOrEmpty(md.VATNumber))
                    {
                        lt_data.Text += "Company VAT Number: " + md.VATNumber + "<br/>";
                        current.getProperty("companyVATNumber").Value = md.VATNumber;
                    }

                    if (!string.IsNullOrEmpty(md.InvoicingEmail))
                    {
                        lt_data.Text += "Company Invoice Email: " + md.VATNumber + "<br/>";
                        current.getProperty("companyInvoiceEmail").Value = md.InvoicingEmail;
                    }

                    string groups = string.Empty;
                    foreach (string mg in md.MemberGroups)
                    {
                       

                        MemberGroup g = MemberGroup.GetByName(mg);
                        if (g != null)
                        {
                            groups += mg + ", ";
                            current.AddGroup(g.Id);
                        }
                    }

                    if (!string.IsNullOrEmpty(groups))
                    {
                        lt_data.Text += "Groups: " + groups.Substring(0, groups.Length - 2);
                    }
                }



                lookupSuccess.Visible = true;
                lookupLogin.Visible = false;

            }
            catch
            {
                lookupFailed.Visible = true;
            }
        }
    }
}