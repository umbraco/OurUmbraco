using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Providers.Orders;
using Marketplace.Interfaces;
using Marketplace.Providers.Listing;
using Marketplace.Providers;
using Marketplace.Providers.OrderNote;
using System.Data;
using Marketplace.Providers.OrderItem;

namespace Marketplace.usercontrols.Deli.Admin
{
    public partial class ProcessRefund : System.Web.UI.UserControl
    {

        #region properties

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

        public IOrder Order;

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

        #endregion

        protected void GetOrderButton_Click(object sender, EventArgs e)
        {
            int oId = 0;

            if (int.TryParse(OrderId.Text, out oId))
                oId = int.Parse(OrderId.Text);

            if (oId > 0)
            {
                Order = OrderProvider.GetOrder(oId);

                OrderReviewDisplay.Text = Order.Member.Name + ", Paid on: " + Order.PaymentDate.ToString() + " Amount Paid: " + Order.ProcessedTotal.ToString() + " in Currency: " + Order.Currency.ToString() + ", Order Status: " + Order.Status.ToString();

                DataTable dt = new DataTable();
                dt.Columns.Add("id");
                dt.Columns.Add("item");

                foreach (IOrderItem i in Order.Items)
                {
                    string itemText = ListingProvider.GetListing(i.ListingItemId).Name + 
                        ", License Type: " + LicenseProvider.GetLicense(i.LicenseId).LicenseType.ToString() + 
                        ", Quantity: " + i.Quantity.ToString() +
                        ", Net Price: " + i.NetPrice.ToString() + " EUR, each";

                    dt.Rows.Add(i.Id, itemText);
                }

                OrderItems.DataSource = dt;
                OrderItems.DataTextField = "item";
                OrderItems.DataValueField = "id";
                OrderItems.DataBind();

                // order notes
                IEnumerable<IOrderNote> Notes = OrderNoteProvider.GetOrderNotes(oId);
                OrderNotesRepeater.DataSource = Notes;
                OrderNotesRepeater.DataBind();

            }
            else
                OrderReviewDisplay.Text = "No order with the Id: " + oId.ToString();

        }

        protected void ProcessRefundButton_Click(object sender, EventArgs e)
        {
            int oId = 0;
            if (int.TryParse(OrderId.Text, out oId))
                oId = int.Parse(OrderId.Text);

            if (oId > 0)
                Order = OrderProvider.GetOrder(oId);

            // mark order as refunded
            Order.Status = OrderStatus.Refunded;
            Order.IsRefunded = true;
            Order.RefundDate = DateTime.Now.ToUniversalTime();
            OrderProvider.SaveOrUpdate(Order);
            
            // mark item as refunded
            IOrderItem item = ItemProvider.GetOrderItem(int.Parse(OrderItems.SelectedValue));
            item.IsRefunded = true;
            item.RefundDate = DateTime.Now.ToUniversalTime();
            ItemProvider.SaveOrUpdate(item);

            // revoke member license
            var licenses = MemberLicenseProvider.GetLicensesByOrderItemId(item.Id);
            foreach (var license in licenses)
            {
                license.IsActive = false;
                MemberLicenseProvider.SaveOrUpdate(license);
            }

            // add order note
            OrderNoteProvider.AddOrderNote((int)Order.Id, (int)Order.Member.Id, "Refund processed for item: " + item.Id.ToString());

            RefundConfirmation.Text = "Refund processed and license revoked.";
        }


        }
    }