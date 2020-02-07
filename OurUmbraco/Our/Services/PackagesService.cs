using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Persistence;

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

            var packages = _database.Fetch<PackageDownloads>($"SELECT TOP {amountOfRecords} projectId, COUNT(projectId) as downloadCount" +
                                                                                  $" FROM projectDownload" +
                                                                                  $" WHERE DATEPART(m, [timestamp]) = DATEPART(m, '{formattedDateTime}')" +
                                                                                  $" AND DATEPART(yyyy, [timestamp]) = DATEPART(yyyy, '{formattedDateTime}')" +
                                                                                  $" GROUP BY projectId" +
                                                                                  $" ORDER BY downloadCount DESC");

            return packages;
        }

        public IEnumerable<PackageDownloads> GetTopXYearlyPackageDownloads(int amountOfRecords)
        {
            var formattedDateTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            var packages = _database.Fetch<PackageDownloads>($"SELECT TOP {amountOfRecords} projectId, COUNT(projectId) as downloadCount" +
                                                                                                       $" FROM projectDownload" +
                                                                                                       $" WHERE [timestamp] > DATEADD(year, -1, '{formattedDateTime}')" +
                                                                                                       $" GROUP BY projectId" +
                                                                                                       $" ORDER BY downloadCount DESC");

            return packages;
        }

        public class PackageDownloads
        {
            public int ProjectId { get; set; }
            public int DownloadCount { get; set; }
        }
    }
}