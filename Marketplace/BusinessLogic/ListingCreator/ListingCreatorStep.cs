using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marketplace.Interfaces;

namespace Marketplace.BusinessLogic.ListingCreator
{
    public abstract class ListingCreatorStep : IListingCreatorStep
    {
        public abstract string Alias { get; }
        public abstract string Name { get; }
        public abstract string UserControl { get; }
        public virtual int Index { get; set; }

        public virtual bool MoveToNextStepAutomaticly { get; set; }

        public virtual bool HideFromNavigation
        {
            get
            {
                return false;
            }
        }
        public virtual bool CommercialOnly
        {
            get
            {
                return false;
            }
        }

        public virtual bool Completed { get; set; }

    }
}
