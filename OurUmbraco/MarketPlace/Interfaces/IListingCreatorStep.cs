namespace OurUmbraco.MarketPlace.Interfaces
{
    public interface IListingCreatorStep
    {
        string Alias { get; }
        bool Completed { get; set; }
        string Name { get; }
        string UserControl { get; }
        bool HideFromNavigation { get; }
    }
}
