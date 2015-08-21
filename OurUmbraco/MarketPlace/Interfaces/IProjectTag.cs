using System;

namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IProjectTag : IEquatable<IProjectTag>
    {
        int Id { get; set; }
        string Text { get; set; }
        int Count { get; set; }
        int LiveCount { get; set; }
        bool Equals(IProjectTag other);
    }
}
