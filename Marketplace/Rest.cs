using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marketplace.Interfaces;
using Marketplace.Providers;
using Marketplace.Data;
using System.Web.Security;
using Umbraco.Web.BaseRest;

namespace Marketplace
{
    [RestExtension("deli")]
    public class Rest
    {
        /// <summary>
        /// Gets the popular listings.
        /// </summary>
        /// <param name="listingType">Type of the listing.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        [RestExtensionMethod(ReturnXml = false)]
        public static string GetPopularListings(string listingType, int skip, int take)
        {

            var ProjectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

            IEnumerable<IListingItem> Projects;

            switch (listingType)
            {
                case "free":
                    Projects = ProjectsProvider.GetListingsByPopularity(skip, take, true, false).Where(x => x.ListingType == Interfaces.ListingType.free).Take(take);
                    break;

                case "commercial":
                    Projects = ProjectsProvider.GetTopPaidListings(skip, take, true, false);
                    break;

                default:
                    Projects = ProjectsProvider.GetListingsByPopularity(skip, take, true, false);
                    break;
            }


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(Projects.Select(x => new 
            { 
                Name = x.Name,
                CategoryName = Marketplace.library.GetCategoryName((int)x.Id),
                DefaultScreenShot = GetScreenShot(x.DefaultScreenshot),
                Description = Marketplace.library.ShortenText(x.Description.ToString()),
                NiceUrl = x.NiceUrl,
                ListingType = x.ListingType.ToString(),
                Karma = x.Karma,
                Downloads = x.Downloads

            }));
        }

        /// <summary>
        /// Gets the newest listings.
        /// </summary>
        /// <param name="listingType">Type of the listing.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        [RestExtensionMethod(ReturnXml = false)]
        public static string GetNewestListings(string listingType, int skip, int take)
        {

            var ProjectsProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];

            IEnumerable<IListingItem> Projects;

            switch (listingType)
            {
                case "free":
                    Projects = ProjectsProvider.GetListingsByLatest(skip, take, ListingType.free, true, false);
                    break;

                case "commercial":
                    Projects = ProjectsProvider.GetListingsByLatest(skip, take, ListingType.commercial, true, false);
                    break;

                default:
                    Projects = ProjectsProvider.GetListingsByLatest(skip, take, true, false);
                    break;
            }


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(Projects.Select(x => new 
            { 
                Name = x.Name,
                CategoryName = Marketplace.library.GetCategoryName((int)x.Id),
                DefaultScreenShot = GetScreenShot(x.DefaultScreenshot),
                Description = Marketplace.library.ShortenText(x.Description.ToString()),
                NiceUrl = x.NiceUrl,
                ListingType = x.ListingType.ToString(),
                Karma = x.Karma,
                Downloads = x.Downloads

            }));
        }

        [RestExtensionMethod(ReturnXml = false)]
        public static string CompatibilityReport(int projectId, int fileId, string compatArr)
        {
            using (var ctx = new MarketplaceDataContext())
            {
                int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;
                var versionData = compatArr.Split(',').Select(x => new { version = x.Split('^')[0], compat = x.Split('^')[1] });


                // load all the existing member reports for this package by this member.  This is to stop multiple insertions of reports for a single member
                var memberReports = ctx.DeliVersionCompatibilities.Where(x => x.memberId == _currentMember && x.fileId == fileId && x.projectId == projectId).ToList();

               

                foreach (var ver in versionData)
                {
                    if (ver.compat == "1" || ver.compat == "0")
                    {
                        var compVer = new DeliVersionCompatibility() { fileId = fileId, memberId = _currentMember, isCompatible = ver.compat == "1" ? true : false, dateStamp = DateTime.Now, version = ver.version, projectId = projectId };
                        
                        //try find an existing report
                        var existingReport = memberReports.Where(x => x.fileId == compVer.fileId && x.memberId == compVer.memberId && x.projectId == compVer.projectId && x.version == compVer.version).FirstOrDefault();
                        if (existingReport != null)
                        {
                            existingReport.isCompatible = compVer.isCompatible;
                            existingReport.dateStamp = DateTime.Now;
                        }
                        else
                        {
                            // otherwise insert the new compat ver report
                            ctx.DeliVersionCompatibilities.InsertOnSubmit(compVer);
                        }

                    }

                }
                ctx.SubmitChanges();

                //give them some Karma if they haven't reported on this before.
                if (memberReports.Count() == 0)
                {
                    uPowers.Library.Rest.Action("ProjectVersionVote", fileId);
                }
            }
            return "got it thanks";
        }



        private static string GetScreenShot(string ss){
            if(!string.IsNullOrEmpty(ss)){
                return "/umbraco/imagegen.ashx?image="+ ss +"&amp;pad=true&amp;width=50&amp;height=50;";
            }
            else return "/css/img/package2.png";
        }

    }
}

