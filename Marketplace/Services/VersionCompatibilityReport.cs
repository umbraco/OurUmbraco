using System;
using System.Collections.Generic;
using System.Linq;
using Marketplace.Interfaces;
using Marketplace.Providers;
using uProject.Models;
using uProject.uVersion;

namespace uProject.Services
{
    public class VersionCompatibilityReport
    {
        private readonly int _pid;

        public VersionCompatibilityReport(int pid)
        {
            _pid = pid;
        }

        public IEnumerable<VersionCompatibility> GetCompatibilityReport()
        {
            var uVersions = UVersion.GetAllVersions();
            var projectProvider = (IListingProvider)MarketplaceProviderManager.Providers["ListingProvider"];
            var project = projectProvider.GetListing(_pid, false);

            var compatList = new List<VersionCompatibility>();
            using (Marketplace.Data.MarketplaceDataContext ctx = new Marketplace.Data.MarketplaceDataContext())
            {

                foreach (var ver in uVersions)
                {
                    var ver1 = ver;
                    var reports = ctx.DeliVersionCompatibilities.Where(x => x.version == ver1.Name && x.projectId == project.Id);

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
            }
            return compatList;

        }
    }
}