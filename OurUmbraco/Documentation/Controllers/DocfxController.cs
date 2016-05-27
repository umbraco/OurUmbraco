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

namespace OurUmbraco.Documentation.Controllers
{
    [PluginController("Documentation")]
    public class DocfxController : UmbracoApiController
    {

        [HttpPost]
        public async Task<HttpResponseMessage> Update(DocFxUpdateModel model)
        {
            var job = model.eventData.jobs.FirstOrDefault();
            if (job == null) throw new FormatException("No job found in payload");

            var artifact = job.artifacts.FirstOrDefault();
            if (artifact == null) throw new FormatException("No artifact found in payload");

            using (var client = new HttpClient())
            {
                var bytes = await client.GetByteArrayAsync(artifact.url);

                var docZip = HostingEnvironment.MapPath("~/App_Data/Documentation/csharp/docs.zip");
                var docsFolder = HostingEnvironment.MapPath("~/App_Data/Documentation/csharp");
                var hostedDocsFolder = HostingEnvironment.MapPath("~/apidocs/csharp");

                //clear everything
                if (Directory.Exists(docsFolder))
                    Directory.Delete(docsFolder);
                Directory.CreateDirectory(docsFolder);

                if (Directory.Exists(hostedDocsFolder))
                    Directory.Delete(hostedDocsFolder);
                Directory.CreateDirectory(hostedDocsFolder);

                using (var memStream = new MemoryStream(bytes))
                using (var stream = new FileStream(docZip, FileMode.Create))
                {
                    await memStream.CopyToAsync(stream);
                }

                ZipFile.ExtractToDirectory(docZip, hostedDocsFolder);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
