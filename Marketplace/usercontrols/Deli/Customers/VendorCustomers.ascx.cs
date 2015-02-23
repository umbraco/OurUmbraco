using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Marketplace.Interfaces;
using Marketplace.Providers;
using NotificationsCore;
using System.Data;

namespace Marketplace.usercontrols.Deli.Customers
{
    public partial class VendorCustomers : System.Web.UI.UserControl
    {


        private IDeliOrderItemProvider _orderItemProvider;
        private IOrderProvider _orderProvider;
        private IMemberProvider _memberProvider;
        private IVendorProvider _vendorProvider;
        private IListingProvider _listingProvider;
        private ILicenseProvider _licenseProvider;
        private IVendor vendor;


        protected void Page_Load(object sender, EventArgs e)
        {

            _memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
            _vendorProvider = (IVendorProvider)MarketplaceProviderManager.Providers["VendorProvider"];
            _listingProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            _orderProvider = (IOrderProvider)MarketplaceProviderManager.Providers["OrderProvider"];
            _orderItemProvider = (IDeliOrderItemProvider)MarketplaceProviderManager.Providers["OrderItemProvider"];
            _licenseProvider = (ILicenseProvider)MarketplaceProviderManager.Providers["LicenseProvider"];

            IMember mem = _memberProvider.GetCurrentMember();
            vendor = _vendorProvider.GetVendorByGuid(mem.UniqueId);

            if (!IsPostBack)
            {
                BindOrderItems();
            }
        }

        protected void BindOrderItems()
        {
            

            // bind the table to any order that has not been paid out and does not have a payout id
            var items = _orderItemProvider.GetOrderItemsByVendor(vendor.Member.Id)
                .Select(x => new
            {
                Id = x.Id,
                Package = _listingProvider.GetListing(x.ListingItemId,true).Name,
                LicenseTypeName = _licenseProvider.GetLicense(x.LicenseId).LicenseType.ToString(),
                Member = _orderProvider.GetOrder(x.OrderId).Member,
                OrderDate = string.Format("{0:D}",x.CreateDate)
            });

            CustomerList.DataSource = items;
            CustomerList.DataBind();

        }


        protected void ExportToCSV(object s, EventArgs e)
        {

            HttpResponse response = HttpContext.Current.Response;

            response.Clear();
            response.ClearHeaders();
            response.Buffer = true;
            response.ContentType = "application/vnd.ms-excel";
            response.AddHeader("Content-Disposition", "inline;filename=customers.csv");
            response.Charset = "";
            EnableViewState = false;

            // Create the CSV file to which grid data will be exported.
            // First we will write the headers.


            DataTable dt = new DataTable("Subscriptions");


            dt.Columns.Add("Package", Type.GetType("System.String"));
            dt.Columns.Add("LicenseTypeName", Type.GetType("System.String"));
            dt.Columns.Add("Member", Type.GetType("System.String"));
            dt.Columns.Add("Email", Type.GetType("System.String"));
            dt.Columns.Add("Country", Type.GetType("System.String"));
            dt.Columns.Add("OrderDate", Type.GetType("System.String"));


            // bind the table to any order that has not been paid out and does not have a payout id
            var items = _orderItemProvider.GetOrderItemsByVendor(vendor.Member.Id);
             
            foreach (var x in items)
            {
                var member = _orderProvider.GetOrder(x.OrderId).Member;
          
                DataRow dr = dt.NewRow();

                dr["Package"] = _listingProvider.GetListing(x.ListingItemId, true).Name;
                dr["LicenseTypeName"] = _licenseProvider.GetLicense(x.LicenseId).LicenseType.ToString();
                dr["Member"] = member.Name;
                dr["Email"] = member.Email;
                dr["Country"] = member.CompanyCountry;
                dr["OrderDate"] = x.CreateDate.ToString();

                dt.Rows.Add(dr);
            }

            int iColCount = dt.Columns.Count;
            for (int i = 0; i < iColCount; i++)
            {
                response.Output.Write(dt.Columns[i]);
                if (i < iColCount - 1)
                {
                    response.Output.Write(",");
                }
            }
            Response.Output.Write("\n");
            // Now write all the rows.
            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i < iColCount; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        if (dr[i].ToString().Contains(","))
                        {
                            response.Output.Write("\"" + dr[i].ToString().Replace("\n", "").Replace("\r", "") + "\"");
                        }
                        else
                        {
                            response.Output.Write(dr[i].ToString().Replace("\n", "").Replace("\r", ""));
                        }
                    }
                    if (i < iColCount - 1)
                    {
                        response.Output.Write(",");
                    }
                }
                response.Output.Write("\n");
            }
            response.Output.Close();
            response.End();
        }

    }
}