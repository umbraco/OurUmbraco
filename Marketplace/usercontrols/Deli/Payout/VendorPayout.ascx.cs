using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using NotificationsCore;

namespace Marketplace.usercontrols.Deli.Payout
{
    public partial class VendorPayout : System.Web.UI.UserControl
    {


        public int PayoutHistoryPage { get; set; }
        protected string HistoryPage;

        private IDeliOrderItemProvider _orderItemProvider;
        private IOrderProvider _orderProvider;
        private IMemberProvider _memberProvider;
        private IVendorProvider _vendorProvider;
        private IListingProvider _listingProvider;
        private ILicenseProvider _licenseProvider;
        private IPayoutProvider _payoutProvider;
        private IVendor vendor;


        protected void Page_Load(object sender, EventArgs e)
        {

            _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            _vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            _listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            _orderProvider = (IOrderProvider)MarketplaceProviderManager.Providers["OrderProvider"];
            _orderItemProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];
            _licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];
            _payoutProvider = (IPayoutProvider)MarketplaceProviderManager.Providers["PayoutProvider"];

            IMember mem = _memberProvider.GetCurrentMember();
            vendor = _vendorProvider.GetVendorByGuid(mem.UniqueId);

            HistoryPage = umbraco.library.NiceUrl(PayoutHistoryPage);

            if (!IsPostBack)
            {
                BindOrderItems();
            }
        }

        protected void BindOrderItems()
        {
            

            DateTime now = DateTime.Now;

            DateTime lastDayLastMonth = new DateTime(now.Year, now.Month, 1);
            lastDayLastMonth = lastDayLastMonth.AddDays(-1);


            // bind the table to any order that has not been paid out and does not have a payout id
            var items = _orderItemProvider.GetOrderItemsForPayout(vendor.Member.Id, lastDayLastMonth)
                .Where(x => x.PayoutId == null)
                .Select(x => new
            {
                Id = x.Id,
                Package = _listingProvider.GetListing(x.ListingItemId,true).Name,
                LicenseTypeName = _licenseProvider.GetLicense(x.LicenseId).LicenseType.ToString(),
                Member = _orderProvider.GetOrder(x.OrderId).Member,
                PayoutAmount = x.PayoutAmount,
                OrderDate = string.Format("{0:D}",x.CreateDate)
            });


            if (items.Count() > 0)
            {
                PayoutList.DataSource = items;
                PayoutList.DataBind();
                payoutForm.Visible = true;
                NoPayouts.Visible = false;
            }
            else
            {
                payoutForm.Visible = false;
                NoPayouts.Visible = true;
            }
        }

        protected void PayoutRequest_Click(object s, EventArgs e)
        {
            // Create the new payout
            var payout = new Marketplace.Providers.Payout.Payout();
            payout.VendorRef = vendor_reference.Text;
            payout.Guid = Guid.NewGuid();
            payout.PayoutDate = DateTime.Now;
            payout.VendorId = vendor.Member.Id;
            payout.Processed = false;
            _payoutProvider.SaveOrUpdate(payout);


            //grab the list of orderitems that this payout applies to

            List<IOrderItem> items = new List<IOrderItem>();
            foreach (RepeaterItem ri in PayoutList.Items)
            {
                if (ri.ItemType != ListItemType.Header || ri.ItemType != ListItemType.Footer)
                {
                    var check = (CheckBox)ri.FindControl("RowCheckbox");
                    if (check.Checked)
                    {
                        var itemID = Int32.Parse(((HiddenField)ri.FindControl("RowId")).Value);
                        var orderItem = _orderItemProvider.GetOrderItem(itemID);
                        
                        //set the payout id for the order items
                        orderItem.PayoutId = payout.Id;
                        _orderItemProvider.SaveOrUpdate(orderItem);

                        //add to the list of the order items for the payout
                        items.Add(orderItem);
                    }
                }
            }

            payout.OrderItems = items;

            //send notification to Niels :)
            InstantNotification not = new InstantNotification();
            not.Invoke(NotificationsWeb.Config.ConfigurationFile, NotificationsWeb.Config.AssemblyDir, "DeliPayoutRequest", payout);

            //rebind the table to remove any that have been set as paid out
            payoutHolder.Visible = false;
            PayoutThanks.Visible = true;
        }

    }
}