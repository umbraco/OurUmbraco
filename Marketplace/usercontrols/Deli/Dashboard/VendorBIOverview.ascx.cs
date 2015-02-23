using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Providers.Accounting;

namespace Marketplace.usercontrols.Deli.Dashboard
{
    public enum ChartType
    {
        Revenue,
        Downloads,
        Purchases,
        PageViews
    }

    public partial class VendorBIOverview : System.Web.UI.UserControl
    {

        public int DetailView { get; set; }
        private IDeliOrderItemProvider _orderProvider;
        private IVendorProvider _vendorProvider;
        private IListingProvider _listingProvider;
        private IMemberProvider _memberProvider;

        public string RevenueScript { get; set; }
        public string DownloadsScript { get; set; }
        public string PageViewsScript { get; set; }
        public string PurchasesScript { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            rp_productRepeater.ItemDataBound += new RepeaterItemEventHandler(rp_productRepeater_ItemDataBound);
        }

        void rp_productRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            dynamic item = e.Item.DataItem as dynamic;

            if (e.Item.ItemType.ToString() != "Header" && e.Item.ItemType.ToString() != "Footer")
            {
                Literal revenue = (Literal)e.Item.FindControl("DetailItemRevenue");
                revenue.Text = DeliCurrency.NiceMoney((decimal)item.Revenue, DeliCurrency.FromSymbol("EUR"));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            _vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            _listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            _orderProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];

            BindProducts();
        }

        protected void BindProducts()
        {

            var vendor = _vendorProvider.GetVendorById(_memberProvider.GetCurrentMember().Id);

            var projects = _listingProvider.GetListingsByVendor(vendor.Member.Id, false, true)
                .Where(x => x.ListingType == ListingType.commercial)
                .Select(x => new BIYTDOrderInfo()
            {
                Name = x.Name,
                DetailView = umbraco.library.NiceUrl(DetailView) + "?id=" + x.Id.ToString(),
                PageViews = x.ProjectViews,
                Downloads = x.Downloads,
                OrderItems = _orderProvider.GetOrderItemsByListingId(x.Id, true, true)
            }).ToList();


            BuildChart(vendor, projects, ChartType.Revenue);
            BuildChart(vendor, projects, ChartType.Downloads);
            BuildChart(vendor, projects, ChartType.PageViews);
            BuildChart(vendor, projects, ChartType.Purchases);


            rp_productRepeater.DataSource = projects;
            rp_productRepeater.DataBind();

            DownloadsTotal.Text = projects.Sum(x => x.Downloads).ToString();
            PageViewTotal.Text = projects.Sum(x => x.PageViews).ToString();
            PurchasesTotal.Text = projects.Sum(x => x.Purchases).ToString();
            RevenueTotal.Text = DeliCurrency.NiceMoney((decimal)projects.Sum(x => x.Revenue), DeliCurrency.FromSymbol("EUR"));

            RevenueOverview.Text = RevenueTotal.Text;
            RefundsOverview.Text = DeliCurrency.NiceMoney((decimal)projects.Sum(x => x.Refunds), DeliCurrency.FromSymbol("EUR"));
        }

        private void BuildChart(IVendor vendor, IEnumerable<BIYTDOrderInfo> projects, ChartType chartDataType)
        {
            var projectsList = projects.ToList();
            var count = projects.Count();


            var addColTemplate = "";
            var dataTemplate = "";
            var countTemplate = "";

            switch (chartDataType)
            {
                case ChartType.Revenue:
                    addColTemplate = "revData.addColumn('{0}', '{1}');";
                    dataTemplate = "revData.setValue({0},{1},{2});";
                    countTemplate = string.Format("revData.addRows({0});", count);
                    RevenueScript += string.Format(addColTemplate, "string", "Product");
                    RevenueScript += string.Format(addColTemplate, "number", "Revenue");
                    RevenueScript += countTemplate;
                    break;
                case ChartType.Downloads:
                    addColTemplate = "downData.addColumn('{0}', '{1}');";
                    dataTemplate = "downData.setValue({0},{1},{2});";
                    countTemplate = string.Format("downData.addRows({0});", count);
                    DownloadsScript += string.Format(addColTemplate, "string", "Product");
                    DownloadsScript += string.Format(addColTemplate, "number", "No. of Downloads");
                    DownloadsScript += countTemplate;
                    break;
                case ChartType.Purchases:
                    addColTemplate = "purchasesData.addColumn('{0}', '{1}');";
                    dataTemplate = "purchasesData.setValue({0},{1},{2});";
                    countTemplate = string.Format("purchasesData.addRows({0});", count);
                    PurchasesScript += string.Format(addColTemplate, "string", "Product");
                    PurchasesScript += string.Format(addColTemplate, "number", "No. of Purchases");
                    PurchasesScript += countTemplate;
                    break;
                case ChartType.PageViews:
                    addColTemplate = "pageViewData.addColumn('{0}', '{1}');";
                    dataTemplate = "pageViewData.setValue({0},{1},{2});";
                    countTemplate = string.Format("pageViewData.addRows({0});", count);
                    PageViewsScript += string.Format(addColTemplate, "string", "Product");
                    PageViewsScript += string.Format(addColTemplate, "number", "No. of Page Views");
                    PageViewsScript += countTemplate;
                    break;
                default:
                    break;
            }

            for(var i=0; i < projectsList.Count();i++)
            {

                switch (chartDataType)
                {
                    case ChartType.Revenue:
                        RevenueScript += string.Format(dataTemplate, i, 0, "'" + projectsList[i].Name + "'");
                        RevenueScript += string.Format(dataTemplate, i, 1, projectsList[i].Revenue);
                        break;
                    case ChartType.Downloads:
                        DownloadsScript += string.Format(dataTemplate, i, 0, "'" + projectsList[i].Name + "'");
                        DownloadsScript += string.Format(dataTemplate, i, 1, projectsList[i].Downloads);
                        break;
                    case ChartType.Purchases:
                        PurchasesScript += string.Format(dataTemplate, i, 0, "'" + projectsList[i].Name + "'");
                        PurchasesScript += string.Format(dataTemplate, i, 1, projectsList[i].Purchases);
                        break;
                    case ChartType.PageViews:
                        PageViewsScript += string.Format(dataTemplate, i, 0, "'" + projectsList[i].Name + "'");
                        PageViewsScript += string.Format(dataTemplate, i, 1, projectsList[i].PageViews);
                        break;
                    default:
                        break;
                }
            }
        }

        private IEnumerable<IOrderItem> GetOrderItems(int id)
        {
            return _orderProvider.GetOrderItemsByListingId(id, true, true);

        }

    }

    public class BIYTDOrderInfo
    {
        public string Name { get; set; }
        public string DetailView { get; set; }
        public int PageViews { get; set; }
        public int Downloads { get; set; }
        public double Revenue
        {
            get
            {
                return GetProductRevenue(OrderItems);
            }
        }
        public int Purchases
        {
            get
            {
                return GetProductPurchases(OrderItems);
            }
        }
        public double Refunds
        {
            get
            {
                return GetProductRefunds(OrderItems);
            }
        }
        public IEnumerable<IOrderItem> OrderItems { get; set; }


        private double GetProductRevenue(IEnumerable<IOrderItem> x)
        {
            var orderItems = x.Where(y => !y.IsRefunded);
            var total = 0.00;
            foreach (var i in orderItems)
            {
                total += (i.NetPrice * i.Quantity);
            }
            return total;

        }

        private double GetProductRefunds(IEnumerable<IOrderItem> x)
        {
            var orderItems = x.Where(y => y.IsRefunded);
            var total = 0.00;
            foreach (var i in orderItems)
            {
                total += (i.NetPrice * i.Quantity);
            }

            return total;

        }

        private int GetProductPurchases(IEnumerable<IOrderItem> x)
        {
            var orderItems = x.Where(y => !y.IsRefunded);
            var count = 0;
            foreach (var i in orderItems)
            {
                count += i.Quantity;
            }
            return count;
        }
    }
}