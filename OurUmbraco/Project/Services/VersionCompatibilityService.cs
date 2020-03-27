using System;
using System.Collections.Generic;
using System.Linq;
using OurUmbraco.Project.Models;
using OurUmbraco.Project.uVersion;
using Umbraco.Core;

namespace OurUmbraco.Project.Services
{
    public class VersionCompatibilityService
    {
        private readonly DatabaseContext _databaseContext;

        public VersionCompatibilityService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public void UpdateCompatibility(int projectId, int fileId, int memberId, Dictionary<string, bool> compatibilityDictionary)
        {
            foreach (var item in compatibilityDictionary)
            {
                _databaseContext.Database.Insert("DeliVersionCompatibility", "id", new
                {
                    fileId = fileId,
                    memberId = memberId,
                    projectId = projectId,
                    version = item.Key,
                    isCompatible = item.Value,
                    dateStamp = DateTime.Now
                });
            }
        }

        public IEnumerable<VersionCompatibility> GetCompatibilityReport(int projectId)
        {
            var versions = new UVersion();
            var uVersions = versions.GetAllVersions();
            var projectProvider = new OurUmbraco.MarketPlace.NodeListing.NodeListingProvider();
            var project = projectProvider.GetListing(projectId, true);

            var compatList = new List<VersionCompatibility>();

            var projectCompatibilities = _databaseContext.Database.Fetch<dynamic>(
                "SELECT * FROM DeliVersionCompatibility WHERE projectId = @projectId", new {projectId = projectId});

            foreach (var ver in uVersions)
            {
                var ver1 = ver;
                var reports = projectCompatibilities.Where(x => x.version == ver1.Name.Replace("Version ", string.Empty) && x.projectId == project.Id).ToArray();

                if (reports.Any())
                {
                    float compats = reports.Count(x => x.isCompatible);
                    float numReps = reports.Count();
                    var perc = Convert.ToInt32(((compats / numReps) * 100));

                    var smiley = "unhappy";

                    if (perc >= 95)
                    {
                        smiley = "joyous";
                    }
                    else if (perc < 95 && perc >= 80)
                    {
                        smiley = "happy";
                    }
                    else if (perc < 80 && perc >= 65)
                    {
                        smiley = "neutral";
                    }
                    else if (perc < 65 && perc >= 50)
                    {
                        smiley = "unhappy";
                    }
                    else
                    {
                        smiley = "superUnhappy";
                    }



                    compatList.Add(new VersionCompatibility() { Percentage = perc, Smiley = smiley, Version = ver.Name });
                }
                else
                {
                    compatList.Add(new VersionCompatibility() { Percentage = 0, Smiley = "untested", Version = ver.Name });
                }

            }

            return compatList;

        }

        public int GetAllCompatibilityReportsCount()
        {
            var allProjectCompatibilities = _databaseContext.Database.Query<int>("SELECT COUNT(*) FROM DeliVersionCompatibility");
            var count = allProjectCompatibilities.First();
            return count;
        }

        public int GetAllCompatibilityReportsCountByDateRange(DateTime fromDate, DateTime toDate)
        {
            var allProjectCompatibilities = _databaseContext.Database.Query<int>(
                "SELECT COUNT(*) FROM DeliVersionCompatibility WHERE (dateStamp BETWEEN '" + 
                fromDate.ToString("yyyy-MM-dd") + "' AND '" + toDate.ToString("yyyy-MM-dd") + "')");

            var count = allProjectCompatibilities.First();
            return count;
        }
    }
}