using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marketplace.Interfaces;
using Marketplace.Providers;
using umbraco;
using our;
using System.Text.RegularExpressions;
using umbraco.NodeFactory;
using System.Globalization;
using Marketplace.Data;

namespace uProject
{
    [XsltExtension("deli.library")]
    public class library
    {

        public static bool HasVendorAccess(int pageId)
        {
            var page = new umbraco.NodeFactory.Node(pageId);

            if (page.GetProperty("vendorOnly") != null)
            {
                var memberProvider = (IMemberProvider)MarketplaceProviderManager.Providers["MemberProvider"];
                var member = memberProvider.GetCurrentMember();

                var vendorOnly = page.GetProperty("vendorOnly").Value == "1" ? true : false;
                if (vendorOnly)
                {
                    if (member.IsDeliVendor)
                        return true;
                    else
                        return false;
                }
                return true;
            } 
            return true;

        }

        public static string ShortenText(string text)
        {
            text = Utils.StripHTML(text).Replace("&nbsp;", "");
            if (text.Length > 210)
            {

                text = text.Substring(0, 210);
                text = text.Substring(0, text.LastIndexOf(' '));
            }

            text = Regex.Replace(text, @"[^a-zA-Z0-9\s.?!&;]", ""); //strip all crap from the listing such as ======================================= !!!!
            return text.Trim();
        }



        public static string GetTagsAsCSV(IEnumerable<IProjectTag> tagList)
        {
            var tags = string.Empty;
            int counter = 0;

            foreach (var t in tagList)
            {
                tags += t.Text + ", ";
                counter++;

                if (counter > 3)
                    break;
            }

            if (tags.Length > 0)
            {
                tags = tags.Substring(0, tags.Length - 2);
            }
            return tags;
        }

        public static string GetCategoryName(int projectId)
        {
            var node = new Node(projectId);
            return node.Parent.Name; 

        }




        public static string GetDefaultScreenshot(string prop)
        {
            return !String.IsNullOrEmpty(prop) ? "/umbraco/imagegen.ashx?image=" + prop + "&amp;pad=true&amp;width=50&amp;height=50;" : "/css/img/package2.png";
        }

        public static string GetManufacturerName(IVendor vendor)
        {
            if (!string.IsNullOrEmpty(vendor.VendorCompanyName))
            {
                return vendor.VendorCompanyName;
            }
            else
            {
                return vendor.Member.Name;
            }

        }

        public static string GetCountry(string code)
        {


            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {

                RegionInfo ri = new RegionInfo(ci.LCID);
                if (ri.TwoLetterISORegionName.ToLowerInvariant() == code)
                    return ri.EnglishName;
            }

            return "";

        }


        /// <summary>
        /// Check to see if member has reported on compatibility for a package regardless of version of the package
        /// </summary>
        /// <param name="memberId">The members Id</param>
        /// <param name="packageId">The package Id</param>
        /// <returns></returns>
        public static bool HasDownloaded(int memberId, int packageId)
        {
            using (var ctx = new MarketplaceDataContext())
            {
                var downs = ctx.projectDownloads.Where(x => x.memberId == memberId && x.projectId == packageId);
                if (downs.Count() > 0)
                    return true;
                return false;

            }
        }
    }
}