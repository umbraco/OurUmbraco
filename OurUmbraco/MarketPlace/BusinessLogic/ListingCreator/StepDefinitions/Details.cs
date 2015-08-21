namespace OurUmbraco.MarketPlace.BusinessLogic.ListingCreator.StepDefinitions
{
    public class Details : ListingCreatorStep
    {
        public override string Alias
        {
            get { return "details"; }
        }

        public override string Name
        {
            get { return "Package Details"; }
        }

        public override string UserControl
        {
            get { return "~/usercontrols/deli/package/steps/details.ascx"; }
        }
    }
}
