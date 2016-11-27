using System;
using System.Collections.Generic;
using System.Web;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.Our;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;

namespace OurUmbraco.Project
{
    public class ProjectFileUploadHandler : IHttpHandler {

        #region IHttpHandler Members

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {

            HttpPostedFile file = context.Request.Files["Filedata"];
            string userguid = context.Request.Form["USERGUID"];
            string projectguid = context.Request.Form["NODEGUID"];
            string nodeId = context.Request.Form["id"];
            string fileType = context.Request.Form["FILETYPE"];
            string fileName = context.Request.Form["FILENAME"];
            string umbraoVersion = (context.Request.Form["UMBRACOVERSION"] != null) ? context.Request.Form["UMBRACOVERSION"] : "nan";
            string dotNetVersion = (context.Request.Form["DOTNETVERSION"] != null) ? context.Request.Form["DOTNETVERSION"] : "nan";
            string trustLevel = (context.Request.Form["TRUSTLEVEL"] != null) ? context.Request.Form["TRUSTLEVEL"] : "nan";




            List<OurUmbraco.Wiki.BusinessLogic.UmbracoVersion> v = new List<OurUmbraco.Wiki.BusinessLogic.UmbracoVersion>() { OurUmbraco.Wiki.BusinessLogic.UmbracoVersion.DefaultVersion() };

            if (!string.IsNullOrEmpty(umbraoVersion))
            {
                v.Clear();
                v = WikiFile.GetVersionsFromString(umbraoVersion);
            }

            
            bool trust = false;
            if(trustLevel == "Medium"){
                trust = true;
            }


            if (!string.IsNullOrEmpty(userguid) && !string.IsNullOrEmpty(projectguid) && !string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(fileName))
            {
                var nodeListingProvider = new OurUmbraco.MarketPlace.NodeListing.NodeListingProvider();
                var p = nodeListingProvider.GetListing(new Guid(projectguid));
                
                Member mem = new Member(new Guid(userguid));

                if (p.VendorId == mem.Id || Utils.IsProjectContributor(mem.Id, p.Id))
                {
                    var mediaProvider = new OurUmbraco.MarketPlace.Providers.MediaProvider();

                    var packageFileType = (FileType)Enum.Parse(typeof(FileType), (string)fileType , true);
                    // TODO: Don't know how else to get the bloody version
                    var version = ApplicationContext.Current.Services.ContentService.GetById(p.Id).Version;
                    mediaProvider.CreateFile(fileName, version, mem.UniqueId, file, packageFileType, v, dotNetVersion, trust);

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
