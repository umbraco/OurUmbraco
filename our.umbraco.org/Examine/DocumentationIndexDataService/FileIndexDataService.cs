using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using Examine;
using Examine.LuceneEngine;
using our.Examine.DocumentationIndexDataService.Model;
using our.Examine.DocumentationIndexDataService.Helper;
using umbraco.BusinessLogic;
namespace our.ExamineServices
{
    public class FileIndexDataService:ISimpleDataService{

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var config = FileIndexerConfig.Settings;

            var directory = new DirectoryInfo(config.DirectoryToIndex);


            FileInfo[] files;
            if(config.Recursive)
            {
                files = directory.GetFiles(config.SupportedFileTypes,
                                                   SearchOption.AllDirectories);
            }    
            else
            {
                files = directory.GetFiles(config.SupportedFileTypes);
            }

            var ds = new List<SimpleDataSet>();
            int i = 1; //unique id for each doc
            foreach (var file in files)
            {
                try
                {
                    var sd = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                    sd = ExamineHelper.MapFileToSimpleDataIndexItem(file, sd, i, indexType);

                    ds.Add(sd);
                }
                catch (Exception ex) {
                    Log.Add(LogTypes.Error, i, "error processing file  " + file.FullName + " " + ex.ToString());
                }
                
                i++;
            }

            return ds;
        }
    }
}