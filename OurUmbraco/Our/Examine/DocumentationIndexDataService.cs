using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;

namespace OurUmbraco.Our.Examine
{
    /// <summary>
    /// Used to index the documentation 
    /// </summary>
    public class DocumentationIndexDataService : ISimpleDataService
    {

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var config = DocumentationIndexConfig.Settings;
            var fullPath = HostingEnvironment.MapPath(config.DirectoryToIndex);

            var directory = new DirectoryInfo(fullPath);

            var files = config.Recursive ? directory.GetFiles(config.SupportedFileTypes, SearchOption.AllDirectories) : directory.GetFiles(config.SupportedFileTypes);

            var i = 0; //unique id for each doc

            foreach (var file in files)
            {
                i++;
                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                simpleDataSet = ExamineHelper.MapFileToSimpleDataIndexItem(file, simpleDataSet, i, indexType);
                yield return simpleDataSet;
            }
        }
    }
}