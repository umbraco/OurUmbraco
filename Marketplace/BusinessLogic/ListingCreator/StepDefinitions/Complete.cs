using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marketplace.BusinessLogic.ListingCreator;


namespace Marketplace.BusinessLogic.ListingCreator.StepDefinitions
{
    public class Complete : ListingCreatorStep
    {
        public override string Alias
        {
            get { return "complete"; }
        }

        public override string Name
        {
            get { return "Creation Complete"; }
        }

        public override string UserControl
        {
            get { return "~/usercontrols/deli/package/steps/complete.ascx"; }
        }

    }
}