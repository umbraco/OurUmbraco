using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;
using System;

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

            var files = config.Recursive
                ? directory.GetFiles(config.SupportedFileTypes, SearchOption.AllDirectories)
                : directory.GetFiles(config.SupportedFileTypes);

            var i = 0; //unique id for each doc

            foreach (var file in files)
            {
                i++;
                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                try
                {
                    simpleDataSet = ExamineHelper.MapFileToSimpleDataIndexItem(file, simpleDataSet, i, indexType);
                }
                catch (Exception ex)
                {
                    Umbraco.Core.Logging.LogHelper.Error<DocumentationIndexDataService>(
                        $"Indexing docs - could not parse document {file.FullName}", ex);
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                }
                yield return simpleDataSet;
            }
            Umbraco.Core.Logging.LogHelper.Info<DocumentationIndexDataService>(
                        $"Indexed documentation files: {0}", () => files.Length);
        }
    }
}
