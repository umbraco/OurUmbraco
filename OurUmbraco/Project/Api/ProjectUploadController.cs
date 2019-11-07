using OurUmbraco.Wiki.BusinessLogic;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Project.Api
{
    public class ProjectUploadController : UmbracoApiController
    {
        // http://localhost:24292/Umbraco/Api/ProjectUpload/GetPing
        // http://our.umbraco.local/Umbraco/Api/ProjectUpload/GetPing
        public string GetPing()
        {
            return "pong";
        }

        public List<WikiFile> GetProjectFiles()
        {
            // No param of Project ID
            // As this lives in the JWT which is decoded with the Auth Attribute

            var files = WikiFile.CurrentFiles(1179); //CWS SK
            return files.Where(x => x.FileType == "package" || x.FileType == "hotfix").OrderByDescending(x => x.Version.Version).ToList();
        }


        // TODO: Hartvig does his magic for upload
    }
}
