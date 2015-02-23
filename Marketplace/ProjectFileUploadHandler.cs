using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using Marketplace.Interfaces;
using Marketplace.Providers;
using our;
using Marketplace.Providers.MediaFile;
using Marketplace.Umbraco.BusinessLogic;

namespace Marketplace {
    public class ProjectFileUploadHandler : IHttpHandler {

        #region IHttpHandler Members

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {

            HttpPostedFile file = context.Request.Files["Filedata"];
            string userguid = context.Request.Form["USERGUID"];
            string projectguid = context.Request.Form["NODEGUID"];
            string fileType = context.Request.Form["FILETYPE"];
            string fileName = context.Request.Form["FILENAME"];
            string umbraoVersion = (context.Request.Form["UMBRACOVERSION"] != null) ? context.Request.Form["UMBRACOVERSION"] : "nan";
            string dotNetVersion = (context.Request.Form["DOTNETVERSION"] != null) ? context.Request.Form["DOTNETVERSION"] : "nan";
            string trustLevel = (context.Request.Form["TRUSTLEVEL"] != null) ? context.Request.Form["TRUSTLEVEL"] : "nan";




            List<UmbracoVersion> v = new List<UmbracoVersion>() { UmbracoVersion.DefaultVersion() };

            if (!string.IsNullOrEmpty(umbraoVersion))
            {
                v.Clear();
                v = GetVersionsFromString(umbraoVersion);
            }

            
            bool trust = false;
            if(trustLevel == "Medium"){
                trust = true;
            }


            if (!string.IsNullOrEmpty(userguid) && !string.IsNullOrEmpty(projectguid) && !string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(fileName)) {

                var provider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
                var p = provider.GetListing(new Guid(projectguid));
                Member mem = new Member(new Guid(userguid));

                if (p.Vendor != null && p.Vendor.Member.Id == mem.Id || Utills.IsProjectContributor(mem.Id, p.Id))
                {

                    var fileProvider = (IMediaProvider)MarketplaceProviderManager.Providers["MediaProvider"];

                    var packageFileType = (FileType)Enum.Parse(typeof(FileType), (string)fileType , true);

                    fileProvider.CreateFile(fileName, p.Version, mem.UniqueId, file, packageFileType, v, dotNetVersion, trust);

                } else {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 0, "wrong type or not a owner");
                }
            
            }
        }

        public static List<UmbracoVersion> GetVersionsFromString(string p)
        {
            var verArray = p.Split(',');
            var verList = new List<UmbracoVersion>();
            foreach (var ver in verArray)
            {
                verList.Add(UmbracoVersion.AvailableVersions()[ver]);
            }
            return verList;
        }


        #endregion
    }
}
