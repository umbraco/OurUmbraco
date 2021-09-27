using System;
using System.Collections.Generic;

namespace OurUmbraco.Repository.Models
{
    public class Package
    {
        public Package()
        {
            OwnerInfo = new PackageOwnerInfo();
        }

        public DateTime Created { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Excerpt { get; set; }

        public string Icon { get; set; }

        public string Url { get; set; }

        public int Downloads { get; set; }

        public int Likes { get; set; }

        public string Category { get; set; }

        /// <summary>
        /// The latest version of this package
        /// </summary>
        public string LatestVersion { get; set; }

        public PackageOwnerInfo OwnerInfo { get; set; }
        
        public long Score { get; set; }

        /// <summary>
        ///  path to the image, our.umbraco.com - has multiple crops
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        ///  version range (e.g 7.4 - 8.5) for package
        /// </summary>
        public string VersionRange { get; set; }

        /// <summary>
        ///  shorter summary shown on our.umbraco.com
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// A flag indicating whether the package has been certified to work on Umbraco Cloud.
        /// </summary>
        public bool CertifiedToWorkOnUmbracoCloud { get; set; }

        /// <summary>
        /// The package id to identify the exact package on NuGet.org
        /// </summary>
        public string NuGetPackageId { get; set; }

        /// <summary>
        /// Package is in the new NuGet-only format for v9+ sites
        /// </summary>
        public bool IsNuGetFormat { get; set; }
    }
}