namespace OurUmbraco.MarketPlace.BusinessLogic.ListingCreator.StepDefinitions
{
    public class Screenshots : ListingCreatorStep
    {
        public override string Alias
        {
            get { return "screenshots"; }
        }

        public override string Name
        {
            get { return "Screenshots"; }
        }

        public override string UserControl
        {
            get { return "~/usercontrols/deli/package/steps/screenshots.ascx"; }
        }
    }
}
