using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Marketplace.BusinessLogic.ListingCreator;


namespace Marketplace.BusinessLogic.ListingCreator.StepDefinitions
{
    public class Files : ListingCreatorStep
    {
        public override string Alias
        {
            get { return "files"; }
        }

        public override string Name
        {
            get { return "Package Files"; }
        }

        public override string UserControl
        {
            get { return "~/usercontrols/deli/package/steps/files.ascx"; }
        }

    }
}