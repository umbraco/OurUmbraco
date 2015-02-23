using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Umbraco.Support.zendesk;
using Marketplace.Interfaces;
using Marketplace.Providers;

namespace Marketplace.usercontrols.Deli.MemberSupport
{
    public partial class RefundRequest : System.Web.UI.UserControl
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
                string body = "Member name: " + Member.Name + "," + Member.Company +
                                "Order Date: " + OrderDate.Text +
                                "Refund Request: " + umbraco.library.ReplaceLineBreaks(tb_desc.Text);

                Ticket.Create(Member.Email, "Refund Request", body, "Deli", tb_package.Text);

                pnlSuccess.Visible = true;
            }
            catch
            {
                pnlError.Visible = false;
            }
        }
    }
}