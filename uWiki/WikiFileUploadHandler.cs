using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uWiki.Businesslogic;

namespace uWiki
{
    public class WikiFileUploadHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {

            //TODO: Authorize this request!!!

            var file = context.Request.Files["Filedata"];
            var userguid = context.Request.Form["USERGUID"];
            var nodeguid = context.Request.Form["NODEGUID"];
            var fileType = context.Request.Form["FILETYPE"];
            var fileName = context.Request.Form["FILENAME"];
            var umbracoVersion = context.Request.Form["UMBRACOVERSION"];

            var versions = new List<UmbracoVersion> { UmbracoVersion.DefaultVersion() };

            if (string.IsNullOrWhiteSpace(umbracoVersion) == false)
            {
                versions.Clear();
                versions = WikiFile.GetVersionsFromString(umbracoVersion);
            }

            if (string.IsNullOrWhiteSpace(userguid) == false
                && string.IsNullOrWhiteSpace(nodeguid) == false
                && string.IsNullOrWhiteSpace(fileType) == false
                && string.IsNullOrWhiteSpace(fileName) == false)
            {
                WikiFile.Create(fileName, new Guid(nodeguid), new Guid(userguid), file, fileType, versions);
            }
        }

        #endregion
    }
}
