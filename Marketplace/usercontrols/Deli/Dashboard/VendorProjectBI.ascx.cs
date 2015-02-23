using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Data;
using Marketplace.Providers.Accounting;
using System.Diagnostics;

namespace Marketplace.usercontrols.Deli.Dashboard
{
    public partial class VendorProjectBI : System.Web.UI.UserControl
    {

        private int _projectId;
        public int ProjectId
        {
            get
            {
                if (!string.IsNullOrEmpty(Request["id"]))
                {
                    _projectId = Int32.Parse(Request["id"]);
                }
                return _projectId;
            }
            set
            {
                _projectId = value;
            }
        }

        private List<IOrderItem> _orderItems;
        public List<IOrderItem> OrderItems
        {
            get
            {
                if (_orderItems == null)
                    _orderItems = _orderItemProvider.GetOrderItemsByListingId(_projectId, true, true).ToList();
                return _orderItems;
            }
        }

        public string PageViewsScript { get; set; }

        private ILicenseProvider _licenseProvider;
        private IMemberLicenseProvider _memberLicenseProvider;
        private IDeliOrderItemProvider _orderItemProvider;
        private IOrderProvider _orderProvider;
        private IVendorProvider _vendorProvider;
        private IListingProvider _listingProvider;
        private IMemberProvider _memberProvider;
        private IListingItem _project;

        protected void Page_Load(object sender, EventArgs e)
        {
            _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            _vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            _listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            _orderItemProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];
            _orderProvider = (IOrderProvider)MarketplaceProviderManager.Providers["OrderProvider"];
            _memberLicenseProvider = (IMemberLicenseProvider)MarketplaceProviderManager.Providers["MemberLicenseProvider"];
            _licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            _project = _listingProvider.GetListing(ProjectId);
            LoadProjectBI();
        }

        protected void LoadProjectBI()
        {
            ProjectName.Text = _project.Name;
            RevenueOverview.Text = DeliCurrency.NiceMoney((decimal)GetProductRevenue(ProjectId), DeliCurrency.FromSymbol("EUR"));
            RefundOverview.Text = DeliCurrency.NiceMoney((decimal)GetProductRefunds(ProjectId), DeliCurrency.FromSymbol("EUR"));
            DownloadsOverview.Text = _project.Downloads.ToString();
            PurchasesOverview.Text = GetProductPurchases(ProjectId).ToString();
            PageViewOverview.Text = _project.ProjectViews.ToString();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            BindLicenseStats();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Debug.WriteLine("BindLicense Stats RunTime:- " + elapsedTime);

            stopWatch.Start();
            BindOrdersTable();
            stopWatch.Stop();
            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Debug.WriteLine("BindOrdersTable RunTime:- " + elapsedTime);

            BuildChartPageView(DateTime.Now.AddDays(-7), DateTime.Now);

        }

        protected void repeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            dynamic item = e.Item.DataItem as dynamic;

            if (e.Item.ItemType.ToString() != "Header" && e.Item.ItemType.ToString() != "Footer")
            {
                Literal revenue = (Literal)e.Item.FindControl("DetailItemRevenue");
                revenue.Text = DeliCurrency.NiceMoney((decimal)item.Revenue, DeliCurrency.FromSymbol("EUR"));
            }
        }


        protected void BindLicenseStats()
        {
            var licenses = _licenseProvider.GetProjectLicenses(_project.ProjectGuid).Select(x => new LicenseStatObj()
            {
                TypeSold = x.LicenseType.ToString(),
                LicensesSold = GetSold(x.Id),
                Revenue = GetRevenue(x.Id)
            });

            LicensesSold.DataSource = licenses;
            LicensesSold.DataBind();
        }

        private double GetRevenue(int p)
        {
            return OrderItems.Where(x =>!x.IsRefunded) 
                .Where(x => x.LicenseId == p)
                .Sum(x => x.NetPrice);
        }

        private void BindOrdersTable()
        {
            var orderItems = OrderItems.Select(x => new
            {
                OrderId = x.OrderId,
                Order = _orderProvider.GetOrder(x.OrderId,true),
                LicenseSold = _licenseProvider.GetLicense(x.LicenseId).LicenseType.ToString(),
                Quantity = x.Quantity,
                Refunded = x.IsRefunded,
                Revenue = x.NetPrice,
                CssClass = x.IsRefunded ? "refund" : ""
            }).OrderByDescending(x => x.Order.PaymentDate).ToList();

            OrdersRepeater.DataSource = orderItems;
            OrdersRepeater.DataBind();
        }


        private IEnumerable<IMemberLicense> GetSold(int licenseId)
        {
            return _memberLicenseProvider.GetLicensesByLicenseTypeId(licenseId);
        }


        private double GetProductRevenue(int p)
        {
            var orderItems = OrderItems.Where(x=>!x.IsRefunded);
            var total = 0.00;
            foreach (var i in orderItems)
            {
                total += (i.NetPrice * i.Quantity);
            }
            return total;

        }

        private double GetProductRefunds(int p)
        {
            var orderItems = OrderItems.Where(x => x.IsRefunded);
            var total = 0.00;
            foreach (var i in orderItems)
            {
                total += (i.NetPrice * i.Quantity);
            }

            return total;

        }

        private int GetProductPurchases(int p)
        {
            var orderItems = OrderItems.Where(x => !x.IsRefunded);
            var count = 0;
            foreach (var i in orderItems)
            {
                count += i.Quantity;
            }
            return count;
        }

        private void BuildChartPageView(DateTime startDate, DateTime endDate)
        {

            var ctx = new MarketplaceDataContext();
            var pageViews = ctx.DeliProjectViews.Where(x => x.ProjectId == _projectId && x.CreateDate >= startDate && x.CreateDate <= endDate).OrderBy(x => x.CreateDate).ToList();
            var timeSpan = (endDate - startDate).TotalDays;

           
            var addColTemplate = "";
            var dataTemplate = "";
            var countTemplate = "";


            addColTemplate = "data.addColumn('{0}', '{1}');";
            dataTemplate = "data.setValue({0},{1},{2});";
            countTemplate = string.Format("data.addRows({0});", timeSpan);
            PageViewsScript += string.Format(addColTemplate, "string", "Date");
            PageViewsScript += string.Format(addColTemplate, "number", "Page Views");
            PageViewsScript += countTemplate;

            for (var i = 0; i < timeSpan; i++)
            {
                var thisDate = (startDate.AddDays(i+1));
                PageViewsScript += string.Format(dataTemplate, i, 0, "'" + thisDate.ToString("d") + "'");
                var valForDate = pageViews.Where(x => x.CreateDate.ToShortDateString() == thisDate.ToShortDateString()).Count();
                PageViewsScript += string.Format(dataTemplate, i, 1, valForDate);
            }   
          
        }


    }

    public class LicenseStatObj
    {
        public string TypeSold { get; set; }
        public IEnumerable<IMemberLicense> LicensesSold { get; set; }
        public double Revenue { get; set; }
    }

}