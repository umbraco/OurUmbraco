using OurUmbraco.Documentation.Models;
using System;
using System.IO;
using System.IO.Compression;
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
        static HttpClient client = new HttpClient();

        [HttpGet]
        public string Ping()
        {
            return "Pong";
        }

        [HttpPost]
        public async Task<HttpResponseMessage> ReceiveAzureDevOpsPayload(AzureDevopsPayload model)
        {
            // Fetch the build ID out of the AzureDevops Webhook JSON payload
            var buildId = model.Resource.Id;

            LogHelper.Debug<StaticSiteController>($"Received successful build webhook from Azure Devops for Build ID: {buildId}");

            // Query AzureDevops REST API to give us a list of build artifacts
            var artifactResponse = await FetchBuildArtifacts(buildId);

            // Clear out everything
            var hostedDocsFolder = HostingEnvironment.MapPath("~/apidocs/v8");
            var tempExtractedFolder = HostingEnvironment.MapPath("~/apidocs/v8/_temp");

            if (Directory.Exists(hostedDocsFolder))
                Directory.Delete(hostedDocsFolder, true);
            Directory.CreateDirectory(hostedDocsFolder);


            // We will only have one artifact which is the build.out directory zipped up
            // Which in turn contains the two zips, otherwise AzureDevops was zipping up the zip
            foreach (var artifact in artifactResponse.Artifacts)
            {
                var downloadUrl = artifact?.Resource?.DownloadUrl;
                if(downloadUrl != null)
                {
                    // Download the zip file
                    var fileName = HostingEnvironment.MapPath($"~/apidocs/v8/docs-artifact.zip");
                    var zipResponse = await client.GetAsync(downloadUrl);

                    if (zipResponse.IsSuccessStatusCode == false)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"Unable to download Azure Devops build artifact at {downloadUrl}");

                    // Save the response to a physical file on disk
                    using (var fs = new FileStream(fileName, FileMode.CreateNew))
                    {
                        LogHelper.Debug<StaticSiteController>($"Saving build artifact zip file to: {fileName}");
                        await zipResponse.Content.CopyToAsync(fs);
                    }

                    // Unpack ZIP file (with the two zips in it)
                    LogHelper.Debug<StaticSiteController>($"Extracting zip file '{fileName}' to '{tempExtractedFolder}'");
                    ZipFile.ExtractToDirectory(fileName, tempExtractedFolder);
                }
            }

            // Iterate over files in '~/apidocs/v8/_temp' which should be two ZIPs inside a 'docs' folder hence the AllDir search option
            foreach(var file in Directory.GetFiles(tempExtractedFolder,"*.zip", SearchOption.AllDirectories))
            {
                // "C:\\Code\\OurUmbraco\\OurUmbraco.Site\\apidocs\\v8\\_temp\\docs\\csharp-docs.zip"
                var zipFileName = file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                // csharp-docs.zip
                // csharp
                var folderName = zipFileName.Split('-')[0];
                var extractToFolder = HostingEnvironment.MapPath($"~/apidocs/v8/{folderName}");

                LogHelper.Debug<StaticSiteController>($"Extracting zip file '{zipFileName}' from the TEMP folder to '{extractToFolder}'");
                ZipFile.ExtractToDirectory(file, extractToFolder);
            }

            // Clean unzip tempfolder (now its all unpacked)
            if (Directory.Exists(tempExtractedFolder))
                Directory.Delete(tempExtractedFolder, true);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private async Task<AzureDevopsArtifacts> FetchBuildArtifacts(long buildId)
        {
            // Construct a new request URL to query AzureDevops rest API to list the build artifacts
            var apiUrl = $"https://dev.azure.com/umbraco/Umbraco%20Cms/_apis/build/builds/{buildId}/artifacts?&api-version=5.1";

            LogHelper.Debug<StaticSiteController>($"Fetching JSON response from '{apiUrl}' for build artifact download urls");

            AzureDevopsArtifacts artifacts = null;
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                artifacts = await response.Content.ReadAsAsync<AzureDevopsArtifacts>();
            }
            return artifacts;
        }
    }
}
