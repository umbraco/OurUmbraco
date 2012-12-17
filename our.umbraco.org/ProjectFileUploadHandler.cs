using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using uWiki.Businesslogic;

namespace our {
    public class ProjectFileUploadHandler : IHttpHandler {

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

            List<UmbracoVersion> v = new List<UmbracoVersion>() { UmbracoVersion.DefaultVersion() };

            if (!string.IsNullOrEmpty(umbraoVersion))
            {
                v.Clear();
                v = WikiFile.GetVersionsFromString(umbraoVersion);
            }

            if (!string.IsNullOrEmpty(userguid) && !string.IsNullOrEmpty(nodeguid) && !string.IsNullOrEmpty(fileType) && !string.IsNullOrEmpty(fileName)) {

                Document d = new Document( Document.GetContentFromVersion(new Guid(nodeguid)).Id );
                Member mem = new Member(new Guid(userguid));

                if (d.ContentType.Alias == "Project" && d.getProperty("owner") != null && (d.getProperty("owner").Value.ToString() == mem.Id.ToString() ||  Utills.IsProjectContributor(mem.Id,d.Id))) {
                    uWiki.Businesslogic.WikiFile.Create(fileName, new Guid(nodeguid), new Guid(userguid), file, fileType, v);

                    //the package publish handler will make sure we got the right versions info on the package node itself.
                    //ProjectsEnsureGuid.cs is the handler
                    if (fileType.ToLower() == "package")
                    {
                        d.Publish(new umbraco.BusinessLogic.User(0));
                        umbraco.library.UpdateDocumentCache(d.Id);
                    }
                } else {
                    umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Debug, 0, "wrong type or not a owner");
                }
            
            }
        }

        #endregion
    }
}
