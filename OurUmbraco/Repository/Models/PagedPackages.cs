using System.Collections.Generic;

namespace OurUmbraco.Repository.Models
{
    public class PagedPackages
    {
        public IEnumerable<Package> Packages { get; set; }
        public int Total { get; set; }
    }
}