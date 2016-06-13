using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string LatestVersion { get; set; }
        public PackageOwnerInfo OwnerInfo { get; set; }

        /// <summary>
        /// This is the minimum Umbraco version that this package supports
        /// </summary>
        /// <remarks>
        /// This could be null if it is a legacy package
        /// </remarks>
        public string MinimumVersion { get; set; }
    }
}
