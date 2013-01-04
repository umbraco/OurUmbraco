using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using System.Xml.Linq;

using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;

using System.Text.RegularExpressions;
using uDocumentation.Busineslogic;
using our.Examine.DocumentationIndexDataService.Helper;

namespace our
{
    public class DocumentationIndexer : ApplicationBase
    {
        

        public DocumentationIndexer()
        {
            uDocumentation.Busineslogic.GithubSourcePull.ZipDownloader.OnFinish += ZipDownloader_OnFinish;
        }

        void ZipDownloader_OnFinish(object sender, FinishEventArgs e)
        {
            var indexer = (SimpleDataIndexer)ExamineManager.Instance.IndexProviderCollection[ExamineHelper.DocumentationIndexer];
            indexer.RebuildIndex();
        }

    }
}