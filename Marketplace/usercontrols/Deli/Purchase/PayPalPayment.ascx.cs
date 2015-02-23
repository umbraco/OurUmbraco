using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.cms.businesslogic.member;
using umbraco.businesslogic;

using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Orders;
using Marketplace.Providers.PaymentInfo;
using Marketplace.Providers.Listing;
using Marketplace.Providers.Accounting;
using System.Configuration;

namespace Marketplace.usercontrols.Deli.Purchase
{
    public partial class PayPalPayment : System.Web.UI.UserControl
    {

        #region Properties

        private bool testMode = bool.Parse(ConfigurationManager.AppSettings["deliPayPalTest"]);
        public bool TestMode
        {
            get { return testMode; }
            set { testMode = value; }
        }

        private string merchantEmailAddress;
        public string MerchantEmailAddress
        {
            get { return merchantEmailAddress; }
            set { merchantEmailAddress = value; }
        }

        private int notifyPageID;
        public int NotiFyPageID
        {
            get { return notifyPageID; }
            set { notifyPageID = value; }
        }

        private int returnPageID;
        public int ReturnPageID
        {
            get { return returnPageID; }
            set { returnPageID = value; }
        }

        public string _email = "";
        public string _totalPrice = "";
        public string _orderTaxTotal = "";
        public string _paypalurl = "https://www.paypal.com/cgi-bin/webscr";
        public string _notifyurl = "";
        public string _returnurl = "";

        #endregion

        private ICartProvider _cartProvider;
        public ICartProvider CartProvider
        {
            get
            {
                if (_cartProvider != null)
                {
                    return _cartProvider;
                }
                else
                {
                    _cartProvider = (ICartProvider)MarketplaceProviderManager.Providers["CartProvider"];
                    return _cartProvider;
                }
            }
        }

        private ICart _cart;
        public ICart ShoppingCart
        {
            get
            {
                if (_cart != null)
                {
                    return _cart;
                }
                else
                {
                    _cart = CartProvider.GetCurrentCart();
                    return _cart;
                }
            }
            set
            {
                _cart = value;
            }
        }

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

        private IOrder _order;
        public IOrder Order
        {
            get
            {
                if (_order != null)
                    return _order;
                else
                {
                    if (ShoppingCart.OrderId != null)
                    {
                        _order = OrderProvider.GetOrder((int)ShoppingCart.OrderId);
                        return _order;
                    }
                    else
                    {
                        _order = OrderProvider.CreateOrder(ShoppingCart);
                        return _order;
                    }
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


        protected void Page_Load(object sender, EventArgs e)
        {

                if (umbraco.library.IsLoggedOn())
                {
                    if (Order.Member.Id == Member.Id)
                    {
                        pl_orderInfo.Visible = true;

                        // set config
                        SetConfiguration();

                        _totalPrice = CartProvider.GetLocalizedCartGrossValue().ToString();
                        _orderTaxTotal = CartProvider.GetLocalizedTaxTotal().ToString();

                        _email = Member.CompanyInvoiceEmail;

                        // clear the cart from sesison
                        _cartProvider.ClearCart();

                    }
                }
            
        }

        protected void SetConfiguration()
        {

            _notifyurl = "http://" + Request.Url.Host + "/umbraco/ipn/production.aspx";
            _returnurl = "http://" + Request.Url.Host + umbraco.library.NiceUrl(Int32.Parse(ConfigurationManager.AppSettings["deliPayPalReturnUrl"].ToString()));

            merchantEmailAddress = "sales@umbraco.dk";

            if (TestMode)
            {
                _paypalurl = "https://www.sandbox.paypal.com/cgi-bin/webscr";
                merchantEmailAddress = "paul@umbraco.com";
                _notifyurl = "http://dev.our.umbraco.org/umbraco/ipn/dev.aspx";
                _returnurl = "http://dev.our.umbraco.org/deli/order-confirm.aspx";
            }

        }
    }
}