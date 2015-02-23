using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Umbraco.Support.zendesk;
using Marketplace.Interfaces;
using Marketplace.Providers;

namespace Marketplace.usercontrols.Deli.VendorSupport
{
    public partial class VendorSupportRequest : System.Web.UI.UserControl
    {
        private IMemberProvider _memberProvider;
        public IMemberProvider MemberProvider
        {
            get
            {
                if (_memberProvider != null)
                    return _memberProvider;
                else
                {
                    _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
                    return _memberProvider;
                }
            }
        }

        private IMember _member;
        public IMember Member
        {
            get
            {
                if (_member != null)
                    return _member;
                else
                {
                    _member = MemberProvider.GetCurrentMember();
                    return _member;
                }
            }
        }

        protected void bt_submit_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;

            try
            {
                string body = "Submitted by Deli Vendor: " + Member.IsDeliVendor.ToString() + "\r\n" +
                                "Vendor name: " + Member.Name + "," + Member.Company + "\r\n" +
                                umbraco.library.ReplaceLineBreaks(tb_desc.Text);

                Ticket.Create(Member.Email, tb_subject.Text, body, "Deli", tb_package.Text);

                pnlSuccess.Visible = true;
            }
            catch
            {
                pnlError.Visible = false;
            }
        }
    }
}