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
using OurUmbraco.Repository.Services;
using OurUmbraco.Wiki.BusinessLogic;
using OurUmbraco.Wiki.Models;
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
    using OurUmbraco.Community.Nuget;

    /// <summary>
    /// Data service used for projects
    /// </summary>
    public class ProjectNodeIndexDataService : ISimpleDataService
    {
        

        public SimpleDataSet MapProjectToSimpleDataIndexItem(
            IDictionary<int, MonthlyProjectDownloads> projectDownloadStats,
            DateTime mostRecentUpdateDate,
            IPublishedContent project, SimpleDataSet simpleDataSet, string indexType,
            int projectVotes, WikiFile[] files, int downloads, IEnumerable<string> compatVersions, int? dailyNugetDownLoads = null)
        {
            var isLive = project.GetPropertyValue<bool>("projectLive");
            var isApproved = project.GetPropertyValue<bool>("approved");

            var strictPackageFiles = PackageRepositoryService.GetAllStrictSupportedPackageVersions(files);
            
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
            simpleDataSet.RowData.Add("isRetired", project.GetPropertyValue<bool>("isRetired", false) ? "1" : "0");

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
                .Split(',')
                .Select(x => x.Trim().GetFromUmbracoString(reduceToConfigured:false))
                .Where(x => x != null);

            var cleanedCompatVersions = compatVersions
                .Select(x => x.GetFromUmbracoString(reduceToConfigured: false))
                .Where(x => x != null);
            
            var hasForum = project.Children.Any(x => x.IsVisible());

            MonthlyProjectDownloads projStats = null;
            projectDownloadStats.TryGetValue(project.Id, out projStats);

            var points = new ProjectPopularityPoints(                
                mostRecentUpdateDate,
                projStats,
                project.CreateDate, 
                project.UpdateDate,
                project.GetPropertyValue<bool>("worksOnUaaS"), hasForum,
                project.GetPropertyValue<string>("sourceUrl").IsNullOrWhiteSpace() == false,
                project.GetPropertyValue<bool>("openForCollab"),
                downloads, 
                projectVotes,
                dailyNugetDownLoads);
            var pop = points.Calculate();

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

            simpleDataSet.RowData.Add("minimumVersionStrict", string.Join(",", strictPackageFiles.Select(x => x.MinUmbracoVersion.ToString(3))));

            return simpleDataSet;
        }

        public IEnumerable<SimpleDataSet> GetAllData(string indexType)
        {
            var umbContxt = EnsureUmbracoContext();

            var projects = umbContxt.ContentCache.GetByXPath("//Community/Projects//Project [projectLive='1']").ToArray();

            var nugetService = new NugetPackageDownloadService();

            var nugetDownloads = nugetService.GetNugetPackageDownloads();

            var allProjectIds = projects.Select(x => x.Id).ToArray();
            var allProjectKarma = Utils.GetProjectTotalVotes();
            var allProjectWikiFiles = WikiFile.CurrentFiles(allProjectIds);
            var allProjectDownloads = Utils.GetProjectTotalPackageDownload();
            var allCompatVersions = Utils.GetProjectCompatibleVersions();
            var mostRecentDownloadDate = WikiFile.GetMostRecentDownloadDate();
            var downloadStats = WikiFile.GetMonthlyDownloadStatsByProject(mostRecentDownloadDate.Subtract(TimeSpan.FromDays(365)));            

            foreach (var project in projects)
            {
                LogHelper.Debug(this.GetType(), "Indexing " + project.Name);

                var simpleDataSet = new SimpleDataSet { NodeDefinition = new IndexedNode(), RowData = new Dictionary<string, string>() };

                var projectDownloads = allProjectDownloads.ContainsKey(project.Id) ? allProjectDownloads[project.Id] : 0;
                var projectKarma = allProjectKarma.ContainsKey(project.Id) ? allProjectKarma[project.Id] : 0;
                var projectFiles = allProjectWikiFiles.ContainsKey(project.Id) ? allProjectWikiFiles[project.Id].ToArray() : new WikiFile[] { };
                var projectVersions = allCompatVersions.ContainsKey(project.Id) ? allCompatVersions[project.Id] : Enumerable.Empty<string>();

                var nugetPackageId = nugetService.GetNuGetPackageId(project);

                int? dailyNugetDownLoads = null;

                if (!string.IsNullOrWhiteSpace(nugetPackageId))
                {
                    var packageInfo = nugetDownloads.FirstOrDefault(x => x.PackageId == nugetPackageId);

                    if (packageInfo != null)
                    {
                        projectDownloads +=  packageInfo.TotalDownLoads;
                        dailyNugetDownLoads = packageInfo.AverageDownloadPerDay;
                    }
                }

                yield return MapProjectToSimpleDataIndexItem(
                    downloadStats,
                    mostRecentDownloadDate,                    
                    project, simpleDataSet, indexType, projectKarma, projectFiles, projectDownloads, projectVersions, dailyNugetDownLoads);
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
                var versionField = new NumericField(fieldName, Field.Store.YES, true).SetLongValue(numericalVersion);
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
                e.Document.Add(new Field("body", e.Fields["body"].StripHtml(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES));
            }

            var allVersions = new HashSet<string>();

            //each of these contains versions, we want to parse them all into one list and then ensure each of these
            //fields are not analyzed (just stored since we dont use them for searching)
            var delimitedVersionFields = new[] {"versions", "minimumVersionStrict", "compatVersions"};

            foreach (var fieldName in delimitedVersionFields)
            {
                //If there is a versions field, we'll split it and index the same field on each version
                if (e.Fields.ContainsKey(fieldName))
                {
                    //split into separate versions
                    var versions = e.Fields[fieldName].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var version in versions)
                    {
                        allVersions.Add(version);
                    }
                    
                    //remove the current version field from the lucene doc
                    e.Document.RemoveField(fieldName);

                    foreach (var version in versions)
                    {
                        //add a 'versions' field for each version (same field name but different values)
                        //not analyzed, we don't use this for searching
                        e.Document.Add(new Field(fieldName, version, Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
                    }
                }
            }

            //now add all versions to a numerical field
            AddNumericalVersionValue(e, "num_version", allVersions.ToArray());            
            
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
