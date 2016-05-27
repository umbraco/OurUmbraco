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
using System.Web.Http.Filters;

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
            if (job == null) throw new FormatException("No job found in payload");

            var artifacts = job.artifacts;
            if (artifacts == null) throw new FormatException("No artifacts found in payload");

            foreach (var artifact in artifacts)
            {
                if (artifact.fileName.Contains("-"))
                {
                    var folder = artifact.fileName.Split('-')[0];

                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(artifact.url);

                        var docZip = HostingEnvironment.MapPath(string.Format("~/App_Data/Documentation/{0}/{1}", folder, artifact.fileName));
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
                        }

                        ZipFile.ExtractToDirectory(docZip, hostedDocsFolder);
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
