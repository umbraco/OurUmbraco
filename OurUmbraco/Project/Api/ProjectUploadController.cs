using OurUmbraco.Auth;
using OurUmbraco.Wiki.BusinessLogic;
using System.Collections.Generic;
using System.Linq;

namespace OurUmbraco.Project.Api
{
    [UmbracoMemberAuthToken()]
    public class ProjectUploadController : UmbracoMemberAuthApiController
    {
        // http://localhost:24292/Umbraco/Api/ProjectUpload/GetPing
        // http://our.umbraco.local/Umbraco/Api/ProjectUpload/GetPing
        public string GetPing()
        {
            return $"pong Member {AuthorisedMember.Id} {AuthorisedMember.Name} - ProjectNodeId: {ProjectNodeId}";
        }

        public List<WikiFile> GetProjectFiles()
        {
            // No param of Project ID
            // As this lives in the JWT which is decoded with the Auth Attribute

            var files = WikiFile.CurrentFiles(ProjectNodeId);
            return files.Where(x => x.FileType == "package" || x.FileType == "hotfix").OrderByDescending(x => x.Version.Version).ToList();
        }


        // TODO: Hartvig does his magic for upload
    }
}
