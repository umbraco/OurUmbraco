using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Providers.Orders;
using Marketplace.Providers;
using Marketplace.Interfaces;
using Marketplace.Providers.OrderNote;
using Marketplace.Providers.Listing;
using System.Globalization;
using umbraco.BasePages;
using System.Web.UI.HtmlControls;
using umbraco;
using umbraco.cms.businesslogic.member;

namespace Marketplace.usercontrols.Deli.Admin
{
    public partial class OrderReview : System.Web.UI.UserControl
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

        private ITaxProvider _taxProvider;
        public ITaxProvider TaxProvider
        {
            get
            {
                if (_taxProvider != null)
                    return _taxProvider;
                else
                {
                    _taxProvider = (ITaxProvider)MarketplaceProviderManager.Providers["TaxProvider"];
                    return _taxProvider;
                }
            }
        }

        #endregion


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

        protected void RowCommand(object s, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ValidateVat":
                    var but = (Button)s;
                    var lit = (Literal)but.Parent.FindControl("IsValid");

                    if (!string.IsNullOrEmpty((string)e.CommandArgument))
                    {
                        var valid = TaxProvider.SimpleVatValidate((string)e.CommandArgument);
                        lit.Text = valid ? "<span style=\"background:green;padding:3px;text-tranform:uppercase;color:#fff;\">VALID</span>" :
                            "<span style=\"background:red;padding:3px;text-tranform:uppercase;color:#fff;\">INVALID</span>";
                    }
                    else
                    {
                        lit.Text = "<span style=\"background:red;padding:3px;text-tranform:uppercase;color:#fff;\">INVALID</span>";
                    }
                    break;
                default:
                    break;
            }

        }


        protected void GetOrdersButton_Click(object sender, EventArgs e)
        {
            OrderView.ItemDataBound += new RepeaterItemEventHandler(OrderList_ItemDataBound);
            
            DateTime endDate = DateTime.Now.AddDays(int.Parse(DateRangeDdl.SelectedValue) * (-1));
            var Orders = OrderProvider.GetOrdersByDate(endDate).Select(x => new
            {
                Id = x.Id,
                EconomicId = x.OrderId,
                Status = x.Status,
                CssClass = x.Status == OrderStatus.Confirmed ? "confirmed" : "notConfirmed",
                Company = x.CompanyName,
                VatID = x.Member.CompanyVATNumber,
                InvoiceEmail = x.CompanyInvoiceEmail,
                Country = GetCountryFromCode(x.CompanyCountry),
                Date = x.CreateDate,
                TaxTotal = x.TaxTotal.ToString("c", new CultureInfo("fr-FR", false)),
                SubTotal = x.SubTotal.ToString("c", new CultureInfo("fr-FR", false)),
                Total = x.Total.ToString("c", new CultureInfo("fr-FR", false)),
                Currency = x.Currency,
                Refunded = x.IsRefunded,
                RefundDate = x.RefundDate
            }).OrderByDescending(x => x.Id);

            OrderView.DataSource = Orders;
            OrderView.DataBind();
        }

        protected void OrderList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            RepeaterItem ri = (RepeaterItem)e.Item;
            if (ri.ItemType == ListItemType.Item || ri.ItemType == ListItemType.AlternatingItem)
            {
                Repeater itemRepeater = ((RepeaterItem)e.Item).FindControl("orderItemRepeater") as Repeater;
                int rowId = Int32.Parse((((RepeaterItem)e.Item).FindControl("rowId") as HiddenField).Value);

                var orderItems = ItemProvider.GetOrderItems(rowId).Select(x => new
                {
                    Quantity = x.Quantity,
                    Package = ListingProvider.GetListing(x.ListingItemId, true).Name,
                    Vendor = new Member(ListingProvider.GetListing(x.ListingItemId, true).VendorId).Text,
                    LicenseTypeName = LicenseProvider.GetLicense(x.LicenseId).LicenseType.ToString(),
                    Value = x.NetPrice.ToString("c", new CultureInfo("fr-FR", false)),
 
                });

                itemRepeater.DataSource = orderItems;
                itemRepeater.DataBind();
            }

        }


        private string GetCountryFromCode(string code){

           string country = "";
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {

                    RegionInfo ri = new RegionInfo(ci.LCID);
                if(ri.TwoLetterISORegionName.ToLowerInvariant() == code.ToLowerInvariant()){
                    country = ri.EnglishName;
                    break;
                }
            }
            return country;
        }
    }
}


