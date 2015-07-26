using System;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface ICategory
    {
        int Id { get; set; }
        Guid CategoryGuid { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Image { get; set; }
        bool HQOnly { get; set; }
        int ProjectCount { get; set; }
        string Url { get; set; }
    }
}
