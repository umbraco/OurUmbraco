using OurUmbraco.MarketPlace.Interfaces;

namespace OurUmbraco.MarketPlace.Providers
{
    public class Karma : IKarma
    {
        public int ProjectId { get; set; }
        public int Points { get; set; }
    }
}
