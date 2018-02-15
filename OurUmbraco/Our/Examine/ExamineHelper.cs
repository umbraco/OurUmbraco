using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace OurUmbraco.Our.Examine
{
    public static class ExamineHelper
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

        public static SimpleDataSet MapFileToSimpleDataIndexItem(FileInfo file, SimpleDataSet simpleDataSet, int index, string indexType)
        {
            var lines = new List<string>();
            lines.AddRange(File.ReadAllLines(file.FullName));

            var body = lines.Any()
                ? RemoveSpecialCharacters(string.Join("", lines)).StripHtml()
                : string.Empty;

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
            if(file.Name == "broken-link.md")
            {
                simpleDataSet.RowData.Clear();
            }
            return simpleDataSet;
        }

        private static string BuildUrl(string path)
        {
            var startId = path.IndexOf("Documentation", StringComparison.Ordinal);
            var partPath = path.Substring(startId, path.Length - startId);
            var url = "/" + partPath.Replace("\\", "/").Replace(".md", "");
            return url;
        }

        public static string RemoveSpecialCharacters(string input)
        {
            var regex = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return regex.Replace(input, String.Empty);
        }
    }
}
