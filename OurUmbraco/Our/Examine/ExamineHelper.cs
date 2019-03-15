using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine;
using Umbraco.Core;
using Umbraco.Core.Logging;
using System.Text;
using YamlDotNet.Serialization;
using OurUmbraco.Documentation.Busineslogic;
using System.Configuration;

namespace OurUmbraco.Our.Examine
{
    public static partial class ExamineHelper
    {
        public const string DocumentationIndexer = "documentationIndexer";

        /// <summary>
        /// Event handler to log errors for any non-umbraco indexer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void LogErrors(object sender, IndexingErrorEventArgs e)
        {
            LogHelper.Error(sender.GetType(),
                "Indexing error occurred",
                new Exception(e.Message, e.InnerException));
        }

        /// <summary>
        /// get dataset for single item reindex via event
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="docIndex"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDataSet(string filePath, int docIndex)
        {
            var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
            var fileInfo = new FileInfo(filePath);
            simpleDataSet = MapFileToSimpleDataIndexItem(fileInfo, simpleDataSet, docIndex, "documentation");
            return simpleDataSet.RowData;
        }

        internal static SimpleDataSet MapFileToSimpleDataIndexItem(FileInfo file, SimpleDataSet simpleDataSet, int index, string indexType)
        {
            var lines = new List<string>();
            lines.AddRange(File.ReadAllLines(file.FullName));

            // don't parse empty files
            if (lines.Count == 0)
            {
                Umbraco.Core.Logging.LogHelper.Info(typeof(ExamineHelper), "Skipping file {0}", () => file.FullName);
                return simpleDataSet;
            }

            // remove first empty lines
            while (string.IsNullOrWhiteSpace(lines.ElementAt(0)))
            {
                lines.RemoveAt(0);
            }

            int secondYamlMarker = AddYamlFields(simpleDataSet, lines);

            // build body 
            var bodyLines = lines.GetRange(secondYamlMarker + 1, lines.Count - secondYamlMarker - 1);
            var body = bodyLines.Any()
                      ? RemoveSpecialCharacters(string.Join(" ", bodyLines)).StripHtml()
                      : string.Empty;

            // extract the first headline
            var firstHeadline = lines.FirstOrDefault(x => x.StartsWith("#"));
            var headLine = firstHeadline ?? file.FullName.Substring(file.FullName.LastIndexOf("\\", StringComparison.Ordinal) + 1).Replace(".md", string.Empty).Replace("-", " ");

            simpleDataSet.NodeDefinition.NodeId = index;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", body);
            simpleDataSet.RowData.Add("nodeName", RemoveSpecialCharacters(headLine));
            simpleDataSet.RowData.Add("updateDate", file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("nodeTypeAlias", "documentation");

            simpleDataSet.RowData.Add("dateCreated", file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //TODO: This will always be exactly the same since results all files are written at the same time IIRC
            simpleDataSet.RowData.Add("Path", file.FullName);
            simpleDataSet.RowData.Add("searchAblePath", file.FullName.Replace("\\", " ").Replace(":", ""));
            simpleDataSet.RowData.Add("url", BuildUrl(file.FullName));

            // Used to exclude broken link page from the search results.
            if (file.Name == "broken-link.md")
            {
                simpleDataSet.RowData.Clear();
            }
            return simpleDataSet;
        }

        /// <summary>
        /// Add all YAML fields to the index
        /// </summary>
        /// <param name="simpleDataSet"></param>
        /// <param name="lines"></param>
        /// <returns>The linenumber of the second YAML marker</returns>
        private static int AddYamlFields(SimpleDataSet simpleDataSet, List<string> lines)
        {
            // Check if the first line is a YAML marker
            // YAML is only accepted if it's on the top of the document
            // because empty lines are already removed, the index needs to be 0
            bool hasYaml = lines.ElementAt(0).TrimEnd() == "---";
            int secondYamlMarker = 0;

            if (hasYaml)
            {
                // Find the "next" triple dash starting from the second line
                // But first trim all trailing spaces as this only creates issues which are hard to debug
                // and unclear for users. Make sure you have a ToList because IEnumerable has no IndexOf()
                secondYamlMarker = lines
                    .Select(l=>l.TrimEnd())
                    .ToList()
                    .IndexOf("---", 1);

                // add all yaml together and parse YAML meta data
                YamlMetaData yamlMetaData = new YamlMetaData();
                if (secondYamlMarker > 0)
                {
                    // we found a second marker, so we have YAML data available
                    var yamlInput = new StringBuilder();
                    for (int i = 1; i < secondYamlMarker; i++)
                    {
                        yamlInput.AppendLine(lines.ElementAt(i));
                    };

                    // Try to convert the YAML text to a strongly typed model using YamlDotNet
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention())
                        .IgnoreUnmatchedProperties()
                        .Build();
                    yamlMetaData = deserializer.Deserialize<YamlMetaData>(yamlInput.ToString());
                }

                // Add Yaml stuff to the LUCENE index
                simpleDataSet.RowData.Add("tags", yamlMetaData.Tags);
                simpleDataSet.RowData.Add("keywords", yamlMetaData.Keywords);
                simpleDataSet.RowData.Add("versionFrom", yamlMetaData.VersionFrom);
                simpleDataSet.RowData.Add("versionTo", yamlMetaData.VersionTo);
                simpleDataSet.RowData.Add("assetID", yamlMetaData.AssetId);
                simpleDataSet.RowData.Add("product", yamlMetaData.Product);
                simpleDataSet.RowData.Add("topics", yamlMetaData.Topics);
                simpleDataSet.RowData.Add("audience", yamlMetaData.Topics);
                simpleDataSet.RowData.Add("complexity", yamlMetaData.Complexity);
                simpleDataSet.RowData.Add("meta.Title", yamlMetaData.MetaTitle);
                simpleDataSet.RowData.Add("meta.Description", yamlMetaData.MetaDescription);
                simpleDataSet.RowData.Add("versionRemoved", yamlMetaData.VersionRemoved);
                simpleDataSet.RowData.Add("needsV8Update", yamlMetaData.NeedsV8Update);

                var matchingMajorVersions = CalculateMajorVersions(yamlMetaData);
                simpleDataSet.RowData.Add("majorVersion", string.Join(" ", matchingMajorVersions));
            }
            else
            {
                // no YAML information, add the current version as majorVersion
                simpleDataSet.RowData.Add("majorVersion", GetCurrentDocVersion().ToString());
            }
            return secondYamlMarker;
        }

        private static IEnumerable<int> CalculateMajorVersions(YamlMetaData yamlMetaData)
        {
            // Try to find out which major versions are supported
            var currentDocVersion = GetCurrentDocVersion();
            var semverCurrent = new Semver.SemVersion(currentDocVersion);
            bool hasFrom = Semver.SemVersion.TryParse(yamlMetaData.VersionFrom, out Semver.SemVersion semverFrom);
            bool hasTo = Semver.SemVersion.TryParse(yamlMetaData.VersionTo, out Semver.SemVersion semverTo);
            bool hasRemoved = Semver.SemVersion.TryParse(yamlMetaData.VersionRemoved, out Semver.SemVersion semverRemoved);


            if (hasFrom == false) { semverFrom = new Semver.SemVersion(currentDocVersion); }
            if (hasTo == false) { semverTo = semverFrom < semverCurrent ? semverCurrent : semverFrom; }
            if (semverFrom > semverTo) { semverFrom = semverTo; }

            var matchingMajorVersions = new List<int>();
            for (int i = semverFrom.Major; i <= semverTo.Major; i++)
            {
                matchingMajorVersions.Add(i);
            }

            if (hasRemoved)
            {
                matchingMajorVersions.RemoveAll(x => x == semverRemoved.Major);
            }

            return matchingMajorVersions;
        }

        private static int GetCurrentDocVersion()
        {
            return int.Parse(ConfigurationManager.AppSettings[Constants.AppSettings.DocumentationCurrentMajorVersion]);
        }

        private static string BuildUrl(string path)
        {
            var startId = path.IndexOf("Documentation", StringComparison.Ordinal);
            var partPath = path.Substring(startId, path.Length - startId);
            var url = "/" + partPath.Replace("\\", "/").Replace(".md", "").DotToUnderscore();
            return url;
        }

        public static string RemoveSpecialCharacters(string input)
        {
            var regex = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return regex.Replace(input, String.Empty);
        }
    }
}
