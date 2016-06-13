using System;
using System.Collections.Generic;

namespace OurUmbraco.Repository.Models
{
    public class PackageDetails : Package
    {
        public PackageDetails()
        {
            Images = new List<PackageImage>();
            Compatibility = new List<PackageCompatibility>();
            ExternalSources = new List<ExternalSource>();
        }

        public string Description { get; set; }

        public List<PackageImage> Images { get; set; }

        public List<PackageCompatibility> Compatibility { get; set; }

        public List<ExternalSource> ExternalSources { get; set; }        


        public string LicenseName { get; set; }

        public string LicenseUrl { get; set; }

        public string NetVersion { get; set; }

        public string ZipUrl { get; set; }
    }
}