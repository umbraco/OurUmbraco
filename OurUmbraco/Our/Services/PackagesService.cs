using System;
using System.Collections.Generic;
using System.Linq;
using OurUmbraco.Repository.Services;
using OurUmbraco.Wiki.BusinessLogic;
using OurUmbraco.Wiki.Extensions;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Security;

namespace OurUmbraco.Our.Services
{
    public class PackagesService
    {
        private readonly UmbracoDatabase _database;

        public PackagesService(UmbracoDatabase database)
        {
            _database = database;
        }

        public PackagesService() : this(ApplicationContext.Current.DatabaseContext.Database)
        {
        }

        public IEnumerable<PackageDownloads> GetTopXMonthlyPackageDownloads(DateTime dateTime, int amountOfRecords)
        {
            var formattedDateTime = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");

            var packages = ApplicationContext.Current.DatabaseContext.Database.Fetch<PackageDownloads>($"SELECT TOP {amountOfRecords} projectId, COUNT(projectId) as downloadCount" +
                                                                                  $" FROM projectDownload" +
                                                                                  $" WHERE DATEPART(m, [timestamp]) = DATEPART(m, '{formattedDateTime}')" +
                                                                                  $" AND DATEPART(yyyy, [timestamp]) = DATEPART(yyyy, '{formattedDateTime}')" +
                                                                                  $" GROUP BY projectId" +
                                                                                  $" ORDER BY downloadCount DESC");

            return packages;
        }

        public class PackageDownloads
        {
            public int ProjectId { get; set; }
            public int DownloadCount { get; set; }
        }

        public List<Package> GetPackageStatisticsData()
        {
            var packages = new List<Package>();

            var umbracoContext = UmbracoContext.Current;
            var prs = new PackageRepositoryService(new UmbracoHelper(umbracoContext), new MembershipHelper(umbracoContext), UmbracoContext.Current.Application.DatabaseContext);
            var allPackages = prs.GetPackages(0, 2000);
            var allPackagesPackages = allPackages.Packages;

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var allPackagesContent= umbracoHelper.TypedContentAtXPath("//Project").ToList();

            foreach (var package in allPackagesPackages)
            {
                var packageContent = allPackagesContent.FirstOrDefault(x => 
                    string.Equals(x.GetPropertyValue<string>("packageGuid"), package.Id.ToString(), StringComparison.InvariantCultureIgnoreCase));

                var pkgFiles = new List<WikiFile>();
                if (packageContent != null)
                    pkgFiles = WikiFile.CurrentFiles(packageContent.Id).OrderByDescending(x => x.CreateDate).ToList();

                var statisticsPackage = new Package
                {
                    Name = package.Name,
                    Owner = package.OwnerInfo.Owner,
                    Created = package.Created.ToString("yyyy-MM-dd"),
                    Updated = (pkgFiles.Any() ? pkgFiles.First().CreateDate : DateTime.MinValue).ToString("yyyy-MM-dd"),
                    Version = package.LatestVersion.TrimStart('v', 'V'),
                    Compatibility = pkgFiles.Any() ? pkgFiles.First().Version.Version : string.Empty,
                    Compatibility2 = pkgFiles.Any() ? pkgFiles.First().Versions.ToVersionString().Replace(" ", string.Empty) : string.Empty,
                    License = packageContent?.GetPropertyValue<string>("licenseName") ?? string.Empty,
                    HasDocumentation = pkgFiles.Any(x => x.FileType == "docs"),
                    Votes = package.Likes,
                    Downloads = package.Downloads
                };

                packages.Add(statisticsPackage);
            }
            

            return packages;
        }

        public class Package
        {
            public string Name { get; set; }
            public string Owner { get; set; }
            public string Created { get; set; }
            public string Updated { get; set; }
            public string Version { get; set; }
            public string Compatibility { get; set; }
            public string License { get; set; }
            public bool HasDocumentation { get; set; }
            public int Votes { get; set; }
            public int Downloads { get; set; }
            public string Compatibility2 { get; set; }
        }
    }
}