﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
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
            var currentFileName = isFolder ? "index" : uri.Segments.LastOrDefault();
            
            if (currentFileName == null) 
                return alternativeDocs;
            
            // does current filename include version number
            // if it ends with a number preceded by "-v", remove the version suffix
            // for example -v7, -v8, -v9, or -v10
            const string pattern = @"-v([0-9]{0,})$";
            var match = Regex.Match(currentFileName, pattern, RegexOptions.IgnoreCase);
            var isCurrentDocumentationPage = match.Success == false;
            
            var pathParts = new List<string>();
            var maxSegments = uri.Segments.Length;
            if (!isCurrentDocumentationPage)
            {
                maxSegments -= 1;
            }

            for (var i = 0; i < maxSegments; i++)
            {
                pathParts.Add(uri.Segments[i]);
            }

            var baseFileName = string.Empty;
            
            if (isCurrentDocumentationPage)
            {
                var positionToStripUpTo = currentFileName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
                if (positionToStripUpTo > -1)
                {
                    baseFileName = currentFileName.Substring(0, positionToStripUpTo);
                }
                else if (isFolder)
                {
                    baseFileName = "index";
                }
            }
            else
            {
                baseFileName = match.Success 
                    ? Regex.Replace(currentFileName, pattern, "") 
                    : currentFileName;
            }

            var joinedPathParts = string.Join("", pathParts);  
            var currentUrl = joinedPathParts.EndsWith(baseFileName) ? joinedPathParts : joinedPathParts + baseFileName;
            var currentPageUrl = joinedPathParts.EndsWith(currentFileName) ? joinedPathParts : joinedPathParts + currentFileName;

            //Now we go off to examine, and search for all entries
            //with path beginning with currentFilePath

            var searcher = ExamineManager.Instance.SearchProviderCollection["documentationSearcher"];
            var searchCriteria = searcher.CreateSearchCriteria();

            //path beginning with current filename
            var query = searchCriteria.Field("__fullUrl", currentUrl.ToLowerInvariant().MultipleCharacterWildcard()).Compile();
            var searchResults = searcher.Search(query);
            
            if (searchResults.TotalItemCount <= 1 && !allVersions) 
                return alternativeDocs;
            
            var versionInfo = searchResults.Select(result =>
                {
                    var version = new DocumentationVersion();
                    version.Url = result["url"];
                    var versionFrom = result["versionFrom"];
                    var versionTo = result["versionTo"];
                    Semver.SemVersion.TryParse(versionFrom, out var versionFromSemver);
                    Semver.SemVersion.TryParse(versionTo, out var versionToSemver);
                    
                    version.Version = CalculateVersionInfo(versionFrom, versionTo);
                    version.VersionFrom = string.IsNullOrWhiteSpace(versionFrom) ?  new Semver.SemVersion(0) : versionFromSemver;
                    version.VersionTo = string.IsNullOrWhiteSpace(versionTo) ? new Semver.SemVersion(0) : versionToSemver;
                    version.VersionRemoved = result["versionRemoved"];
                    version.IsCurrentVersion = string.Equals(result["url"], currentUrl, StringComparison.InvariantCultureIgnoreCase);
                    version.IsCurrentPage = string.Equals(result["url"], currentPageUrl, StringComparison.InvariantCultureIgnoreCase);
                    version.MetaDescription = result["meta.Description"];
                    version.MetaTitle = result["meta.Title"];
                    version.NeedsV8Update = result["needsV8Update"];
                    version.RedirectLink = result["meta.RedirectLink"];
                    
                    return version;
                })
                .OrderByDescending(v=> v.VersionFrom)
                .ThenBy(v=>v.VersionTo);

            alternativeDocs.AddRange(versionInfo);

            return alternativeDocs;
        }

        private string CalculateVersionInfo(string from, string to)
        {
            if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
            {
                return "current";
            }

            if (string.IsNullOrWhiteSpace(from))
            {
                return "pre " + to;
            }

            if (string.IsNullOrWhiteSpace(to))
            {
                return from + " +";
            }

            if (to == from)
            {
                return from;
            }

            return from + " - " + to;
        }
    }
}
