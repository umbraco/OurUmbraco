using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Hangfire.Console;
using Hangfire.Server;
using Newtonsoft.Json;
using OurUmbraco.NuGet.Models;

namespace OurUmbraco.NuGet.Services
{
    public class SearchService
    {
        public List<SearchResult> GetPackages(PerformContext performContext)
        {
            var searchResults = new List<SearchResult>();
            var resultsRetrieved = 0;
            int totalResults;
            
            const int take = 10;
            var skip = 0;
            var tags = new List<string>
            {
                "umbraco",
                "package",
                "plugin"
            };

            do
            {
                var page = GetSearchResults(tags, skip, take);
                resultsRetrieved += page.SearchResults.Count;
                totalResults = page.TotalHits;
                skip += take;
                searchResults.AddRange(page.SearchResults);
            } while (resultsRetrieved < totalResults);

            foreach (var result in searchResults)
            {
                // Latest version assumes we get an ordered list of versions
                var latestVersion = result.Versions.Last();
                var details = GetVersionDetails(latestVersion.Id);
                performContext.WriteLine($"{result.Id} [{latestVersion.Version}] published {details.Published.ToString("yyyy-MM-dd hh:mm:ss")}");                
            }

            return searchResults;
        }

        private ApiResponse GetSearchResults(IEnumerable<string> tags, int skip, int take)
        {
            var webClient = new WebClient();
            var json = webClient.DownloadString($"https://api-v2v3search-0.nuget.org/query?skip={skip}&take={take}&prerelease=true&q=Tags:{string.Join("+", tags)}&semVerLevel=2.0.0");
            var searchResults = JsonConvert.DeserializeObject<ApiResponse>(json);
            return searchResults;
        }

        private VersionDetails GetVersionDetails(string versionEndPointUrl)
        {
            var webClient = new WebClient();
            var gzip = webClient.DownloadData(versionEndPointUrl);
            var json = Unzip(gzip);
            var searchResults = JsonConvert.DeserializeObject<VersionDetails>(json);
            return searchResults;
        }
        
        // From: https://stackoverflow.com/a/7343623/5018
        private static string Unzip(byte[] bytes) 
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) 
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress)) 
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        
        private static void CopyTo(Stream src, Stream dest) 
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) 
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}