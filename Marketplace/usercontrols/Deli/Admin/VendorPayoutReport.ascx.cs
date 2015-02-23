using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using System.Configuration;
using umbraco;
using System.Web.UI.HtmlControls;
using umbraco.BasePages;

namespace Marketplace.usercontrols.Deli.Admin
{
    public partial class VendorPayoutReport : System.Web.UI.UserControl
    {

        private IDeliOrderItemProvider _orderItemProvider;
        private IOrderProvider _orderProvider;
        private IMemberProvider _memberProvider;
        private IVendorProvider _vendorProvider;
        private IListingProvider _listingProvider;
        private ILicenseProvider _licenseProvider;
        private IPayoutProvider _payoutProvider;
        private BasePage prnt;



        protected void Page_Load(object sender, EventArgs e)
        {
            prnt = (BasePage)this.Page;
            HtmlHead head = (HtmlHead)base.Page.Header;
            HtmlLink link = new HtmlLink();

            link.Attributes.Add("href", GlobalSettings.Path + "/dashboard/deli/css/deliadmin.css?v2");
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            head.Controls.Add(link);
        }


        protected void LoadReport(object sender, EventArgs e)
        {
            BindListing(true);
        }

        protected void LoadProcessedReport(object sender, EventArgs e)
        {
            BindListing(false);
        }

        private void BindListing(bool pending)
        {
            _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            _vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            _listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            _orderProvider = (IOrderProvider)MarketplaceProviderManager.Providers["OrderProvider"];
            _orderItemProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];
            _licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            _payoutProvider = (IPayoutProvider)MarketplaceProviderManager.Providers["PayoutProvider"];


            PayoutHistoryList.ItemDataBound += new RepeaterItemEventHandler(PayoutHistoryList_ItemDataBound);

            var payouts = _payoutProvider.GetPayouts(null, true, pending).OrderByDescending(x => x.PayoutDate).Select(x => new 
            { 
                Id = x.Id,
                Vendor = _vendorProvider.GetVendorById(x.VendorId),
                PayoutDate = String.Format("{0:D}",x.PayoutDate),
                Reference = x.VendorRef,
                PayoutAmount = GetPayoutValue(x.OrderItems),
                Status = (x.Processed)?"Processed":"Pending"
            });

            PayoutHistoryList.DataSource = payouts;
            PayoutHistoryList.DataBind();
        }

        protected void PayoutHistoryList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem ri = (RepeaterItem)e.Item;
            if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
            {
                Repeater itemRepeater = ((RepeaterItem)e.Item).FindControl("payoutItemRepeater") as Repeater;
                int rowId = Int32.Parse((((RepeaterItem)e.Item).FindControl("rowId") as HiddenField).Value);

                var payout = _payoutProvider.GetPayout(rowId);

                // bind the table to any order that has not been paid out and does not have a payout id
                var items = payout.OrderItems
                    .Select(x => new
                    {
                        Id = x.Id,
                        Package = _listingProvider.GetListing(x.ListingItemId, true).Name,
                        LicenseTypeName = _licenseProvider.GetLicense(x.LicenseId).LicenseType.ToString(),
                        Member = _orderProvider.GetOrder(x.OrderId).Member,
                        PayoutAmount = x.PayoutAmount,
                        OrderDate = string.Format("{0:D}", x.CreateDate)
                    });

                itemRepeater.DataSource = items;
                itemRepeater.DataBind();
            }

        }

        protected void processPayoutRow(Object sender, RepeaterCommandEventArgs e)
        {

            switch (e.CommandName)
            {
                case "Payout":
                    var payoutId = Int32.Parse(e.CommandArgument.ToString());

                    _payoutProvider = (IPayoutProvider)MarketplaceProviderManager.Providers["PayoutProvider"];
                    _payoutProvider.MarkItemsPaid(payoutId);

                    break;
                default:
                    break;
            }

            BindListing(true);

        }


        private double GetPayoutValue(IEnumerable<IOrderItem> items)
        {
            double val = 0.00;
            foreach (var i in items)
            {
                val += i.PayoutAmount;
            }

            return val;
        }



    }
}