using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;
using Lucene.Net.Documents;
using Lucene.Net.Util;
using uWiki.Businesslogic;
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
        public SimpleDataSet MapProjectToSimpleDataIndexItem(IPublishedContent project, SimpleDataSet simpleDataSet, string indexType,
            int karma, IEnumerable<WikiFile> files, int downloads, IEnumerable<string> compatVersions)
        {
            simpleDataSet.NodeDefinition.NodeId = project.Id;
            simpleDataSet.NodeDefinition.Type = indexType;

            var desciption = project.GetPropertyValue<string>("description");
            if (!string.IsNullOrEmpty(desciption))
            {
                simpleDataSet.RowData.Add("body", umbraco.library.StripHtml(desciption));
            }
            simpleDataSet.RowData.Add("nodeName", project.Name);
            simpleDataSet.RowData.Add("updateDate", project.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("createDate", project.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("nodeTypeAlias", "project");
            simpleDataSet.RowData.Add("url", project.Url );
            simpleDataSet.RowData.Add("uniqueId", project.GetPropertyValue<string>("packageGuid"));

            var image = files.FirstOrDefault(x => x.FileType == "screenshot");
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

            simpleDataSet.RowData.Add("popularity", pop.ToString());
            simpleDataSet.RowData.Add("karma", karma.ToString());
            simpleDataSet.RowData.Add("downloads", downloads.ToString());
            simpleDataSet.RowData.Add("image", imageFile);

            //now we need to add the versions and compat versions
            // first, this is the versions that the project has files tagged against
            simpleDataSet.RowData.Add("versions", string.Join(",", versions));

            //then we index the versions that the project has actually been flagged as compatible against
            simpleDataSet.RowData.Add("compatVersions", string.Join(",", compatVersions));

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            EnsureUmbracoContext();

            var projects = UmbracoContext.Current.ContentCache.GetByXPath("//Community/Projects//Project [projectLive='1']").ToArray();

            var allProjectIds = projects.Select(x => x.Id).ToArray();
            var allProjectKarma = Utils.GetProjectTotalVotes();
            var allProjectWikiFiles = WikiFile.CurrentFiles(allProjectIds);
            var allProjectDownloads = Utils.GetProjectTotalDownload();
            var allCompatVersions = Utils.GetProjectCompatibleVersions();

            foreach (var project in projects)
            {
                LogHelper.Debug(this.GetType(), "Indexing " + project.Name);

                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                var projectDownloads = allProjectDownloads.ContainsKey(project.Id) ? allProjectDownloads[project.Id] : 0;
                var projectKarma = allProjectKarma.ContainsKey(project.Id) ? allProjectKarma[project.Id] : 0;
                var projectFiles = allProjectWikiFiles.ContainsKey(project.Id) ? allProjectWikiFiles[project.Id] : Enumerable.Empty<WikiFile>();
                var projectVersions = allCompatVersions.ContainsKey(project.Id) ? allCompatVersions[project.Id] : Enumerable.Empty<string>();

                yield return MapProjectToSimpleDataIndexItem(project, simpleDataSet, indexType, projectKarma, projectFiles, projectDownloads, projectVersions);
            }
        }

        /// <summary>
        /// Handle custom Lucene indexing when the lucene document is writing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ProjectIndexer_DocumentWriting(object sender, DocumentWritingEventArgs e)
        {
            //TODO: This will be good to do but we need the bleeding edge version of examine v1.x which i haven't released yet

            ////If there is a versions field, we'll split it and index the same field on each version
            //if (e.Fields.ContainsKey("versions"))
            //{
            //    //split into separate versions
            //    var versions = e.Fields["versions"].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);

            //    //remove the current version field from the lucene doc
            //    e.Document.RemoveField("versions");

            //    foreach (var version in versions)
            //    {
            //        //add a 'versions' field for each version (same field name but different values)
            //        e.Document.Add(new Field("versions", version, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            //    }
            //}
        }

        private static void EnsureUmbracoContext()
        {
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

            EnsureUmbracoContext();

            var node = UmbracoContext.Current.ContentCache.GetById(e.NodeId);
            if (node == null) return;

            //this has a project group which is it's category
            if (node.Parent.DocumentTypeAlias == "ProjectGroup")
            {
                e.Fields["categoryFolder"] = node.Parent.Name.ToLowerInvariant().Trim();
            }


        }      
    }
}
