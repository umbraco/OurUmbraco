using System;
using System.Collections.Generic;
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

        public IEnumerable<DocumentationVersion> GetAlternateDocumentationVersions(Uri uri)
        {
            var alternativeDocs = new List<DocumentationVersion>();
            //first off we have the path, do we need to strip the version number from the file name
            string currentFileName = uri.Segments.LastOrDefault();
            //does current filename include version number
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

            string currentUrl = string.Join("",pathParts) + baseFileName;

            //Now we go off to examine, and search for all entries
            //with path beginning with currentFilePath
         
            var searcher = ExamineManager.Instance.SearchProviderCollection["documentationSearcher"];
            var searchCriteria = searcher.CreateSearchCriteria();
          
            //path beginning with current filename
            var query = searchCriteria.Field("__fullUrl", currentUrl.ToLowerInvariant().MultipleCharacterWildcard()).Compile();
            var searchResults = searcher.Search(query);
            if (searchResults.Any())
            {
                alternativeDocs.AddRange(searchResults.Select(f=>new DocumentationVersion(){Url = f["url"],Version = f["versionFrom"]}));
            }

            return alternativeDocs;
        }
    }
}
