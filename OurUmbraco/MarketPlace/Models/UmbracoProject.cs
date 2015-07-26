using System;
using System.Collections.Generic;

namespace OurUmbraco.MarketPlace.Models
{
    public class UmbracoProject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }
        public List<int> CompatibleVersions { get; set; }
        public int TotalDownloads { get; set; }
        public int DownloadsCurrentVersion { get; set; }
        public int Karma { get; set; }
        public int TotalDownloadsRank { get; set; }
        public int CurrentVersionDownloadsRank { get; set; }
        public int KarmaRank { get; set; }
        public int TotalRank { get; set; }
        public int FileId { get; set; }
        public int ReportsCount { get; set; }
        public int CompatibilityCount { get; set; }
        public float CompatibilityScore { get; set; }
    }
}
