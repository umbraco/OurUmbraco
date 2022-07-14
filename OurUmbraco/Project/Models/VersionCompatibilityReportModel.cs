using System.Collections.Generic;
using OurUmbraco.Project.uVersion;

namespace OurUmbraco.Project.Models
{
    public class VersionCompatibilityReportModel
    {
        public IEnumerable<VersionCompatibility> VersionCompatibilities { get; set; }
        public bool CurrentMemberIsLoggedIn { get; set; }
        public bool CurrentMemberHasDownloaded { get; set; }
        public int FileId { get; set; }
        public int ProjectId { get; set; }
        public IEnumerable<UVersion> AllVersions { get; set; }
        
        public IEnumerable<UVersion> AlsoWorksOnVersions { get; set; }
        public bool WorksOnUaaS { get; set; }
    }
}