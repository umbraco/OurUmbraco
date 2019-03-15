using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Examine;
using Examine.LuceneEngine.SearchCriteria;
using OurUmbraco.Repository.Models;

namespace OurUmbraco.Repository.Services
{
    public class DocumentationVersionService
    {

        public string GetCurrentMajorVersion()
        {
            return ConfigurationManager.AppSettings[Constants.AppSettings.DocumentationCurrentMajorVersion];
        }

        public IEnumerable<DocumentationVersion> GetAlternateDocumentationVersions(Uri uri, bool allVersions = false)
        {
            var alternativeDocs = new List<DocumentationVersion>();
            // first off we have the path, do we need to strip the version number from the file name
            var isFolder = uri.ToString().EndsWith("/");
            string currentFileName = isFolder ? "index" : uri.Segments.LastOrDefault();
            // does current filename include version number
            bool isCurrentDocumentationPage = !currentFileName.Contains("-v");//better way?
            List<string> pathParts = new List<string>();

            var maxSegments = uri.Segments.Length;
            if (!isCurrentDocumentationPage)
            {
                maxSegments -= 1;
            }

            for (int i = 0; i < maxSegments; i++)
            {
                pathParts.Add(uri.Segments[i]);
            }

            string baseFileName = String.Empty;
            int positionToStripUpTo = isCurrentDocumentationPage ? currentFileName.LastIndexOf(".") : currentFileName.LastIndexOf("-v");
            if (positionToStripUpTo > -1)
            {
                baseFileName = currentFileName.Substring(0, positionToStripUpTo);
            }
            else if (isFolder)
            {
                baseFileName = "index";
            }

            string currentUrl = string.Join("", pathParts) + baseFileName;
            string currentPageUrl = (string.Join("", pathParts) + currentFileName).ToLowerInvariant();
            

            //Now we go off to examine, and search for all entries
            //with path beginning with currentFilePath

            var searcher = ExamineManager.Instance.SearchProviderCollection["documentationSearcher"];
            var searchCriteria = searcher.CreateSearchCriteria();

            //path beginning with current filename
            var query = searchCriteria.Field("__fullUrl", currentUrl.ToLowerInvariant().MultipleCharacterWildcard()).Compile();
            var searchResults = searcher.Search(query);
            if (searchResults.TotalItemCount > 1 || allVersions)
            {
                var versionInfo = searchResults.Select(f =>
                    new DocumentationVersion()
                    {
                        Url = f["url"],
                        Version = CalculateVersionInfo(f["versionFrom"], f["versionTo"]),
                        VersionFrom = string.IsNullOrWhiteSpace( f["versionFrom"] ) ?  new Semver.SemVersion(0) : Semver.SemVersion.Parse(f["versionFrom"]),
                        VersionTo = string.IsNullOrWhiteSpace(f["versionTo"]) ? new Semver.SemVersion(0) : Semver.SemVersion.Parse(f["versionTo"]),
                        VersionRemoved = f["versionRemoved"],
                        IsCurrentVersion = f["url"].ToLowerInvariant() == currentUrl.ToLowerInvariant(),
                        IsCurrentPage = f["url"].ToLowerInvariant() == currentPageUrl,
                        MetaDescription = f["meta.Description"],
                        MetaTitle = f["meta.Title"],
                        NeedsV8Update = f["needsV8Update"]
                    })
                    .OrderByDescending(v=> v.VersionFrom)
                    .ThenBy(v=>v.VersionTo);

                alternativeDocs.AddRange(versionInfo);
            }

            return alternativeDocs;
        }

        private string CalculateVersionInfo(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
            {
                return "current";
            }
            else if (string.IsNullOrWhiteSpace(from))
            {
                return "pre " + to;
            }
            else if (string.IsNullOrWhiteSpace(to))
            {
                return from + " +";
            }
            else if (to == from)
            {
                return from;
            }
            else
            {
                return from + " - " + to;
            }
        }
    }
}
