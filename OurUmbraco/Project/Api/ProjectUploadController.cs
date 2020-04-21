using Newtonsoft.Json;
using OurUmbraco.Wiki.BusinessLogic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OurUmbraco.Project.Api
{
    public class ProjectUploadController : UmbracoMemberAuthApiController
    {
        public IEnumerable<WikiFile> GetProjectFiles()
        {
            // The project/member id are exposed as base properties and are resolved from the identity created on the request
            var files = WikiFile.CurrentFiles(ProjectNodeId);
            return files.Where(x => x.FileType == "package" || x.FileType == "hotfix").OrderByDescending(x => x.Version.Version).ToList();
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UpdatePackage()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                // verify that a file exist (and only one)
                if (provider.FileData.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "No file attached");
                }
                else if (provider.FileData.Count > 1)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "You can only upload one file");
                }
                else
                {
                    var packageFile = provider.FileData.FirstOrDefault();
                    var fileName = packageFile.Headers.ContentDisposition.FileName.Replace("\"", "");
                    var packageFileExtension = Path.GetExtension(fileName);
                    var fileType = provider.FormData["fileType"];
                    var dotNetVersion = provider.FormData["dotNetVersion"];
                    var umbracoVersions = JsonConvert.DeserializeObject<List<UmbracoVersion>>(provider.FormData["umbracoVersions"]);
                    var isCurrent = bool.Parse(provider.FormData["isCurrent"]);

                    var contentService = ApplicationContext.Services.ContentService;

                    // get package guid from id
                    var packageEntity = contentService.GetById(ProjectNodeId);
                    if (packageEntity != null)
                    {
                        var packageVersion = packageEntity.Version;

                        // create file
                        var file = WikiFile.Create(
                            fileName,
                            packageFileExtension,
                            packageVersion,
                            AuthenticatedMember.Key,
                            System.IO.File.ReadAllBytes(packageFile.LocalFileName),
                            fileType,
                            umbracoVersions,
                            dotNetVersion
                        );

                        file.Current = isCurrent;
                        
                        if (isCurrent)
                            packageEntity.SetValue("file", file.Id);

                        packageEntity.SetValue("dotNetVersion", dotNetVersion);

                       

                        contentService.SaveAndPublishWithStatus(packageEntity);

                        return Request.CreateResponse(HttpStatusCode.OK, "Package file updated");
                    }
                }
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }

            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }
    }
}
