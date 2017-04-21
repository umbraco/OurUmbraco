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
    }
}