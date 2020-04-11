using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Examine;
using Examine.SearchCriteria;
using OurUmbraco.MarketPlace.Extensions;
using OurUmbraco.MarketPlace.Interfaces;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Our;
using OurUmbraco.Our.Examine;
using OurUmbraco.Wiki.Extensions;
using umbraco;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace OurUmbraco.MarketPlace.NodeListing
{
    public class NodeListingProvider
    {
        /// <summary>
        /// get project listing based on ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="optimized"></param>
        /// <param name="projectKarma"></param>
        /// <returns></returns>
        public IListingItem GetListing(int id, bool optimized = false)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContent(id);

            if (content != null)
                return GetListing(content, optimized);

            throw new NullReferenceException("Content is Null cannot find a node with the id:" + id);
        }

        /// <summary>
        /// get project listing  based on GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <returns></returns>
        public IListingItem GetListing(Guid guid, bool optimized = false)
        {
            var strGuid = guid.ToString().ToUpper();

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);

            // we have to use the translate function to ensure that the casing is the same for comparison as there are GUIDS in the db in both upper and lowercase
            var contents =
                umbracoHelper.TypedContentAtXPath(
                    string.Format(
                        "//Project [@isDoc and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') = translate('{0}','ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')]",
                        strGuid));

            var content = contents.FirstOrDefault();

            if (content != null)
            {
                return content.ToIListingItem(optimized);
            }

            throw new NullReferenceException("Node is Null cannot find a node with the guid:" + strGuid);
        }

        /// <summary>
        /// get listing
        /// </summary>
        /// <param name="content"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="projectKarma"></param>
        /// <returns></returns>
        public IListingItem GetListing(IPublishedContent content, bool optimized = false, int? projectKarma = null)
        {
            if (content == null) throw new ArgumentNullException("content");

            //TODO: could easily cache this for a short period of time

            var listingItem = new ListingItem.PublishedContentListingItem(content);

            //this section was created to speed up loading operations and cut down on the number of database interactions
            // TODO: N+1+1+1+1, etc...
            if (optimized == false)
            {
                listingItem.Karma = projectKarma ?? GetProjectKarma(content.Id);
                listingItem.Downloads = GetProjectDownloadCount(content.Id);
                listingItem.DocumentationFile = GetMediaForProjectByType(content.Id, FileType.docs);
                listingItem.ScreenShots = GetMediaForProjectByType(content.Id, FileType.screenshot);
                listingItem.PackageFile = GetMediaForProjectByType(content.Id, FileType.package);
                listingItem.HotFixes = GetMediaForProjectByType(content.Id, FileType.hotfix);
                listingItem.SourceFile = GetMediaForProjectByType(content.Id, FileType.source);
            }

            return listingItem;
        }


        private static int GetProjectDownloadCount(int projectId)
        {
            try
            {
                var searchFilters = new SearchFilters(BooleanOperation.And);

                searchFilters.Filters.Add(new SearchFilter("__NodeId", projectId));

                var filters = new List<SearchFilters> { searchFilters };

                var ourSearcher = new OurSearcher(null, "project", filters: filters);
                            
                var results = ourSearcher.Search("projectSearcher");

                if(results.SearchResults.TotalItemCount > 0)
                {
                    var packageResult = results.SearchResults.First();

                    if (packageResult.Fields.ContainsKey("downloads"))
                    {
                        return int.Parse(packageResult.Fields["downloads"]);
                    }
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public int GetProjectKarma(int projectId)
        {
            using (var sqlHelper = Application.SqlHelper)
            using (var reader = sqlHelper.ExecuteReader("SELECT SUM([points]) AS Karma FROM powersProject WHERE id = @projectId", sqlHelper.CreateParameter("@projectId", projectId)))
            {
                if (reader.Read())
                {
                    var karma = reader.GetInt("Karma");
                    if (karma > 0) 
                    {
                        return karma;
                    }
                    return 0;
                }
                     
            }

            return 0;
        }

        public IEnumerable<IMediaFile> GetMediaForProjectByType(int projectId, FileType type)
        {
            var mediaProvider = new MediaProvider();
            return mediaProvider.GetMediaForProjectByType(projectId, type);
        }

        /// <summary>
        /// Persists the listing object to the database
        /// </summary>
        /// <param name="listingItem"></param>
        public void SaveOrUpdate(IListingItem listingItem)
        {
            var contentService = ApplicationContext.Current.Services.ContentService;
            //check if this is a new listing or an existing one.
            var isUpdate = listingItem.Id != 0;
            var content = (isUpdate)
                ? contentService.GetById(listingItem.Id)
                : contentService.CreateContent(listingItem.Name, listingItem.CategoryId, "Project");

            var packageGuidValue = content.GetValue<string>("packageGuid");
            var packageGuidString = Guid.TryParse(packageGuidValue, out Guid packageGuid)
                ? packageGuid.ToString()
                : Guid.NewGuid().ToString();

            //set all the document properties
            content.SetValue("description", listingItem.Description);
            content.SetValue("version", listingItem.CurrentVersion);
            content.SetValue("file", listingItem.CurrentReleaseFile);
            content.SetValue("status", listingItem.DevelopmentStatus);
            content.SetValue("stable", (listingItem.Stable) ? "1" : "0");
            content.SetValue("projectLive", (listingItem.Live) ? "1" : "0");
            content.SetValue("listingType", listingItem.ListingType.GetListingTypeAsString());
            content.SetValue("gaCode", listingItem.GACode);
            content.SetValue("category", listingItem.CategoryId);
            content.SetValue("licenseName", listingItem.LicenseName);
            content.SetValue("licenseUrl", listingItem.LicenseUrl);
            content.SetValue("supportUrl", listingItem.SupportUrl);
            content.SetValue("sourceUrl", listingItem.SourceCodeUrl);
            content.SetValue("nuGetPackageUrl", listingItem.NuGetPackageUrl);
            content.SetValue("demoUrl", listingItem.DemonstrationUrl);
            content.SetValue("openForCollab", listingItem.OpenForCollab);
            content.SetValue("notAPackage", listingItem.NotAPackage);
            content.SetValue("packageGuid", packageGuidString);
            content.SetValue("approved", (listingItem.Approved) ? "1" : "0");
            if(isUpdate == false)
                content.SetValue("termsAgreementDate", listingItem.TermsAgreementDate);
            content.SetValue("owner", listingItem.VendorId);
            content.SetValue("websiteUrl", listingItem.ProjectUrl);
            content.SetValue("licenseKey", listingItem.LicenseKey);
            content.SetValue("isRetired", listingItem.IsRetired);
            content.SetValue("retiredMessage", listingItem.RetiredMessage);

            if (listingItem.PackageFile != null)
            {
                var currentFiles = listingItem.PackageFile.Where(x => x.Current && x.Archived == false).ToList();
                var supportedDotNetVersions = new List<string>();
                foreach (
                    var currentFile in currentFiles.Where(x => string.IsNullOrWhiteSpace(x.DotNetVersion) == false && x.DotNetVersion != "nan" && supportedDotNetVersions.Contains(x.DotNetVersion) == false))
                {
                    supportedDotNetVersions.Add(currentFile.DotNetVersion);
                }

                content.SetValue("dotNetVersion", string.Join(",", supportedDotNetVersions));

                var supportedUmbracoVersions = new List<string>();
                foreach (var currentFile in currentFiles.Where(x => string.IsNullOrWhiteSpace(x.UmbVersion.ToVersionString()) == false && x.UmbVersion.ToVersionString() != "nan" && supportedUmbracoVersions.Contains(x.UmbVersion.ToVersionString()) == false))
                {
                    supportedUmbracoVersions.Add(currentFile.UmbVersion.ToVersionString());
                }
                content.SetValue("compatibleVersions", string.Join(",", supportedUmbracoVersions));
            }

            //set the files
            content.SetValue("defaultScreenshotPath", listingItem.DefaultScreenshot);

            if (listingItem.Tags != null && listingItem.Tags.Any())
            {
                var tags = new List<string>();
                foreach (var projectTag in listingItem.Tags.Where(projectTag => tags.Any(x => string.Compare(x, projectTag.Text, StringComparison.InvariantCultureIgnoreCase) == 0)))
                {
                    tags.Add(projectTag.Text);
                }
                content.SetTags("tags", tags, true, "project");
            }


            if (listingItem.DocumentationFile != null)
            {
                if (listingItem.DocumentationFile.Any())
                {
                    content.SetValue("documentation", listingItem.DocumentationFile.OrderBy(x => x.Current).First().Path);
                }
            }

            if (listingItem.IsRetired)
                listingItem.Live = false;

            contentService.SaveAndPublishWithStatus(content);

            listingItem.Id = content.Id;
            listingItem.NiceUrl = library.NiceUrl(listingItem.Id);

            var indexer = ExamineManager.Instance.IndexProviderCollection["projectIndexer"];
            if(indexer != null && listingItem.IsRetired)
                indexer.DeleteFromIndex(listingItem.Id.ToString());
        }


        /// <summary>
        /// Gets all listings for a vendor
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="all">If set returns both live and not live listings</param>
        /// <returns></returns>
        public IEnumerable<IListingItem> GetListingsByVendor(int vendorId, bool optimized = false, bool all = false)
        {
            var contents = GetProjectsFromDeliProjectRoot(all).Where(c => c.GetPropertyValue<int>("owner") == vendorId);

            return contents.ToIListingItemList(optimized);
        }

        private static IEnumerable<IPublishedContent> GetProjectsFromDeliProjectRoot(bool all)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var content = umbracoHelper.TypedContent(int.Parse(ConfigurationManager.AppSettings["deliProjectRoot"]));
            if (content == null)
                throw new Exception("Could not find the Deli project root.");
            var contents = content.Descendants().Where(c => c.DocumentTypeAlias == "Project");

            if (all == false)
                contents = contents.Where(p => p.GetPropertyValue<bool>("projectLive"));

            return contents;
        }

        /// <summary>
        /// Returns a listing of projects that a specified member contributes to.
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="all">if set returns both live and not live projects.</param>
        /// <returns></returns>
        public IEnumerable<IListingItem> GetListingsForContributor(int memberId, bool optimized = false, bool all = false)
        {

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var contribProjects = new List<IPublishedContent>();
            const string sql = @"SELECT * FROM projectContributors WHERE memberId=@memberId";
            var contribPackageIds = ApplicationContext.Current.DatabaseContext.Database.Fetch<int>(sql, new { memberId });

            foreach (var contribPackageId in contribPackageIds)
            {
                var contribPackage = umbracoHelper.TypedContent(contribPackageId);
                if (contribPackage != null)
                {
                    contribProjects.Add(contribPackage);
                }
            }

            var listings = new List<IListingItem>();
            foreach (var contribItem in contribProjects)
            {
                listings.Add(GetListing(contribItem.Id, optimized));
            }

            return listings;
        }

        /// <summary>
        /// gets a list of listings
        /// </summary>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="all">if set returns both live and not live listings</param>
        /// <returns></returns>
        public IEnumerable<IListingItem> GetAllListings(bool optimized = false, bool all = false)
        {
            return GetAllListings(0, 0, optimized, all);
        }

        /// <summary>
        /// gets paged list of listings
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="all">if set returns both live and not live listings</param>
        /// <returns></returns>
        public IEnumerable<IListingItem> GetAllListings(int skip, int take, bool optimized = false, bool all = false)
        {
            var contents = GetProjectsFromDeliProjectRoot(all);

            if (take > 0)
                contents = contents.Skip(skip).Take(take);

            return contents.ToIListingItemList(optimized);
        }

        /// <summary>
        /// Gets a paged list of listings by specified category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="all">if set returns both live and not live listings</param>
        /// <returns></returns>
        public IEnumerable<IListingItem> GetListingsByCategory(ICategory category, int skip, int take, bool optimized = false, bool all = false)
        {
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var categoryContent = umbracoHelper.TypedContent(category.Id);
            var items = categoryContent
                .Children(x => x.DocumentTypeAlias == "Project");
            if (!all)
                items = items.Where(x => x.GetPropertyValue<bool>("projectLive"));
            if (take > 0)
                items = items.Skip(skip).Take(take);
            return items.Select(x => x.ToIListingItem(optimized));
        }

        /// <summary>
        /// Returns a paged list of projects ordered by Karma
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="optimized">if set performs less DB interactions to increase speed.</param>
        /// <param name="all">If set returns both live and not live</param>
        /// <returns></returns>
        public IEnumerable<IListingItem> GetListingsByKarma(int skip, int take, bool optimized = false, bool all = false)
        {
            var karmaProvider = new KarmaProvider();
            var projectsByKarma = karmaProvider.GetProjectsKarmaList();
            var projectsFromDeliProjectRoot = GetProjectsFromDeliProjectRoot(false).ToArray();

            var items = projectsByKarma
                .Select(x => Tuple.Create(x, projectsFromDeliProjectRoot.FirstOrDefault(y => y.Id == x.ProjectId)))
                .Where(x => x.Item2 != null);

            if (take > 0)
                items = items.Skip(skip).Take(take);

            return items.Select(x => GetListing(x.Item2, optimized, x.Item1.Points));
        }

        public IEnumerable<IPublishedContent> GetVotedProjectsForMember(int memberId)
        {
            var votedProjects = new List<IPublishedContent>();
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var projectsVoted = Utils.GetProjectsMemberHasVotedUp(memberId);

            foreach (var projectId in projectsVoted)
            {
                var content = umbracoHelper.TypedContent(projectId);
                if (content != null)
                    votedProjects.Add(content);
            }

            return votedProjects;
        }

        public IEnumerable<IPublishedContent> GetDownloadedProjectsForMember(int memberId)
        {
            var downloadedProjects = new List<IPublishedContent>();
            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var projectsDownloaded = Utils.GetProjectsMemberHasDownloaded(memberId);

            foreach (var projectId in projectsDownloaded)
            {
                var content = umbracoHelper.TypedContent(projectId);
                if (content != null)
                    downloadedProjects.Add(content);
            }

            return downloadedProjects;
        }
    }
}