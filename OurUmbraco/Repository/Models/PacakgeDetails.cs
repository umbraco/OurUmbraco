using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OurUmbraco.Repository.Models
{
   
    public class PacakgeDetails : Package
    {
        public PacakgeDetails()
        {
            Images = new List<string>();
            OwnerInfo = new PackageOwnerInfo();
            Compatibility = new List<PackageCompatibility>();
        }
        public string Summary { get; set; }
        public List<string> Images { get; set; }
        public List<PackageCompatibility> Compatibility { get; set; }
        public DateTime Created { get; set; }
        public string LicenseName { get; set; }
        public string LicenseUrl { get; set; }
        public string DotNetVersion { get; set; }
        public PackageOwnerInfo OwnerInfo { get; set; }
        public string ZipUrl { get; set; }
    }
}