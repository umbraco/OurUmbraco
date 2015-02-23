using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.cms.businesslogic.member;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Orders;
using Marketplace.Providers.Members;
using Marketplace.Providers.OrderItem;
using System.Web.Security;
using NotificationsCore;
using NotificationsWeb;
using umbraco.NodeFactory;

namespace Marketplace.usercontrols.Deli.Cart
{
    public partial class CartDetails : System.Web.UI.UserControl
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

        public int RetrievePassword { get; set; }
        public int SignUp { get; set; }

        protected string RetrievePasswordPage { get; set; }
        protected string SignUpPage { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(ShoppingCart.CartItems.Count > 0)
            {
                if (!IsPostBack && Member != null)
                {
                    //check to see if member was created during cart process and move them to review step automatically
                    if (!string.IsNullOrEmpty(Request["ss"]))
                    {

                        //now that the user is authenticated set their cart currency based on their details
                        SetCurrency();

                        CartProvider.InvalidateVat();
                        // send to order review 
                        Response.Redirect("cart-review");
                    }

                    //otherwise show the form
                    SetupDetails();
                }
            }
            else
            {
                Response.Redirect("cart");
            }

            //check if the user is authenticated, otherwise present login option or continue as 'guest'
            // can we just use... if(Member == null) for this
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                RetrievePasswordPage = umbraco.library.NiceUrl(RetrievePassword);
                SignUpPage = umbraco.library.NiceUrl(SignUp);
                MemberLogin.NextPage = Node.GetCurrent().Id;
                NotLoggedIn.Visible = true;
            }
            
        }

        

        protected void SetupDetails()
        {
            UserName.Text = Member.Name;
            UserEmail.Text = Member.Email;
            CompanyVat.Text = Member.CompanyVATNumber;
            CompanyName.Text = Member.Company;
            CompanyCountry.SelectedValue = Member.CompanyCountry;
            CompanyAddress.Text = Member.CompanyAddress;
            CompanyInvoiceEmail.Text = Member.CompanyInvoiceEmail;
        }

        protected void CreateMember()
        {
            string Group = "standard";

            if (UserEmail.Text != null)
            {
                IMember m = MemberProvider.GetMemberByEmail(UserEmail.Text);
                if (m == null)
                {

                    m = new DeliMember();

                    m.Name = UserName.Text;
                    m.Email = UserEmail.Text;
                    // set using generate password
                    m.Password = Membership.GeneratePassword(8, 0);


                    //billing info
                    m.CompanyAddress = CompanyAddress.Text;
                    m.CompanyCountry = CompanyCountry.SelectedValue;

                    if (CompanyInvoiceEmail.Text.Length != 0)
                        m.CompanyInvoiceEmail = CompanyInvoiceEmail.Text;
                    else
                        m.CompanyInvoiceEmail = UserEmail.Text;
                    
                    m.CompanyVATNumber = CompanyVat.Text;

                    //Standard values
                    m.ReputationTotal = 20;
                    m.ReputationCurrent = 20;
                    m.ForumPosts = 0;

                    m.IsVerified = true;
                    m.TermsOfServiceAcceptanceDate = DateTime.Now.ToUniversalTime();
                    m.CreatedByDeli = true;

                    //add the group
                    m.Groups = new string[] { Group };

                    MemberProvider.SaveOrUpdate(m);

                    // send welcome mail - have to do it here so we have access to the password
                    InstantNotification not = new InstantNotification();
                    not.Invoke(Config.ConfigurationFile, Config.AssemblyDir, "DeliNewMember", m, m.Password);
                    
                    // redirect to force Auth cookie to establish
                    Response.Redirect(Request.RawUrl + "?ss=" + Guid.NewGuid().ToString());

                }
            }
        }

        private void SetCurrency()
        {
            if (Member.CompanyCountry.ToLower() != null)
            {

                switch (Member.CompanyCountry)
                {
                    case "dk":
                        CartProvider.SetCurrency(Currency.DKK);
                        break;

                    case "us":
                    case "ca":
                        CartProvider.SetCurrency(Currency.USD);
                        break;

                    default:
                        CartProvider.SetCurrency(Currency.EUR);
                        break;
                }
            }
        }

        protected void CartNavNext_Click(object s, EventArgs e)
        {
            try
            {
                // create new member if not logged in
                if (Member == null)
                {
                    CreateMember();
                }
                // persist details to member
                Member.Company = CompanyName.Text;
                Member.CompanyVATNumber = CompanyVat.Text;
                Member.CompanyCountry = CompanyCountry.SelectedValue;
                Member.CompanyAddress = CompanyAddress.Text;

                if (CompanyInvoiceEmail.Text.Length != 0)
                    Member.CompanyInvoiceEmail = CompanyInvoiceEmail.Text;
                else
                    Member.CompanyInvoiceEmail = UserEmail.Text;

                /* 
                 * if the member changes the billing country during the order we need to update their profile billing details and change
                 * currency where applicable
                 */
                SetCurrency();

                MemberProvider.SaveOrUpdate(Member);

                //invalidate the Vat Number to force recheck;
                CartProvider.InvalidateVat();

                // send to order review 
                Response.Redirect("cart-review");

            }
            catch (Exception ex)
            {
                // put up error and apologize
                DeliMessage.Text = "Oops!  Sorry, little help here:<br />" + ex.Message + "<br />" + ex.InnerException + "<br />" + ex.StackTrace + "<br />";
            }
        }

        protected void CartNavPrevious_Click(object s, EventArgs e)
        {
            try
            {
                // persist details to member
                Member.Company = CompanyName.Text;
                Member.CompanyVATNumber = CompanyVat.Text;

                //invalidate the Vat Number to force recheck;
                CartProvider.InvalidateVat();

                Member.CompanyCountry = CompanyCountry.SelectedValue;
                Member.CompanyAddress = CompanyAddress.Text;

                if (CompanyInvoiceEmail.Text.Length != 0)
                    Member.CompanyInvoiceEmail = CompanyInvoiceEmail.Text;
                else
                    Member.CompanyInvoiceEmail = UserEmail.Text;

                MemberProvider.SaveOrUpdate(Member);

                // send to order review 
                Response.Redirect("cart");

            }
            catch (Exception ex)
            {
                // put up error and apologize
                DeliMessage.Text = "Oops!  Sorry, little help here:<br />" + ex.Message + "<br />" + ex.InnerException + "<br />" + ex.StackTrace + "<br />";
            }
        }
    }
}