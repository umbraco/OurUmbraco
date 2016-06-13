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

        public PackageDetails(Package package)
            : base()
        {
            this.Category = package.Category;
            this.Excerpt = package.Excerpt;
            this.Downloads = package.Downloads;
            this.Id = package.Id;
            this.Likes = package.Likes;
            this.Name = package.Name;
            this.Icon = package.Icon;
            this.LatestVersion = package.LatestVersion;
            this.MinimumVersion = package.MinimumVersion;
            this.OwnerInfo = package.OwnerInfo;
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