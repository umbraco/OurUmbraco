using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Helpers;
using Marketplace.Providers.Orders;
using Marketplace.Providers.Accounting;
using Marketplace.Controls;
using System.Globalization;

namespace Marketplace.usercontrols.Deli.Cart
{
    public partial class CartReview : System.Web.UI.UserControl
    {
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

        public Currency _currency = Currency.EUR;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            CartItems.ItemDataBound += new RepeaterItemEventHandler(CartItems_ItemDataBound);
        }

        void CartItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // The data for this item in the repeatercontrol
            dynamic item = e.Item.DataItem as dynamic;

            if (e.Item.ItemType.ToString() != "Header" && e.Item.ItemType.ToString() != "Footer")
            {
                // set the price and subtotal amounts
                Literal price = (Literal)e.Item.FindControl("price");
                Literal subtotal = (Literal)e.Item.FindControl("subtotal");

                price.Text = DeliCurrency.NiceMoney((decimal)item.Price, DeliCurrency.FromSymbol(_currency.ToString()));
                subtotal.Text = DeliCurrency.NiceMoney((decimal)item.SubTotal, DeliCurrency.FromSymbol(_currency.ToString()));

            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
            {
                LoadOrderInfo();
                LoadCart();
                
            }
            else
            {
                Response.Redirect("cart");
            }
        }

        private void LoadOrderInfo()
        {
            if (Member != null)
            {
                var countryName = GetCountry(Member.CompanyCountry);
                OrderMembername.Text = Member.Name;
                OrderCompany.Text = Member.Company + "<br />" + Member.CompanyAddress + "<br />" + countryName;
                OrderInvoiceEmail.Text = Member.CompanyInvoiceEmail;
                OrderVATCode.Text = String.IsNullOrEmpty(Member.CompanyVATNumber) ? "NONE SPECIFIED" : Member.CompanyVATNumber; 
            }
            else
            {
                Response.Redirect("cart");
            }
        }

        private string GetCountry(string code)
        {


            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {

                RegionInfo ri = new RegionInfo(ci.LCID);
                if (ri.TwoLetterISORegionName.ToLowerInvariant() == code)
                    return ri.EnglishName;
            }

            return "";

        }

        private void LoadCart()
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            _currency = ShoppingCart.Currency;

            var cartItems = CartProvider.GetCartItems().Select(x => new
            {
                Id = x.LicenseId,
                Name = x.ListingItem.Name,
                License = licenseProvider.GetLicense(x.LicenseId).LicenseType.LicenseTypeAsString(),
                Price = GetLocalisedPrice(x.NetPrice),
                Quantity = x.Quantity,
                SubTotal = x.Quantity * GetLocalisedPrice(x.NetPrice)

            });

            CartItems.DataSource = cartItems;
            CartItems.DataBind();

            // tax check and calcs
            double cartValue = 0;
            double taxValue = 0;

            var vatValid = CartProvider.IsVatValid();
            var vatCountry = CartProvider.IsVatCountry();


            taxValue = CartProvider.GetLocalizedTaxTotal();

            cartValue = CartProvider.GetLocalizedCartNetValue();



            CartTotal.Text = DeliCurrency.NiceMoney((decimal)cartValue, DeliCurrency.FromSymbol(_currency.ToString()));


            // check to see if there is a tax value and if its a valid vat country
            // it will always enter this if it is denmark.
            if (taxValue > 0 && vatCountry)
            {
                //it will validate the vat for denmark also to display it.
                if (!vatValid && Member.CompanyCountry.ToLower() != "dk")
                {
                    VatBadMessage.Visible = true;
                    OrderVATCode.Text += " (NOT VALID)";
                }

                TaxRow.Visible = true;
                TaxTotal.Text = DeliCurrency.NiceMoney((decimal)taxValue, DeliCurrency.FromSymbol(_currency.ToString()));
            }
            else
            {
                if (!vatCountry)
                {
                    VATDetails.Visible = false;
                }

                TaxRow.Visible = false;
            }

        }

        private double GetLocalisedPrice(double p)
        {
            // do converison if not EUR
            if (_currency != Currency.EUR)
                return (double)DeliCurrency.ConvertFromEuro((decimal)p, DeliCurrency.FromSymbol(_currency.ToString()));
            else
                return p;
        }

        protected void CartNavNext_Click(object s, EventArgs e)
        {
            Response.Redirect("cart-payment");
        }
        protected void CartNavPrevious_Click(object s, EventArgs e)
        {
            Response.Redirect("cart");
        }
    }
}