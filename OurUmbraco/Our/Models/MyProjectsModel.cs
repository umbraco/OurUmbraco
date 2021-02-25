using System.Collections.Generic;
using OurUmbraco.MarketPlace.Interfaces;

namespace OurUmbraco.Our.Models
{
    public class MyProjectsModel
    {
        public IEnumerable<IListingItem> MyLiveProjects { get; set; }
        public IEnumerable<IListingItem> MyRetiredProjects { get; set; }
        public IEnumerable<IListingItem> MyDraftProjects { get; set; }
        public IEnumerable<IListingItem> ContribLiveProjects { get; set; }
        public IEnumerable<IListingItem> ContribRetiredProjects { get; set; }
        public IEnumerable<IListingItem> ContribDraftProjects { get; set; }
    }
}