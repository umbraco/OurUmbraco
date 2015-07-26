using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marketplace.BusinessLogic.ListingCreator;


namespace Marketplace.BusinessLogic.ListingCreator.StepDefinitions
{
    public class Licenses : ListingCreatorStep
    {
        public override string Alias
        {
            get { return "license"; }
        }

        public override string Name
        {
            get { return "License Details"; }
        }

        public override string UserControl
        {
            get { return "~/usercontrols/deli/package/steps/Licenses.ascx"; }
        }

        public override bool CommercialOnly
        {
            get { return true; }
        }
    }
}