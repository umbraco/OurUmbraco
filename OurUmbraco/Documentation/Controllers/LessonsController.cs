using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OurUmbraco.Documentation.Busineslogic.GithubSourcePull;
using OurUmbraco.Documentation.Models;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Documentation.Controllers
{
    [PluginController("Documentation")]
    public class LessonsController : UmbracoApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The path in the documentation folder structure that you wish to start from & list descendants</param>
        /// <param name="userType">Future Use: Backoffice Usertype requesting lessons</param>
        /// <param name="allowedSections">Future Use: AllowedSections of BackOffice User</param>
        /// <param name="lang">Future Use: Backoffice Users langunage</param>
        /// <param name="version">Umbraco CMS Version as a string</param>
        /// <returns>
        /// A partial part of the documentation tree, that lists out folders
        /// This is used in the new Starter Kit in conjuction with 'Lessons'
        /// </returns>
        public List<ZipDownloader.SiteMapItem> GetDocsForPath(string path, string userType, string allowedSections, string lang, string version)
        {
            //Ensure path is not null & empty
            if (string.IsNullOrEmpty(path))
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Path varibale is null or empty"),
                    ReasonPhrase = "Documentation Path is invalid"
                };

                throw new HttpResponseException(resp);
            }
            
            //Get the documentation Sitemap JS file that lives on disk everytime we fetch & unpack the markdown docs from GitHub
            var docs = new ZipDownloader();

            //Note: For now the folder param is NOT the folder/subtree but where the JSON file is stored to build up this model
            var allDocs = docs.DocumentationSiteMap();
            
            
            //Split path and ensure we can find each part of the path
            //In the array of nested objects
            var pathArray = path.Split('/').Where(x => string.IsNullOrEmpty(x) == false).ToArray();
            var currentDirectory = allDocs;

            for (int i = 0; i < pathArray.Length; i++)
            {
                var pathPart = "/" + string.Join("/", pathArray.Take(i + 1));
                currentDirectory = currentDirectory.directories.First(x => x.path == pathPart);
            }

            return currentDirectory.directories.OrderBy(x=> x.sort).ToList();
        }
    }
}
