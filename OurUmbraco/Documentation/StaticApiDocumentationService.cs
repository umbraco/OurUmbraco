using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using Hangfire.Console;
using Hangfire.Server;
using OurUmbraco.Documentation.Controllers;
using OurUmbraco.Documentation.Models;
using Umbraco.Core.Logging;

namespace OurUmbraco.Documentation
{
    public class StaticApiDocumentationService
    {
        private static readonly HttpClient Client = new HttpClient();
        
        public async Task FetchNewApiDocs(PerformContext context)
        {
            var buildId = string.Empty;
            var apiDocsMarker = HostingEnvironment.MapPath("~/App_Data/TEMP/ApiDocsMarker.txt");
            if (File.Exists(apiDocsMarker) == false)
            {
                // we're done, nothing to process
                context.WriteLine("No new API Docs to process.");
                return;
            }
            
            buildId = File.ReadAllText(apiDocsMarker);

            context.WriteLine($"Processing API docs for build Id {buildId}");
                
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
                    var fileName = HostingEnvironment.MapPath("~/apidocs/v8/docs-artifact.zip");
                    var zipResponse = await Client.GetAsync(downloadUrl);

                    if (zipResponse.IsSuccessStatusCode == false)
                        context.WriteLine($"Unable to download Azure Devops build artifact at {downloadUrl}");

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

            File.Delete(apiDocsMarker);
        }
        
        private async Task<AzureDevopsArtifacts> FetchBuildArtifacts(string buildId)
        {
            // Construct a new request URL to query AzureDevops rest API to list the build artifacts
            var apiUrl = $"https://dev.azure.com/umbraco/Umbraco%20Cms/_apis/build/builds/{buildId}/artifacts?&api-version=5.1";

            LogHelper.Debug<StaticSiteController>($"Fetching JSON response from '{apiUrl}' for build artifact download urls");

            AzureDevopsArtifacts artifacts = null;
            var response = await Client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                artifacts = await response.Content.ReadAsAsync<AzureDevopsArtifacts>();
            }
            return artifacts;
        }
    }
}