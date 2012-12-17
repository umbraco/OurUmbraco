using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uWiki.Businesslogic;

namespace uWiki {
    public class WikiFileUploadHandler : IHttpHandler {

        #region IHttpHandler Members

        public bool IsReusable {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context) {

            HttpPostedFile file = context.Request.Files["Filedata"];
            string userguid = context.Request.Form["USERGUID"];
            string nodeguid = context.Request.Form["NODEGUID"];
            string fileType = context.Request.Form["FILETYPE"];
            string fileName = context.Request.Form["FILENAME"];
            string umbraoVersion = context.Request.Form["UMBRACOVERSION"];

            List<UmbracoVersion> v = new List<UmbracoVersion>(){Businesslogic.UmbracoVersion.DefaultVersion()};

            if (!string.IsNullOrEmpty(umbraoVersion))
            {
                v.Clear();
                v = Businesslogic.WikiFile.GetVersionsFromString(umbraoVersion);
            }

            if (!string.IsNullOrEmpty(userguid) && !string.IsNullOrEmpty(nodeguid) && !string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(fileName))
                uWiki.Businesslogic.WikiFile.Create(fileName, new Guid(nodeguid), new Guid(userguid), file, fileType, v);
        }

        #endregion
    }
}
