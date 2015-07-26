using System;
using System.Collections.Generic;

namespace OurUmbraco.Release.Models
{
    public class Version
    {
        public bool Released { get; set; }

        public bool Archived { get; set; }

        public bool LatestRelease { get; set; }

        public bool InProgressRelease { get; set; }

        public long ReleaseDate { get; set; }

        public string Description { get; set; }

        public DateTime GetReleaseDate()
        {
            return Extensions.JavaTimeStampToDateTime(ReleaseDate);
        }

        public string Value { get; set; }

        public string ReleaseStatus { get; set; }
    }

    public class CustomField
    {
        public string bundle { get; set; }
    }

    public class VersionBundle
    {
        public string Name { get; set; }

        public List<Version> Versions { get; set; }
    }
}