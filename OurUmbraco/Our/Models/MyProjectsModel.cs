using System.Collections.Generic;
using OurUmbraco.MarketPlace.Interfaces;

namespace OurUmbraco.Our.Models
{
    public class MyProjectsModel
    {
        public IEnumerable<IListingItem> LiveProjects { get; set; }
        public IEnumerable<IListingItem> RetiredProjects { get; set; }
        public IEnumerable<IListingItem> DraftProjects { get; set; }
        public IEnumerable<IListingItem> ContribProjects { get; set; }
    }
}