using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Examine.LuceneEngine;
using our.Examine.DocumentationIndexDataService.Model;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Examine.Providers;
using Examine;
using umbraco.BusinessLogic;
namespace our.Examine.DocumentationIndexDataService.Helper
{
    public static class ExamineHelper
    {
        public const string DocumentationIndexer = "documentationIndexer";

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
        public static Dictionary<string, string> GetDataSet(string filePath, int docIndex) {
            var sd = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
            FileInfo fi1 = new FileInfo(filePath);
            sd = MapFileToSimpleDataIndexItem(fi1, sd, docIndex, "documentation");
            return sd.RowData;
        }

        public static SimpleDataSet MapFileToSimpleDataIndexItem(FileInfo file, SimpleDataSet sd, int i, string indexType)
        {
         
            var lines = new List<string>();
            lines.AddRange(System.IO.File.ReadAllLines(file.FullName));
            string headLine = string.Empty;
            string body = string.Empty;
            if (lines.Count > 0) {
                headLine = RemoveSpecialCharacters(lines[0]);
                lines.RemoveAt(0);
                body = RemoveSpecialCharacters(string.Join("", lines));
            }
            
            sd.NodeDefinition.NodeId = i;
            sd.NodeDefinition.Type = indexType;
            sd.RowData.Add("Body", body);
            sd.RowData.Add("Title", headLine);
            sd.RowData.Add("dateCreated", file.CreationTime.ToString("yyyy-MM-dd-HH:mm:ss"));
            sd.RowData.Add("dateCreatedSearchAble", file.CreationTime.SerializeForLucene());
            sd.RowData.Add("Path", file.FullName);
            sd.RowData.Add("searchAblePath", file.FullName.Replace("\\"," ").Replace(":",""));
            
            sd.RowData.Add("nodeTypeAlias", "document");
            sd.RowData.Add("url", BuildUrl(file.FullName));
            return sd;
        }

        private static string BuildUrl(string path)
        {
            int startId = path.IndexOf("Documentation");
            string partPath = path.Substring(startId, path.Length - startId);
            string url = "/" + partPath.Replace("\\", "/").Replace(".md", "");
            return url;
        }

        public static string RemoveSpecialCharacters(string input)
        {
            Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r.Replace(input, String.Empty);
        }


    }
}
