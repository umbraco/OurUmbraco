using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Examine;
using Examine.LuceneEngine;
using Umbraco.Core.Logging;

namespace our.Examine
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

        public static string SerializeForLucene(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        public static DateTime DeserializeFromLucene(this string str)
        {
            return DateTime.ParseExact(str, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
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
            var headLine = string.Empty;
            var body = string.Empty;
            if (lines.Count > 0)
            {
                headLine = RemoveSpecialCharacters(lines[0]);
                lines.RemoveAt(0);
                body = umbraco.library.StripHtml( RemoveSpecialCharacters(string.Join("", lines)) );
            }

            simpleDataSet.NodeDefinition.NodeId = index;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", body);
            simpleDataSet.RowData.Add("nodeName", headLine);
            simpleDataSet.RowData.Add("updateDate", file.CreationTime.SerializeForLucene());
            simpleDataSet.RowData.Add("nodeTypeAlias", "documentation");

            simpleDataSet.RowData.Add("dateCreated", file.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));

            //TODO: This will always be exactly the same since all files are written at the same time IIRC
            simpleDataSet.RowData.Add("Path", file.FullName);
            simpleDataSet.RowData.Add("searchAblePath", file.FullName.Replace("\\", " ").Replace(":", ""));
            simpleDataSet.RowData.Add("url", BuildUrl(file.FullName));
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
