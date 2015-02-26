using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace our.Examine
{
    /// <summary>
    /// Data service used for projects
    /// </summary>
    public class ProjectNodeIndexDataService : ISimpleDataService
    {
        public SimpleDataSet MapProjectToSimpleDataIndexItem(IPublishedContent project, SimpleDataSet simpleDataSet, int index, string indexType)
        {
            simpleDataSet.NodeDefinition.NodeId = project.Id;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", umbraco.library.StripHtml( project.GetProperty("description").Value.ToString() )) ;
            simpleDataSet.RowData.Add("nodeName", project.Name);
            simpleDataSet.RowData.Add("updateDate", project.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("nodeTypeAlias", "project");
            simpleDataSet.RowData.Add("url", project.Url );


            var karma = our.Utills.GetProjectTotalKarma(project.Id);
            var files = uWiki.Businesslogic.WikiFile.CurrentFiles(project.Id);
            var downloads = our.Utills.GetProjectTotalDownloadCount(project.Id);

            var image = files.Where(x => x.FileType == "screenshot").FirstOrDefault();
            var imageFile = "";
            if (image != null)
                imageFile = image.Path;

            //Clean up version data before its included in the index
            int o;
            var version = project.GetProperty("compatibleVersions").Value;
            var versions = version.ToString().ToLower()
                            .Replace("nan", "")
                            .Replace("saved", "")
                            .Replace("v", "")
                            .Trim(',').Split(',')
                            .Where(x => int.TryParse(x, out o))
                            .Select(x => (decimal.Parse(x.PadRight(3, '0') ) / 100));

            //popularity for sorting number = downloads + karma * 100;
            var pop = downloads + (karma * 100);

            simpleDataSet.RowData.Add("popularity", pop.ToString("D8"));
            simpleDataSet.RowData.Add("karma", karma.ToString());
            simpleDataSet.RowData.Add("downloads", downloads.ToString());
            simpleDataSet.RowData.Add("image", imageFile);
            simpleDataSet.RowData.Add("versions", string.Join(",", versions));

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var dataSets = new List<SimpleDataSet>();
            var projects = Umbraco.Web.UmbracoContext.Current.ContentCache.GetByXPath("//Community/Projects//Project [projectLive='1']");
           
            //index all projects
            for (int i = 0; i < projects.Count(); i++)
            {
                var project = projects.ElementAt(i);
                try
                {
                    LogHelper.Debug(this.GetType(), "Indexing " + project.Name);

                    var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };
                    simpleDataSet = MapProjectToSimpleDataIndexItem(project, simpleDataSet, project.Id, indexType);
                    dataSets.Add(simpleDataSet);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(this.GetType(), ex.Message, ex);
                }
            }

            return dataSets;
        }

        /// <summary>
        /// Need to ensures some custom data is added to this index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ProjectIndexer_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            //Need to add category, which is a parent folder if it has one, we only care about published data
            // so we can just look this up from the published cache

            //TODO: To get at the IPublishedCaches it is only available on the UmbracoContext (which we need to fix)
            // but since this method operates async, there isn't one, so we need to make our own to get at the cache
            // object by creating a fake HttpContext. Not pretty but it works for now.
            if (UmbracoContext.Current == null)
            {
                var dummyHttpContext = new HttpContextWrapper(
                    new HttpContext(
                        new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));
                UmbracoContext.EnsureContext(dummyHttpContext,
                    ApplicationContext.Current,
                    new WebSecurity(dummyHttpContext, ApplicationContext.Current), false);
            }

            var node = UmbracoContext.Current.ContentCache.GetById(e.NodeId);
            if (node == null) return;

            //this has a project group which is it's category
            if (node.Parent.DocumentTypeAlias == "ProjectGroup")
            {
                e.Fields["categoryFolder"] = node.Parent.Name;
            }


        }      
    }
}
