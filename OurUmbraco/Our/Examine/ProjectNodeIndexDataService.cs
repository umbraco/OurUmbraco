using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
using OurUmbraco.Project;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace OurUmbraco.Our.Examine
{

    /// <summary>
    /// Data service used for projects
    /// </summary>
    public class ProjectNodeIndexDataService : ISimpleDataService
    {
        public SimpleDataSet MapProjectToSimpleDataIndexItem(IPublishedContent project, SimpleDataSet simpleDataSet, string indexType,
            int projectVotes, WikiFile[] files, int downloads, IEnumerable<string> compatVersions)
        {
            var isLive = project.GetPropertyValue<bool>("projectLive");
            var isApproved = project.GetPropertyValue<bool>("approved");

            simpleDataSet.NodeDefinition.NodeId = project.Id;
            simpleDataSet.NodeDefinition.Type = indexType;

            simpleDataSet.RowData.Add("body", project.GetPropertyValue<string>("description"));
            simpleDataSet.RowData.Add("nodeName", project.Name);
            simpleDataSet.RowData.Add("categoryFolder", project.Parent.Name.ToLowerInvariant().Trim());
            simpleDataSet.RowData.Add("updateDate", project.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("createDate", project.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
            simpleDataSet.RowData.Add("nodeTypeAlias", "project");
            simpleDataSet.RowData.Add("url", project.Url);
            simpleDataSet.RowData.Add("uniqueId", project.GetPropertyValue<string>("packageGuid"));
            simpleDataSet.RowData.Add("worksOnUaaS", project.GetPropertyValue<string>("worksOnUaaS"));

            var imageFile = string.Empty;
            if (project.HasValue("defaultScreenshotPath"))
            {
                imageFile = project.GetPropertyValue<string>("defaultScreenshotPath");
            }
            if (string.IsNullOrWhiteSpace(imageFile))
            {
                var image = files.FirstOrDefault(x => x.FileType == "screenshot");
                if (image != null)
                    imageFile = image.Path;
            }

            //Clean up version data before its included in the index, the reason we have to do this
            // is due to the way the version data is stored, you can see it in uVersion.config - it's super strange
            // because of the 3 digit nature but when it doesn't end with a '0' it's actually just the major/minor version
            // so we have to do all of this parsing.
            var version = project.GetPropertyValue<string>("compatibleVersions") ?? string.Empty;
            var cleanedVersions = version.ToLower()
                .Replace("nan", "")
                .Replace("saved", "")
                .Replace("v", "")
                .Trim(',')
                .Split(',')
                //it's stored as an int like 721 (for version 7.2.1)
                .Where(x => x.Length <= 3 && x.Length > 0)
                //pad it out to 3 digits
                .Select(x => x.PadRight(3, '0'))
                .Select(x =>
                {
                    int o;
                    if (int.TryParse(x, out o))
                    {
                        //if it ends with '0', that means it's a X.X.X version
                        // if it does not end with '0', that means that the last 2 digits are the 
                        // Minor part of the version
                        return x.EndsWith("0")
                            ? string.Format("{0}.{1}.{2}", x[0], x[1], 0)
                            : string.Format("{0}.{1}.{2}", x[0], x.Substring(1), 0);
                    }
                    return null;
                })
                .Where(x => x != null);

            var cleanedCompatVersions = compatVersions.Select(x => x.Replace("nan", "")
                .Replace("saved", "")
                .Replace("nan", "")
                .Replace("v", "")
                .Replace(".x", "")
                .Trim(','));

            //popularity for sorting number = downloads + karma * 100;
            //TODO: Change score so that we take into account:
            // - recently updated
            // - works on latest umbraco versions
            // - works on uaas
            // - has a forum
            // - has source code link
            // - open for collab / has collaborators
            // - download count in a recent timeframe - since old downloads should count for less

            var pop = downloads + (projectVotes * 100);

            simpleDataSet.RowData.Add("popularity", pop.ToString());
            simpleDataSet.RowData.Add("karma", projectVotes.ToString());
            simpleDataSet.RowData.Add("downloads", downloads.ToString());
            simpleDataSet.RowData.Add("image", imageFile);

            var packageFiles = files.Count(x => x.FileType == "package");
            simpleDataSet.RowData.Add("packageFiles", packageFiles.ToString());

            simpleDataSet.RowData.Add("projectLive", isLive ? "1" : "0");
            simpleDataSet.RowData.Add("approved", isApproved ? "1" : "0");

            //now we need to add the versions and compat versions
            // first, this is the versions that the project has files tagged against
            simpleDataSet.RowData.Add("versions", string.Join(",", cleanedVersions));
            //then we index the versions that the project has actually been flagged as compatible against
            simpleDataSet.RowData.Add("compatVersions", string.Join(",", cleanedCompatVersions));

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var umbContxt = EnsureUmbracoContext();

            var projects = umbContxt.ContentCache.GetByXPath("//Community/Projects//Project [projectLive='1']").ToArray();

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
                var projectFiles = allProjectWikiFiles.ContainsKey(project.Id) ? allProjectWikiFiles[project.Id].ToArray() : new WikiFile[] { };
                var projectVersions = allCompatVersions.ContainsKey(project.Id) ? allCompatVersions[project.Id] : Enumerable.Empty<string>();

                yield return MapProjectToSimpleDataIndexItem(project, simpleDataSet, indexType, projectKarma, projectFiles, projectDownloads, projectVersions);
            }
        }

        /// <summary>
        /// Given the string versions, this will put them into the index as numerical versions, this way we can compare/range query, etc... on versions
        /// </summary>
        /// <param name="e"></param>
        /// <param name="fieldName"></param>
        /// <param name="versions"></param>
        /// <remarks>
        /// This stores a numerical version as a Right padded 3 digit combined long number. Example:
        /// 7.5.0 would be:
        ///     007005000 = 7005000
        /// 4.11.0 would be:
        ///     004011000 = 4011000
        /// </remarks>
        private static void AddNumericalVersionValue(DocumentWritingEventArgs e, string fieldName, IEnumerable<string> versions)
        {
            var numericalVersions = versions.Select(x =>
                {
                    System.Version o;
                    return System.Version.TryParse(x, out o) ? o : null;
                })
                .Where(x => x != null)
                .Select(x => x.GetNumericalValue())
                .ToArray();

            foreach (var numericalVersion in numericalVersions)
            {
                //don't store, we're just using this to search
                var versionField = new NumericField(fieldName, Field.Store.NO, true).SetLongValue(numericalVersion);
                e.Document.Add(versionField);
            }
        }

        /// <summary>
        /// Handle custom Lucene indexing when the lucene document is writing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ProjectIndexer_DocumentWriting(object sender, DocumentWritingEventArgs e)
        {
            //if there is a "body" field, we'll strip the html but also store it's raw value
            if (e.Fields.ContainsKey("body"))
            {
                //store the raw value
                e.Document.Add(new Field(
                    string.Concat(LuceneIndexer.SpecialFieldPrefix, "body"),
                    e.Fields["body"],
                    Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
                //remove the current version field from the lucene doc
                e.Document.RemoveField("body");
                //add a 'body' field with stripped html
                e.Document.Add(new Field("body", library.StripHtml(e.Fields["body"]), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            }

            //If there is a versions field, we'll split it and index the same field on each version
            if (e.Fields.ContainsKey("versions"))
            {
                //split into separate versions
                var versions = e.Fields["versions"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                AddNumericalVersionValue(e, "num_versions", versions);

                //remove the current version field from the lucene doc
                e.Document.RemoveField("versions");

                foreach (var version in versions)
                {
                    //add a 'versions' field for each version (same field name but different values)
                    //not analyzed, we don't use this for searching
                    e.Document.Add(new Field("versions", version, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
                }
            }

            //If there is a compatVersions field, we'll split it and index the same field on each version
            if (e.Fields.ContainsKey("compatVersions"))
            {
                //split into separate versions
                var compatVersions = e.Fields["compatVersions"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                AddNumericalVersionValue(e, "num_compatVersions", compatVersions);

                //remove the current compatVersions field from the lucene doc
                e.Document.RemoveField("compatVersions");

                foreach (var version in compatVersions)
                {
                    //add a 'compatVersions' field for each compatVersion (same field name but different values)
                    //not analyzed, we don't use this for searching
                    e.Document.Add(new Field("compatVersions", version, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
                }
            }
        }

        private static UmbracoContext EnsureUmbracoContext()
        {
            //TODO: To get at the IPublishedCaches it is only available on the UmbracoContext (which we need to fix)
            // but since this method operates async, there isn't one, so we need to make our own to get at the cache
            // object by creating a fake HttpContext. Not pretty but it works for now.
            if (UmbracoContext.Current != null)
                return UmbracoContext.Current;

            var dummyHttpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));

            return UmbracoContext.EnsureContext(dummyHttpContext,
                ApplicationContext.Current,
                new WebSecurity(dummyHttpContext, ApplicationContext.Current),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviderResolver.Current.Providers,
                false);
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

            var umbContxt = EnsureUmbracoContext();

            if (e.Fields["categoryFolder"].IsNullOrWhiteSpace())
            {
                var node = umbContxt.ContentCache.GetById(e.NodeId);
                if (node == null) return;

                //this has a project group which is it's category
                if (node.Parent.DocumentTypeAlias == "ProjectGroup")
                {
                    e.Fields["categoryFolder"] = node.Parent.Name.ToLowerInvariant().Trim();
                }
            }

        }
    }
}
