using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Providers.Orders;
using Marketplace.Providers;
using Marketplace.Interfaces;
using Marketplace.Providers.Listing;
using Marketplace.Providers.OrderNote;
using Marketplace.Providers.MemberLicense;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using umbraco.BusinessLogic;
using Umbraco.Ecommerce.BusinessLogic;
using Umbraco.Ecommerce.PaymentMethods;
using System.Configuration;
using NotificationsCore;

namespace Marketplace.usercontrols.Deli.Purchase
{
    public partial class PayPalConfirm : System.Web.UI.UserControl
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

        public IOrder dOrder;

        private IDeliOrderItemProvider _orderItemProvider;
        public IDeliOrderItemProvider OrderItemProvider
        {
            get
            {
                if (_orderItemProvider != null)
                    return _orderItemProvider;
                else
                {
                    _orderItemProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];
                    return _orderItemProvider;
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

        public MemberLicense mLicense;

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


        private bool testMode = bool.Parse(ConfigurationManager.AppSettings["deliPayPalTest"]);
        public bool TestMode
        {
            get { return testMode; }
            set { testMode = value; }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {

                    NameValueCollection form = HttpContext.Current.Request.QueryString;
                    HandleOrder(form, TestMode);

                }

            }
            catch (Exception ex)
            {
                string formStr = "";
                foreach (string key in HttpContext.Current.Request.QueryString.AllKeys)
                    formStr += key + ": " + HttpContext.Current.Request.QueryString[key] + "...";

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "PAYPAL CONFIRM: Response: " + ex.ToString() + formStr);

            }
        }

        public void TESTHandleOrder(NameValueCollection form, bool testMode)
        {
            try
            {
                string amount = form["amount"];
                int invoice = int.Parse(form["invoice"]);
                string status = form["payment_status"];
                string txt_id = form["txn_id"];

                // log the IPN response in the umbracoLog
                string formStr = "";
                foreach (string key in form.AllKeys)
                    formStr += key + ": " + form[key] + "....";

                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL CONFIRM: - " + formStr);

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL CONFIRM ERROR: " + ex.ToString());
            }
        }

        public void HandleOrder(NameValueCollection form, bool testMode)
        {
            try
            {
                string amount = form["mc_gross"];
                int invoice = int.Parse(form["invoice"]);
                string status = form["payment_status"];
                string txt_id = form["txn_id"];
                string NoteText = "";

                // log the PayPal response in the umbracoLog before anything else
                string formStr = "";
                foreach (string key in form.AllKeys)
                    formStr += key + ": " + form[key] + "....";

                // get the Deli Order
                dOrder = OrderProvider.GetOrder(invoice);

                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL CONFIRM: Order: " + dOrder.Id.ToString() + " - " + formStr);

                switch (status)
                {
                    case "Completed":
                        // make sure this isn't a duplicate from paypal
                        if (dOrder.Status != OrderStatus.Confirmed)
                        {
                            if (!testMode)  // production!
                            {
                                NoteText = "order status changed from " + dOrder.Status.ToString() + " to " + OrderStatus.Confirmed.ToString() + "with transaction id: " + txt_id;
                                dOrder.Status = OrderStatus.Confirmed;
                                OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, NoteText);

                                dOrder.PaymentDate = DateTime.Now.ToUniversalTime();
                                dOrder.ProcessedTotal = Convert.ToDouble(amount);

                                // create a MemberLicense for each OrderItem
                                foreach (IOrderItem item in dOrder.Items)
                                {
                                    // create licenses for multiple quantity
                                    for (int i = 0; i < item.Quantity; i++)
                                    {
                                        mLicense = new MemberLicense();
                                        mLicense.LicenseId = LicenseProvider.GetLicense(item.LicenseId).Id;
                                        mLicense.Member = dOrder.Member;
                                        mLicense.OrderItemId = item.Id;
                                        mLicense.ListingItemId = item.ListingItemId;
                                        mLicense.CreateDate = DateTime.Now.ToUniversalTime();
                                        mLicense.IsActive = true;

                                        MemberLicenseProvider.SaveOrUpdate(mLicense);
                                    }
                                }

                                OrderProvider.SaveOrUpdate(dOrder);

                                // webservice call to create order on umbraco.com
                                string invoicePath;

                                try
                                {
                                    invoicePath = CreateDotComOrder(dOrder, txt_id);
                                }
                                catch (Exception e)
                                {
                                    Log.Add(LogTypes.Debug, -1, "DELI PAYPAL CONFIRM ERROR: " + e.Message + " :: " + e.InnerException + " :: " + e.StackTrace);

                                    invoicePath = string.Empty;

                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli Error: invoice generation failed");

                                    InvoicePanel.Visible = false;
                                    PaymentConfirmPanel.Visible = true;
                                    PaymentPendingPanel.Visible = false;
                                }

                                if (invoicePath.Length > 0)
                                {
                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli invoice: " + invoicePath);

                                    InvoiceLink.NavigateUrl = "http://umbraco.com" + invoicePath;

                                    InvoicePanel.Visible = true;

                                }

                                PaymentConfirmPanel.Visible = true;
                                PaymentPendingPanel.Visible = false;


                                // send order complete mail
                                InstantNotification not = new InstantNotification();
                                not.Invoke(NotificationsWeb.Config.ConfigurationFile, NotificationsWeb.Config.AssemblyDir, "DeliOrderComplete", dOrder, dOrder.Member, txt_id);

                            }
                            else  // test!
                            {
                                NoteText = "DEV order status changed from " + dOrder.Status.ToString() + " to " + OrderStatus.Confirmed.ToString() + "with transaction id: " + txt_id;
                                dOrder.Status = OrderStatus.Confirmed;
                                OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, NoteText);

                                dOrder.PaymentDate = DateTime.Now.ToUniversalTime();
                                dOrder.ProcessedTotal = Convert.ToDouble(amount);

                                // create a MemberLicense for each OrderItem
                                foreach (IOrderItem item in dOrder.Items)
                                {
                                    // create licenses for multiple quantity
                                    for (int i = 0; i < item.Quantity; i++)
                                    {
                                        mLicense = new MemberLicense();
                                        mLicense.LicenseId = LicenseProvider.GetLicense(item.LicenseId).Id;
                                        mLicense.Member = dOrder.Member;
                                        mLicense.OrderItemId = item.Id;
                                        mLicense.ListingItemId = item.ListingItemId;
                                        mLicense.CreateDate = DateTime.Now.ToUniversalTime();
                                        mLicense.IsActive = true;

                                        MemberLicenseProvider.SaveOrUpdate(mLicense);
                                    }
                                }

                                OrderProvider.SaveOrUpdate(dOrder);

                                // webservice call to create order on umbraco.com
                                string invoicePath;

                                try
                                {
                                    invoicePath = CreateDotComOrder(dOrder, txt_id);
                                }
                                catch (Exception e)
                                {

                                    Log.Add(LogTypes.Debug, -1, "DELI PAYPAL CONFIRM ERROR: " + e.Message + " :: " + e.InnerException + " :: " + e.StackTrace);
                                    
                                    invoicePath = string.Empty;

                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli Error: invoice generation failed");

                                    InvoicePanel.Visible = false;
                                    PaymentConfirmPanel.Visible = true;
                                    PaymentPendingPanel.Visible = false;
                                }

                                if (invoicePath.Length > 0)
                                {
                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli invoice: " + invoicePath);

                                    InvoiceLink.NavigateUrl = "http://umbraco.com" + invoicePath;

                                    InvoicePanel.Visible = true;

                                }
                                 

                                PaymentConfirmPanel.Visible = true;
                                PaymentPendingPanel.Visible = false;
                                

                                // send order complete mail
                                InstantNotification not = new InstantNotification();
                                not.Invoke(NotificationsWeb.Config.ConfigurationFile, NotificationsWeb.Config.AssemblyDir, "DeliOrderComplete", dOrder, dOrder.Member, txt_id);

                            }

                        }
                        break;

                    case "Pending":
                        NoteText = "order status changed from " + dOrder.Status.ToString() + " to " + OrderStatus.Pending.ToString() + "with transaction id: " + txt_id;
                        dOrder.Status = OrderStatus.Pending;
                        OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, NoteText);

                        OrderProvider.SaveOrUpdate(dOrder);

                        break;

                    case "Failed":
                        NoteText = "order status changed from " + dOrder.Status.ToString() + " to " + OrderStatus.Rejected.ToString() + "with transaction id: " + txt_id;
                        dOrder.Status = OrderStatus.Rejected;
                        OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, NoteText);

                        OrderProvider.SaveOrUpdate(dOrder);

                        break;

                    case "Refunded":
                        NoteText = "order status changed from " + dOrder.Status.ToString() + " to " + OrderStatus.Refunded.ToString() + "with transaction id: " + txt_id;
                        dOrder.Status = OrderStatus.Refunded;
                        dOrder.IsRefunded = true;
                        dOrder.RefundDate = DateTime.Now.ToUniversalTime();

                        OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, NoteText);

                        // todo: revoke license here
                        // todo:  mark specific OrderItems as Refunded

                        OrderProvider.SaveOrUpdate(dOrder);

                        break;

                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL CONFIRM " + ex.Message + " :: " + ex.InnerException + " :: " + ex.StackTrace);
            }
        }

        public int CreateEcomOrder(IOrder o, bool TestMode)
        {
            try
            {
                // Create customer information
                CustomerInfo ci = new CustomerInfo();
                ci.AddData("id", o.Member.Id.ToString());
                ci.AddData("email", o.Member.Email);

                Guid payment = new CreditCard().Id;

                // create order
                Order providerOrder = Order.MakeNew(ci.ToXml(), o.Id.ToString(), payment, "deli order", o.Member.Email);
                providerOrder.CustomerReference = o.Id.ToString();
                providerOrder.ChangeStatus(Order.Status.Confirmed);
                providerOrder.Comment = "deli order";

                if (TestMode)
                    providerOrder.TestMode = true;

                foreach (IOrderItem item in dOrder.Items)
                {
                    OrderLine l = new OrderLine();
                    l.Quantity = item.Quantity;
                    l.ProductName = ListingProvider.GetListing(item.ListingItemId, false).Name + ", License: " + LicenseProvider.GetLicense(item.LicenseId).LicenseType.ToString();
                    l.Price = (decimal)item.NetPrice;
                    l.ProductId = item.ListingItemId.ToString();
                    l.Note = "deli order id: " + o.Id.ToString();

                    providerOrder.AddOrderLine(l);
                }

                // generate invoice
                if (!TestMode)
                    providerOrder.GenerateInvoice(false);
                else
                    providerOrder.GenerateInvoice(true);


                return providerOrder.OrderId;
            }
            catch
            {
                return 0;
            }
        }

        protected string CreateDotComOrder(IOrder dOrder, string transactionId)
        {
            com.umbraco.orders.Orders orderService = new com.umbraco.orders.Orders();
            com.umbraco.orders.OrderProxy orderProxy = new com.umbraco.orders.OrderProxy();
            com.umbraco.orders.BuyerProxy buyerProxy = new com.umbraco.orders.BuyerProxy();

            // set up Ecomm Order
            orderProxy.DeliOrderID = dOrder.Id;
            orderProxy.PurchaseOrder = "";
            orderProxy.TransactionID = transactionId;

            // buyer
            buyerProxy.ourMemberId = dOrder.Member.Id;
            buyerProxy.ourMemberGuid = dOrder.Member.UniqueId;
            buyerProxy.IP = dOrder.IP;
            buyerProxy.Name = dOrder.Member.Name;
            buyerProxy.Login = dOrder.Member.Name;
            buyerProxy.Address = dOrder.Member.CompanyAddress;
            buyerProxy.Company = dOrder.Member.Company;
            buyerProxy.Country = Marketplace.library.GetCountry(dOrder.Member.CompanyCountry);
            buyerProxy.Email = dOrder.CompanyInvoiceEmail;
            buyerProxy.VAT = !dOrder.Member.VatInvalid?dOrder.Member.CompanyVATNumber:"";
            buyerProxy.Phone = "";

            //order lines
            var orderLineProxyList = new List<com.umbraco.orders.OrderLineProxy>();

            foreach(IOrderItem item in dOrder.Items)
            {
                com.umbraco.orders.OrderLineProxy lineProxy = new com.umbraco.orders.OrderLineProxy();
                IListingItem listingItem = ListingProvider.GetListing(item.ListingItemId);
                
                lineProxy.VendorEconomicID = listingItem.Vendor.EconomicId;
                lineProxy.Amount = (decimal)item.NetPrice;
                lineProxy.Quantity = item.Quantity;
                lineProxy.description = ListingProvider.GetListing(item.ListingItemId, false).Name + ", License Type: " + LicenseProvider.GetLicense(item.LicenseId).LicenseType.ToString();

                // add order line to order
                orderLineProxyList.Add(lineProxy);

            }


            orderProxy.OrderLines = orderLineProxyList.ToArray();
            orderProxy.Buyer = buyerProxy;

            bool isTestMode = bool.Parse(ConfigurationManager.AppSettings["deliPayPalTest"]);


            com.umbraco.orders.OrderProxy returnOrderProxy = orderService.Create(orderProxy, isTestMode, ConfigurationManager.AppSettings["deliWebServiceUser"], ConfigurationManager.AppSettings["deliWebServicePass"]);


            // set Ecomm order id
            dOrder.OrderId =  returnOrderProxy.EcommerceOrderID;
            OrderProvider.SaveOrUpdate(dOrder);

            return returnOrderProxy.InvoicePath;
             
        }


        #region Utilities

        private string Encode(string oldValue)
        {
            string newValue = oldValue.Replace("\"", "'");
            newValue = System.Web.HttpUtility.UrlEncode(newValue);
            newValue = newValue.Replace("%2f", "/");
            return newValue;
        }

        #endregion

    }
}