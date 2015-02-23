using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

using Marketplace.Providers.Orders;
using Marketplace.Interfaces;
using Marketplace.Providers.Listing;
using Marketplace.Providers;
using Marketplace.Providers.OrderNote;
using Marketplace.Providers.OrderItem;
using Marketplace.Providers.Vendor;
using Marketplace.Providers.Payout;
using umbraco.cms.businesslogic.member;

namespace Marketplace.usercontrols.Deli.Admin
{
    public partial class VendorPayout : System.Web.UI.UserControl
    {
        #region properties

        public IEnumerable<IOrderItem> PayoutList;
        public IEnumerable<IPayout> Payouts;


        private UmbracoEcommerceOrderProvider _orderProvider;
        public UmbracoEcommerceOrderProvider OrderProvider
        {
            get
            {
                if (_orderProvider != null)
                    return _orderProvider;
                else
                {
                    _orderProvider = (UmbracoEcommerceOrderProvider)MarketplaceProviderManager.Providers["OrderProvider"];
                    return _orderProvider;
                }
            }
        }

        private IDeliOrderItemProvider _itemProvider;
        public IDeliOrderItemProvider ItemProvider
        {
            get
            {
                if (_itemProvider != null)
                    return _itemProvider;
                else
                {
                    _itemProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];
                    return _itemProvider;
                }
            }
        }

        private ILicenseProvider _licenseProvider;
        public ILicenseProvider LicenseProvider
        {
            get
            {
                if (_licenseProvider != null)
                    return _licenseProvider;
                else
                {
                    _licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
                    return _licenseProvider;
                }
            }
        }

        private IMemberLicenseProvider _memberLicenseProvider;
        public IMemberLicenseProvider MemberLicenseProvider
        {
            get
            {
                if (_memberLicenseProvider != null)
                    return _memberLicenseProvider;
                else
                {
                    _memberLicenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["memberLicenseProvider"];
                    return _memberLicenseProvider;
                }
            }
        }

        private OrderNoteProvider _orderNoteProvider;
        public OrderNoteProvider OrderNoteProvider
        {
            get
            {
                if (_orderNoteProvider != null)
                    return _orderNoteProvider;
                else
                {
                    _orderNoteProvider = (OrderNoteProvider)MarketplaceProviderManager.Providers["OrderNoteProvider"];
                    return _orderNoteProvider;
                }
            }
        }

        private NodeListingProvider _listingProvider;
        public NodeListingProvider ListingProvider
        {
            get
            {
                if (_listingProvider != null)
                    return _listingProvider;
                else
                {
                    _listingProvider = (NodeListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                    return _listingProvider;
                }
            }
        }

        private MemberVendorProvider _vendorProvider;
        public MemberVendorProvider VendorProvider
        {
            get
            {
                if (_vendorProvider != null)
                    return _vendorProvider;
                else
                {
                    _vendorProvider = (MemberVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
                    return _vendorProvider;
                }
            }
        }

        private PayoutProvider _payoutProvider;
        public PayoutProvider PayoutProvider
        {
            get
            {
                if (_payoutProvider != null)
                    return _payoutProvider;
                else
                {
                    _payoutProvider = (PayoutProvider)MarketplaceProviderManager.Providers["PayoutProvider"];
                    return _payoutProvider;
                }
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PayoutReport.ItemDataBound += new RepeaterItemEventHandler(PayoutReport_ItemDataBound);
        }

        void PayoutReport_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // The data for this item in the repeatercontrol
            dynamic item = e.Item.DataItem as dynamic;

            if (e.Item.ItemType.ToString() != "Header" && e.Item.ItemType.ToString() != "Footer")
            {
                Literal VendorName = (Literal)e.Item.FindControl("VendorName");
                Literal EconomicId = (Literal)e.Item.FindControl("EconomicId");
                Literal NetPayout = (Literal)e.Item.FindControl("NetPayout");
                double payoutAmount = 0;

                VendorName.Text = VendorProvider.GetVendorById(item.VendorId).VendorCompanyName;
                EconomicId.Text = VendorProvider.GetVendorById(item.VendorId).EconomicId.ToString();
                foreach (IOrderItem i in item.OrderItems)
                {
                    payoutAmount += (i.NetPrice * .75);
                }

                NetPayout.Text = payoutAmount.ToString();
               
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GetPayoutButton_Click(object sender, EventArgs e)
        {
            // get last day of previous month
            var last = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);

            Payouts = PayoutProvider.GetPayouts(null, true);
            PayoutReport.DataSource = Payouts;
            PayoutReport.DataBind();

            EndDate.Text = last.ToLongDateString();

            // show details
            PayoutPlaceholder.Visible = true;

        }

        protected void MarkAsPaidButton_Click(object sender, EventArgs e)
        {
            //PayoutProvider.MarkItemsPaid(int.Parse(PayoutId.Text));
        }

    }
}