using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.Collections.Specialized;

using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Orders;
using Marketplace.Providers.OrderNote;
using Marketplace.Providers.MemberLicense;
using Marketplace.Providers.Listing;

using Umbraco.Ecommerce.BusinessLogic;
using Umbraco.Ecommerce.PaymentMethods;

using umbraco.BusinessLogic;
using System.Configuration;
using NotificationsCore;
using System.Globalization;


namespace Marketplace.HttpHandlers
{
    public class PayPalIPN
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

        #endregion

        public void Process(HttpContext context, bool TestMode)
        {

            try
            {
                if (VerifiedByPaypal(context, TestMode))
                {
                    NameValueCollection form = context.Request.Form;
                    HandleOrder(form, TestMode);

                }
                else
                {
                    string formStr = "";
                    foreach (string key in context.Request.Form.AllKeys)
                        formStr += key + ": " + context.Request.Form[key] + "...";
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "PAYPAL IPN: Response:" + formStr);
                }

            }
            catch (Exception ex)
            {
                string formStr = "";
                foreach (string key in context.Request.Form.AllKeys)
                    formStr += key + ": " + context.Request.Form[key] + "...";

                umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, -1, "PAYPAL IPN: Response: " + ex.ToString() + formStr);

            }

        }

        public bool VerifiedByPaypal(HttpContext context, bool TestMode)
        {
            try
            {
                string _url = "https://www.paypal.com/cgi-bin/webscr";
                if (TestMode)
                {
                    _url = "https://www.sandbox.paypal.com/cgi-bin/webscr";
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_url);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] param = context.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                string strRequest = Encoding.UTF8.GetString(param);
                strRequest += "&cmd=_notify-validate";
                req.ContentLength = strRequest.Length;

                // Modify the POST string.
                string formPostData = "cmd = _notify-validate";
                foreach (String postKey in context.Request.Form)
                {
                    string postValue = Encode(context.Request.Form[postKey]);
                    formPostData += string.Format("&{0}={1}", postKey, postValue);
                }

                // POST the verification response back to PayPal.
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;

                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                byte[] postByteArray = Encoding.UTF8.GetBytes(formPostData);
                byte[] responseArray = client.UploadData(_url, "POST", postByteArray);
                string response = Encoding.UTF8.GetString(responseArray);

                // check the response, return 'true' if its "VERIFIED"
                return (response == "VERIFIED");
            }
            catch 
            { 
                return false; 
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

                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL IPN: - " + formStr);

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL IPN ERROR: " + ex.ToString());
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

                // log the IPN response in the umbracoLog before anything else
                string formStr = "";
                foreach (string key in form.AllKeys)
                    formStr += key + ": " + form[key] + "....";

                // getDeli Order
                dOrder = OrderProvider.GetOrder(invoice);

                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL IPN: Order: " + dOrder.Id.ToString() + " - " + formStr);

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
                                dOrder.TransactionId = txt_id;
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
                                    Log.Add(LogTypes.Debug, -1, "DELI PAYPAL IPN ERROR: " + e.Message + " :: " + e.InnerException + " :: " + e.StackTrace);

                                    invoicePath = string.Empty;

                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli Error: invoice generation failed");

                                }

                                if (invoicePath.Length > 0)
                                {
                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli invoice: " + invoicePath);

                                }

                                // send order complete mail
                                InstantNotification not = new InstantNotification();
                                not.Invoke(NotificationsWeb.Config.ConfigurationFile, NotificationsWeb.Config.AssemblyDir, "DeliOrderComplete", dOrder, dOrder.Member, txt_id);

                                foreach(var oi in dOrder.Items)
                                {
                                        var listing = ListingProvider.GetListing(oi.ListingItemId,false);
                                        var body = "<p>Hi " + listing.Vendor.Member.Name + ",</p><br/>" +
                                                    "<p>The following order has just been received for the following product on the Umbraco Deli</p><br/>" +
                                                    "<p><strong>" + listing.Name + " " + LicenseProvider.GetLicense(oi.LicenseId).LicenseType.ToString() + "</strong><br/>" +
                                                    "ordered by" + dOrder.Member.Name + " email " + dOrder.Member.Email + "</p>" +
                                                    "<br/><p>Best,<br/>The Umbraco Deli Robot</p>";


                                        umbraco.library.SendMail("robot@umbraco.org", listing.Vendor.Member.Email, "New Deli Order", body, true);
                                }
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
                                    Log.Add(LogTypes.Debug, -1, "DELI PAYPAL IPN ERROR: " + e.Message + " :: " + e.InnerException + " :: " + e.StackTrace);

                                    invoicePath = string.Empty;

                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli Error: invoice generation failed - Error:" + e.Message + " :: " + e.InnerException + " :: " + e.StackTrace);

                                }

                                if (invoicePath.Length > 0)
                                {
                                    OrderNoteProvider.AddOrderNote((int)dOrder.Id, (int)dOrder.Member.Id, "Deli invoice: " + invoicePath);

                                }

                                // send order complete mail
                                InstantNotification not = new InstantNotification();
                                not.Invoke(NotificationsWeb.Config.ConfigurationFile, NotificationsWeb.Config.AssemblyDir, "DeliOrderComplete", dOrder, dOrder.Member, txt_id);
                                foreach (var oi in dOrder.Items)
                                {
                                    not.Invoke(NotificationsWeb.Config.ConfigurationFile, NotificationsWeb.Config.AssemblyDir, "DeliNotifyVendor", oi, dOrder.Member, oi.Id);
                                }
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
                Log.Add(LogTypes.Debug, -1, "DELI PAYPAL IPN " + ex.Message + " :: " + ex.InnerException+ " :: " +ex.StackTrace);
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

                if(TestMode)
                    providerOrder.TestMode = true;

                foreach(IOrderItem item in dOrder.Items)
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
                if(!TestMode)
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
            buyerProxy.VAT = !dOrder.Member.VatInvalid ? dOrder.Member.CompanyVATNumber : "";
            buyerProxy.Phone = "";

            //order lines
            var orderLineProxyList = new List<com.umbraco.orders.OrderLineProxy>();

            foreach (IOrderItem item in dOrder.Items)
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
            dOrder.OrderId = returnOrderProxy.EcommerceOrderID;
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