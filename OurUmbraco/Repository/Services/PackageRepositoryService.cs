using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.MarketPlace.Providers;
using OurUmbraco.Our;
using OurUmbraco.Project.Services;
using OurUmbraco.Repository.Controllers;
using OurUmbraco.Repository.Models;
using OurUmbraco.Wiki.BusinessLogic;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.Repository.Services
{
    internal class PackageRepositoryService
    {
        private readonly DatabaseContext DatabaseContext;
        private readonly MembershipHelper MembershipHelper;
        private readonly UmbracoHelper UmbracoHelper;

        const string BASE_URL = "https://our.umbraco.org";

        public PackageRepositoryService(UmbracoHelper umbracoHelper, MembershipHelper membershipHelper, DatabaseContext databaseContext)
        {
            UmbracoHelper = umbracoHelper;
            MembershipHelper = membershipHelper;
            DatabaseContext = databaseContext;
        }

        public IEnumerable<Models.Category> GetCategories()
        {
            var xpath = "//ProjectGroup[@isDoc]";
            var helper = new UmbracoHelper(UmbracoContext.Current);

            var items = helper.TypedContentAtXPath(xpath);

            return items
                .OrderBy(x => x.SortOrder)
                .Select(x => new Models.Category
                {
                    Icon = "",
                    Name = x.Name
                });
        }

        public PagedPackages GetPackages(
            int pageIndex,
            int pageSize,
            string category = null,
            string query = null,
            PackageSortOrder order = PackageSortOrder.Latest)
        {
            var items = Enumerable.Empty<IPublishedContent>();

            if (string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(query))
            {
                if (order == PackageSortOrder.Latest)
                {
                    // [LK:2016-06-13@CGRT16] This feels hacky, but unsure how else to get
                    // a list of all the newly created projects.
                    var xpath = "//Project[@isDoc and projectLive = 1 and approved = 1 and notAPackage = 0]";
                    items = UmbracoHelper
                        .TypedContentAtXPath(xpath)
                        .OrderByDescending(x => x.CreateDate);
                }
                else
                {
                    // [LK:2016-06-13@CGRT16] Attempting to reuse legacy code
                    var karmaProvider = new KarmaProvider();
                    var projectsByKarma = karmaProvider.GetProjectsKarmaList();

                    items = UmbracoHelper
                        .TypedContent(projectsByKarma.Select(x => x.ProjectId))
                        .Where(x =>
                            x.GetPropertyValue<bool>("projectLive") &&
                            x.GetPropertyValue<bool>("approved") &&
                            !x.GetPropertyValue<bool>("notAPackage"));
                }
            }
            else
            {
                var q = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(category))
                {
                    q.AppendFormat("+categoryFolder: \"{0}\" ", category);
                    q.Append(" ");
                }

                if (!string.IsNullOrWhiteSpace(query))
                {                    
                    q.AppendFormat(GenerateLuceneQuery(query));
                    q.Append(" ");
                }

                var searcher = ExamineManager.Instance.SearchProviderCollection["projectSearcher"];
                var criteria = searcher.CreateSearchCriteria().RawQuery(q.ToString());

                items = UmbracoHelper.TypedSearch(criteria, searcher);
            }

            if (items == null)
            {
                return null;
            }

            var packages = items
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(x => MapContentToPackage(x));

            IEnumerable<Models.Package> sorted;

            if (order == PackageSortOrder.Latest)
            {
                sorted = packages
                    .OrderByDescending(x => x.Created);
            }
            else
            {
                sorted = packages
                    .OrderByDescending(x => x.Downloads)
                    .ThenByDescending(x => x.Likes);
            }

            return new PagedPackages
            {
                Packages = sorted,
                Total = items.Count()
            };
        }

        private string GenerateLuceneQuery(string query)
        {
            var sb = new StringBuilder();
            if (query.Trim(new[] { '\"', '\'' }).IsNullOrWhiteSpace())
            {
                return string.Empty;
            }
            
            query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

            var querywords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //check if text is surrounded by single or double quotes, if so, then exact match
            var surroundedByQuotes = Regex.IsMatch(query, "^\".*?\"$")
                                     || Regex.IsMatch(query, "^\'.*?\'$");

            //node name exactly boost x 10
            sb.Append("(nodeName:");
            if (surroundedByQuotes == false) sb.Append("\"");
            sb.Append(query.ToLower());
            if (surroundedByQuotes == false) sb.Append("\"");
            sb.Append(")^10.0 ");

            //node name normally
            if (!surroundedByQuotes)
            {
                sb.Append("(nodeName:");
                foreach (var w in querywords)
                {
                    sb.Append(w.ToLower());
                }
                sb.Append(")^5 ");

                //node name normally with wildcards
                sb.Append("(nodeName:");
                foreach (var w in querywords)
                {
                    sb.Append(w.ToLower());
                    sb.Append("*");
                }
                sb.Append(") ");

                //node name normally with fuzzy
                sb.Append("(nodeName:");
                foreach (var w in querywords)
                {
                    sb.Append(w.ToLower());
                }
                sb.Append("~0.8) ");
            }

            //other fields to search that are less important
            var fields = new[] { "body" };

            foreach (var f in fields)
            {
                //additional fields exactly
                sb.Append("(");
                sb.Append(f);
                sb.Append(":");
                if (surroundedByQuotes == false) sb.Append("\"");
                sb.Append(query.ToLower());
                if (surroundedByQuotes == false) sb.Append("\"");
                sb.Append(")^0.7 ");

                //additional fields normally
                if (!surroundedByQuotes)
                {
                    sb.Append("(");
                    sb.Append(f);
                    sb.Append(":");
                    foreach (var w in querywords)
                    {
                        sb.Append(w.ToLower());
                    }
                    sb.Append(")^0.5 ");

                    //additional fields fuzzy
                    sb.Append("(");
                    sb.Append(f);
                    sb.Append(":");
                    foreach (var w in querywords)
                    {
                        sb.Append(w.ToLower());
                    }
                    //boost less for these fields
                    sb.Append("~0.8)^0.2 ");

                    //additional fields wildcard
                    sb.Append("(");
                    sb.Append(f);
                    sb.Append(":");
                    foreach (var w in querywords)
                    {
                        sb.Append(w.ToLower());
                        sb.Append("*");
                    }
                    //boost less for these fields with wildcard
                    sb.Append(")^0.1 ");
                }
            }

            return sb.ToString();
        }
    

        public Models.PackageDetails GetDetails(Guid id)
        {
            // [LK:2016-06-13@CGRT16] We're using XPath as we experienced issues with query Examine for GUIDs,
            // (it might worth but we were up against the clock).
            // The XPath 'translate' is being used to force the 'packageGuid' to be lowercase for comparison.
            var xpath = string.Format("//Project[@isDoc and projectLive = 1 and approved = 1 and translate(packageGuid,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz') = '{0}']", id.ToString("D").ToLowerInvariant());
            var item = UmbracoHelper.TypedContentSingleAtXPath(xpath);

            if (item == null)
                return null;

            return MapContentToPackageDetails(item);
        }

        private Models.Package MapContentToPackage(IPublishedContent content)
        {
            if (content == null)
                return null;

            var wikiFiles = WikiFile.CurrentFiles(content.Id);

            return new Models.Package
            {
                Category = content.Parent.Name,
                Created = content.CreateDate,
                Excerpt = GetPackageExcerpt(content, 12),
                Downloads = Utils.GetProjectTotalDownloadCount(content.Id),
                Id = content.GetPropertyValue<Guid>("packageGuid"),
                Likes = Utils.GetProjectTotalVotes(content.Id),
                Name = content.Name,
                Icon = GetThumbnailUrl(BASE_URL + content.GetPropertyValue<string>("defaultScreenshotPath", "/css/img/package2.png"), 154, 281),
                LatestVersion = content.GetPropertyValue<string>("version"),
                MinimumVersion = GetMinimumVersion(content, wikiFiles.Where(x => x.FileType.InvariantEquals("package"))),
                OwnerInfo = GetPackageOwnerInfo(content),
                Url = string.Concat(BASE_URL, content.Url)
            };
        }

        private Models.PackageDetails MapContentToPackageDetails(IPublishedContent content)
        {
            if (content == null)
                return null;

            var package = MapContentToPackage(content);
            var packageDetails = new PackageDetails(package);
            var wikiFiles = WikiFile.CurrentFiles(content.Id);

            packageDetails.Compatibility = GetPackageCompatibility(content);
            packageDetails.NetVersion = content.GetPropertyValue<string>("dotNetVersion");
            packageDetails.LicenseName = content.GetPropertyValue<string>("licenseName");
            packageDetails.LicenseUrl = content.GetPropertyValue<string>("licenseUrl");
            packageDetails.Description = content.GetPropertyValue<string>("description");
            packageDetails.Images = GetPackageImages(wikiFiles.Where(x => x.FileType.InvariantEquals("screenshot")), 154, 281);
            packageDetails.ExternalSources = GetExternalSources(content);
            packageDetails.ZipUrl = string.Concat(BASE_URL, "/FileDownload?id=", content.GetPropertyValue<string>("file"));

            return packageDetails;
        }

        private List<PackageCompatibility> GetPackageCompatibility(IPublishedContent content)
        {
            var service = new VersionCompatibilityService(DatabaseContext);
            var report = service.GetCompatibilityReport(content.Id);

            if (report == null || !report.Any())
                return null;

            return report
                .Select(x => new PackageCompatibility
                {
                    Percentage = x.Percentage,
                    Version = x.Version
                })
                .ToList();
        }

        private string GetMinimumVersion(IPublishedContent content, IEnumerable<WikiFile> packages)
        {
            var currentVersion = content.GetPropertyValue<int>("file");
            var latest = packages.FirstOrDefault(x => x.Id == currentVersion);

            if (latest == null || string.IsNullOrWhiteSpace(latest.Version.Version) || latest.Version.Version.InvariantEquals("nan"))
                return null;

            if (latest.Version.Version.InvariantStartsWith("v"))
            {
                var legacyFormat = latest.Version.Version;

                if (legacyFormat.InvariantStartsWith("v4"))
                {
                    return string.Concat(legacyFormat.Replace("v4", "4."), ".0");
                }
                else
                {
                    return string.Join(".", legacyFormat.ToCharArray().Skip(1));
                }
            }

            return latest.Version.Version;
        }

        private PackageOwnerInfo GetPackageOwnerInfo(IPublishedContent content)
        {
            var ownerId = content.GetPropertyValue<int>("owner");
            var owner = MembershipHelper.GetById(ownerId);

            var ownerInfo = new PackageOwnerInfo
            {
                Karma = owner.Karma(),
                Owner = owner.Name,
                OwnerAvatar = Utils.GetMemberAvatar(owner, 200, true)
            };

            if (content.GetPropertyValue<bool>("openForCollab", false))
            {
                var service = new ContributionService(DatabaseContext);
                var contributors = service.GetContributors(content.Id).ToList();

                if (contributors != null && contributors.Any())
                {
                    var names = new List<string>();

                    foreach (var contributor in contributors)
                    {
                        var member = MembershipHelper.GetById(contributor.MemberId);
                        if (member != null)
                        {
                            names.Add(member.Name);
                        }
                    }

                    ownerInfo.Contributors = names.ToArray();
                }
            }

            return ownerInfo;
        }

        private List<PackageImage> GetPackageImages(IEnumerable<WikiFile> images, int thumbnailHeight, int thumbnailWidth)
        {
            var items = new List<PackageImage>();

            foreach (var image in images)
            {
                var url = string.Concat(BASE_URL, image.Path);

                items.Add(new PackageImage
                {
                    Source = url,
                    Thumbnail = GetThumbnailUrl(url, thumbnailHeight, thumbnailWidth)
                });
            }

            return items;
        }

        private string GetThumbnailUrl(string url, int height, int width)
        {
            return string.Format("{0}?bgcolor=fff&height={1}&width={2}&format=png", url, height, width);
        }

        private List<ExternalSource> GetExternalSources(IPublishedContent content)
        {
            var items = new List<ExternalSource>();

            if (content.HasValue("websiteUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Project website",
                    Url = content.GetPropertyValue<string>("websiteUrl")
                });
            }

            if (content.HasValue("sourceUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Source code",
                    Url = content.GetPropertyValue<string>("sourceUrl")
                });
            }

            if (content.HasValue("demoUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Demonstration",
                    Url = content.GetPropertyValue<string>("demoUrl")
                });
            }

            if (content.HasValue("supportUrl"))
            {
                items.Add(new ExternalSource
                {
                    Name = "Issue tracker",
                    Url = content.GetPropertyValue<string>("supportUrl")
                });
            }

            return items;
        }

        private string GetPackageExcerpt(IPublishedContent content, int count = 35)
        {
            var input = content.GetPropertyValue<string>("description");

            if (string.IsNullOrWhiteSpace(input))
                return null;

            var words = input
                .StripHtml()
                .StripNewLines()
                .Replace("&nbsp;", " ")
                .Split(new[] { ' ' })
                .Take(count)
                .ToList();

            return string.Concat(string.Join(" ", words), "...");
        }
    }
}