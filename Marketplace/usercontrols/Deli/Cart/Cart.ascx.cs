using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Accounting;
using Marketplace.Helpers;

namespace Marketplace.usercontrols.Deli.Cart
{
    public partial class Cart : System.Web.UI.UserControl
    {

        #region properties
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

        private Currency _currency = Currency.EUR; 
        #endregion

        protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
            CartItems.ItemDataBound += new RepeaterItemEventHandler(CartItems_ItemDataBound);
			CartItems.ItemCommand +=new RepeaterCommandEventHandler(CartItems_ItemCommand);
		}

        protected void Page_PreRender(object sender, EventArgs e)
        {

            if (ShoppingCart != null && ShoppingCart.CartItems.Count > 0)
            {
                LoadCart();

            }
            else
            {
                CartHolder.Visible = false;
                CartEmptyHolder.Visible = true;
            }

        }

        private void LoadCart()
        {
            var licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            // use cart currency
            _currency = ShoppingCart.Currency;

            //to enusre the cart rounds and adds properly we convert to localised price when its doing the subtotal step.
            var cartItems = CartProvider.GetCartItems().Select(x => new {
                Id = x.LicenseId,
                Name = x.ListingItem.Name,
                License = licenseProvider.GetLicense(x.LicenseId).LicenseType.LicenseTypeAsString(),
                Price = _cartProvider.GetLocalizedPrice(x.NetPrice),
                Quantity = x.Quantity,
                SubTotal = x.Quantity * _cartProvider.GetLocalizedPrice(x.NetPrice)

            });

            CartItems.DataSource = cartItems;
            CartItems.DataBind();

            
            CartTotal.Text = DeliCurrency.NiceMoney((decimal)_cartProvider.GetLocalizedCartGrossValue(), DeliCurrency.FromSymbol(_currency.ToString()));

        }

        protected void QuantityOnChange(object sender, EventArgs e)
        {
            string licenseId = ((HiddenField)((TextBox)sender).Parent.FindControl("LicenseId")).Value;
            TextBox qtyBox = (TextBox)sender;
            int qty = 0;
            if (!string.IsNullOrEmpty(qtyBox.Text))
            {
                qty = int.Parse(qtyBox.Text);
            }

            var ci = CartProvider.GetCartItems().Where(x => x.LicenseId == Int32.Parse(licenseId)).FirstOrDefault();
            if (ci != null)
            {
                CartProvider.ChangeItemQuantity(ci, qty);
            }


        }

        void CartItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // The data for this item in the repeatercontrol
            dynamic item = e.Item.DataItem as dynamic;

            if (e.Item.ItemType.ToString() != "Header" && e.Item.ItemType.ToString() != "Footer")
            {
                Button btnDel = (Button)e.Item.FindControl("Del");

                // Assign ids of items to the buttons
                btnDel.CommandName = "delItem";
                btnDel.CommandArgument = item.Id.ToString();

                // set the price and subtotal amounts
                Literal price = (Literal)e.Item.FindControl("price");
                Literal subtotal = (Literal)e.Item.FindControl("subtotal");

                price.Text = DeliCurrency.NiceMoney((decimal)item.Price, DeliCurrency.FromSymbol(_currency.ToString()));
                
                //no need to localize the price for the subtotal as this is done during the datasource creation
                subtotal.Text = DeliCurrency.NiceMoney((decimal)item.SubTotal,DeliCurrency.FromSymbol(_currency.ToString()));
            
            }
        }

        void CartItems_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			string licenseId = e.CommandArgument.ToString();

			switch (e.CommandName)
			{
				case "delItem":
					{
                        var ci = CartProvider.GetCartItems().Where(x => x.LicenseId == Int32.Parse((string)e.CommandArgument)).FirstOrDefault();
                        if (ci != null)
                        {
                            CartProvider.RemoveItem(ci);
                        }
						break;
					}
			}
		}

        protected void CartNavNext_Click(object s, EventArgs e)
        {
            Response.Redirect("cart-details");
        }

        protected void CartNavPrevious_Click(object s, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request["returnUrl"]))
                Response.Redirect(Request["returnUrl"]);
            else
                Response.Redirect("/projects");

        }
    }
}