using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using System.Xml.Linq;
using Examine.LuceneEngine.Providers;
using Examine;
using System.Text.RegularExpressions;

using uDocumentation.Busineslogic;

namespace our
{
    public class DocumentationIndexer : ApplicationBase
    {
       
        public DocumentationIndexer()
        {
            uDocumentation.Busineslogic.GithubSourcePull.ZipDownloader.OnCreate += new EventHandler<CreateEventArgs>(ZipDownloader_OnCreate);
            uDocumentation.Busineslogic.GithubSourcePull.ZipDownloader.OnUpdate += new EventHandler<UpdateEventArgs>(ZipDownloader_OnUpdate);
            uDocumentation.Busineslogic.GithubSourcePull.ZipDownloader.OnDelete += new EventHandler<DeleteEventArgs>(ZipDownloader_OnDelete);
        }

        void ZipDownloader_OnDelete(object sender, DeleteEventArgs e)
        {
            Log.Add(LogTypes.Debug, -1, "Deleting " + e.FilePath);


            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["DocumentationIndexer"];
            indexer.DeleteFromIndex(GetKey(e.FilePath));
        }
        
        void ZipDownloader_OnUpdate(object sender, UpdateEventArgs e)
        {
            Log.Add(LogTypes.Debug, -1, "Updating " + e.FilePath);


            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["DocumentationIndexer"];
            var data = createDataSet(e.FilePath);
            var key = GetKey(e.FilePath);

            var xml = ToExamineXml(data, key, "Documentation");
            indexer.ReIndexNode(xml, "documents");
        }

        void ZipDownloader_OnCreate(object sender, CreateEventArgs e)
        {
            Log.Add(LogTypes.Debug, -1, "Creating " + e.FilePath);

            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection["DocumentationIndexer"];
            var data = createDataSet(e.FilePath);
            var key = GetKey(e.FilePath);
            
            var xml = ToExamineXml(data, key, "Documentation");
            indexer.ReIndexNode(xml, "documents");
        }

        public static XElement ToExamineXml(Dictionary<string, string> data, string key, string nodeType)
        {
            return new XElement("node",
                //creates the element attributes
                new XAttribute("id", key),
                new XAttribute("nodeTypeAlias", nodeType),
                //creates the data nodes
                    data.Select(x => new XElement("data",
                        new XAttribute("alias", x.Key),
                        new XCData(x.Value))).ToList());

        }

        private static string rootDir = "\\Documenation\\";
        private static string TrimPath(string fullpath)
        {
            var defaultVersionPath = rootDir + uDocumentation.Busineslogic.DefaultVersion.Instance.Number + "\\";
            var index = fullpath.IndexOf(defaultVersionPath) + defaultVersionPath.Length;
            return fullpath.Substring(index);            
        }

        private Dictionary<string, string> createDataSet(string fulPath)
        {
            var lines = new List<string>(); 
            lines.AddRange(System.IO.File.ReadAllLines(fulPath));

            var headLine = RemoveSpecialCharacters(lines[0]);
            lines.RemoveAt(0);

            var body = RemoveSpecialCharacters(string.Join("", lines));
            var path = TrimPath(fulPath);

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Body", body);
            data.Add("Title", headLine);
            data.Add("Path", path);

            return data;
        }

        public static string GetKey(string fullPath)
        {
            var key = fullPath;
            var defaultVersionPath = rootDir + uDocumentation.Busineslogic.DefaultVersion.Instance.Number + "\\";
            if(fullPath.Contains(defaultVersionPath))
                key = TrimPath(key);

            return RemoveSpecialCharacters(key);

        }

        public static string RemoveSpecialCharacters(string input)
        {
            Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return r.Replace(input, String.Empty);
        }
    }
}