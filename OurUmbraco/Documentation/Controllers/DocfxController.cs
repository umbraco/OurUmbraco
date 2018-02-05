using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using OurUmbraco.Documentation.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.IO.Compression;
using Newtonsoft.Json;
using Umbraco.Core.Logging;

namespace OurUmbraco.Documentation.Controllers
{
    [AppVeyorAuthorizeFilter]
    [PluginController("Documentation")]
    public class DocfxController : UmbracoApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Update(DocFxUpdateModel model)
        {
            var job = model.eventData.jobs.FirstOrDefault();
            if (job == null)
            {
                const string errorMessage = "No job found in payload";
                var input = JsonConvert.SerializeObject(model.eventData);
                LogHelper.Warn<DocFxUpdateModel>($"{errorMessage} - input was: {input}");
                throw new FormatException(errorMessage);
            }

            var artifacts = job.artifacts;
            if (artifacts == null)
            {
                const string errorMessage = "No artifacts found in payload";
                var input = JsonConvert.SerializeObject(job);
                LogHelper.Warn<DocFxUpdateModel>($"{errorMessage} - input was: {input}");
                throw new FormatException(errorMessage);
            }

            LogHelper.Info<DocFxUpdateModel>(string.Format("Found {0} artifacts in the notification", artifacts.Length));

            foreach (var artifact in artifacts)
            {
                LogHelper.Info<DocFxUpdateModel>(string.Format("Processing artifact {0}", artifact.fileName));

                if (artifact.fileName.Contains("-"))
                {
                    // Take the filename which exists after the last "/" in the string
                    var fileName = artifact.fileName.Substring(artifact.fileName.LastIndexOf("/", StringComparison.Ordinal) + 1);

                    var folder = fileName.Split('-')[0];

                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(artifact.url);

                        LogHelper.Info<DocFxUpdateModel>(string.Format("Artifact {0} downloaded", fileName));

                        var docZip = HostingEnvironment.MapPath(string.Format("~/App_Data/Documentation/{0}/{1}", folder, fileName));
                        var docsFolder = HostingEnvironment.MapPath(string.Format("~/App_Data/Documentation/{0}", folder));
                        var hostedDocsFolder = HostingEnvironment.MapPath(string.Format("~/apidocs/{0}", folder));

                        //clear everything
                        if (Directory.Exists(docsFolder))
                            Directory.Delete(docsFolder, true);
                        Directory.CreateDirectory(docsFolder);

                        if (Directory.Exists(hostedDocsFolder))
                            Directory.Delete(hostedDocsFolder, true);
                        Directory.CreateDirectory(hostedDocsFolder);

                        using (var memStream = new MemoryStream(bytes))
                        using (var stream = new FileStream(docZip, FileMode.Create))
                        {
                            await memStream.CopyToAsync(stream);
                            LogHelper.Info<DocFxUpdateModel>(string.Format("Artifact {0} written to {1}", fileName, docZip));
                        }

                        ZipFile.ExtractToDirectory(docZip, hostedDocsFolder);
                        LogHelper.Info<DocFxUpdateModel>(string.Format("Artifact {0} unzipped to {1}", fileName, hostedDocsFolder));
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
