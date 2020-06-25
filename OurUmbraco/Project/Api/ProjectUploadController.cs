using System;
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
        /// <summary>
        /// Used by the package CLI tool, gets a list of project files to check if one already exists with the same name
        /// </summary>
        public IEnumerable<WikiFile> GetProjectFiles()
        {
            // The project/member id are exposed as base properties and are resolved from the identity created on the request
            var files = WikiFile.CurrentFiles(ProjectNodeId);
            return files.Where(x => x.FileType == "package" || x.FileType == "hotfix").OrderByDescending(x => x.Version.Version).ToList();
        }

        /// <summary>
        /// Posts a package file + some related meta data to update a package from the CLI package tool.
        /// Includes the zip, whether it should be current, the compatible .NET and Umbraco versions.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        public async Task<HttpResponseMessage> UpdatePackage()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data/");
            var workingDir = CreateWorkingFolder(root, "TempPkgFiles");
            var provider = new MultipartFormDataStreamProvider(workingDir);
            MultipartFileData packageFile = null;

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
                    packageFile = provider.FileData.FirstOrDefault();
                    var fileName = packageFile.Headers.ContentDisposition.FileName.Replace("\"", "");
                    var packageFileExtension = Path.GetExtension(fileName);
                    var fileType = provider.FormData["fileType"];
                    var dotNetVersion = provider.FormData["dotNetVersion"];
                    var umbracoVersions = JsonConvert.DeserializeObject<List<UmbracoVersion>>(provider.FormData["umbracoVersions"]);
                    var isCurrent = bool.Parse(provider.FormData["isCurrent"]);
                    var packageVersionNumber = provider.FormData["packageVersion"];

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
                        {
                            packageEntity.SetValue("file", file.Id);
                            
                            if(!string.IsNullOrEmpty(packageVersionNumber)) // nessecary check for older versions of umbpack
                                packageEntity.SetValue("version", packageVersionNumber);
                        }

                        packageEntity.SetValue("dotNetVersion", dotNetVersion);
                        contentService.SaveAndPublishWithStatus(packageEntity);

                        DeleteTempFile(packageFile);

                        return Request.CreateResponse(HttpStatusCode.OK, "Package file updated");
                    }
                }
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
            finally
            {
                // if there's any errors, make sure to delete the temp file 
                DeleteTempFile(packageFile);
            }
            
            throw new HttpResponseException(HttpStatusCode.BadRequest);

        }

        private static void DeleteTempFile(MultipartFileData packageFile)
        {
            try
            {
                if (packageFile != null)
                {
                    File.Delete(packageFile.LocalFileName);
                }
            }
            catch (Exception e)
            {
                // if we can't delete the file then that's alright
                // we'll monitor and see if we need to clean up at some point
            }
        }

        private static string CreateWorkingFolder(string path, string subFolder = "", bool clean = true) 
        {
            var folder = Path.Combine(path, subFolder);
            
            if (clean && Directory.Exists(folder))
                Directory.Delete(folder, true);

            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
