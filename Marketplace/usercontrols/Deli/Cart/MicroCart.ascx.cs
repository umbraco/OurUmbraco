using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Accounting;

namespace Marketplace.usercontrols.Deli.Cart
{
    public partial class MicroCart : System.Web.UI.UserControl
    {
        private int _numberOfItems = 0;
        private double _cartTotal = 0;
        private ICart cart;
        private Currency _currency = Currency.EUR;



        protected void Page_PreRender(object sender, EventArgs e)
        {

            var cartProvider = (ICartProvider)MarketplaceProviderManager.Providers["CartProvider"];
            cart = cartProvider.GetCurrentCart();

            _currency = cart.Currency;

            _numberOfItems = cartProvider.GetCartItemCount();


            this.Visible = true;
            _cartTotal = cartProvider.GetLocalizedCartGrossValue();


            NumItems.Text = (_numberOfItems == 1) ? _numberOfItems + " Item" : _numberOfItems + " Items";
            CartTotal.Text = DeliCurrency.NiceMoney((decimal)_cartTotal, DeliCurrency.FromSymbol(_currency.ToString()));

            if (_numberOfItems > 0)
            {
                microCartHolder.Visible = true;
            }
            else
            {
                microCartHolder.Visible = false;

            }
        }
    }
}