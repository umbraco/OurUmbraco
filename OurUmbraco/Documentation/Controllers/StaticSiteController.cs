using OurUmbraco.Documentation.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Documentation.Controllers
{
    [IncomingWebHookAuthorizeFilter]
    [PluginController("Documentation")]
    public class StaticSiteController : UmbracoApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> ReceiveAzureDevOpsPayload(AzureDevopsPayload model)
        {
            // Fetch the build ID out of the AzureDevops Webhook JSON payload
            var buildId = model.Resource.Id;

            LogHelper.Debug<StaticSiteController>($"Received successful build webhook from Azure Devops for Build ID: {buildId}");

            var apiDocsMarker = HostingEnvironment.MapPath("~/App_Data/TEMP/ApiDocsMarker.txt");
            File.WriteAllText(apiDocsMarker, model.Resource.Id.ToString());

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
