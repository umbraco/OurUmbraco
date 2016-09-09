using System.Collections.Generic;
using OurUmbraco.MarketPlace.Interfaces;

namespace OurUmbraco.Our.Models
{
    public class MyProjectsModel
    {
        public IEnumerable<IListingItem> Projects { get; set; }
        public IEnumerable<IListingItem> ContribProjects { get; set; }
    }
}