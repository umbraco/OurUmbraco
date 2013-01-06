using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Examine;
using Examine.LuceneEngine;
using our.Examine.DocumentationIndexDataService.Model;
using our.Examine.DocumentationIndexDataService.Helper;
using umbraco.BusinessLogic;

namespace our.ExamineServices
{
    public class FileIndexDataService : ISimpleDataService
    {
        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var config = FileIndexerConfig.Settings;
            var fullPath = HttpContext.Current.Server.MapPath(config.DirectoryToIndex);

            var directory = new DirectoryInfo(fullPath);

            var files = config.Recursive ? directory.GetFiles(config.SupportedFileTypes, SearchOption.AllDirectories) : directory.GetFiles(config.SupportedFileTypes);

            var dataSets = new List<SimpleDataSet>();
            var i = 1; //unique id for each doc

            foreach (var file in files)
            {
                try
                {
                    var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                    simpleDataSet = ExamineHelper.MapFileToSimpleDataIndexItem(file, simpleDataSet, i, indexType);

                    dataSets.Add(simpleDataSet);
                }
                catch (Exception ex)
                {
                    Log.Add(LogTypes.Error, i, "error processing file  " + file.FullName + " " + ex);
                }

                i++;
            }

            return dataSets;
        }
    }
}