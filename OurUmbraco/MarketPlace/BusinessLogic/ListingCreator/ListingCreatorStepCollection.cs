using System.Collections.Generic;
using System.Linq;

namespace OurUmbraco.MarketPlace.BusinessLogic.ListingCreator
{
    public class ListingCreatorStepCollection : Dictionary<string, ListingCreatorStep>
    {
        public void Add(ListingCreatorStep step, bool isDeliVendor)
        {
            step.Index = this.Count;

            if (!step.CommercialOnly)
                Add(step.Alias, step);
            else if (step.CommercialOnly && isDeliVendor)
                Add(step.Alias, step);
        }

        public ListingCreatorStep Get(string key)
        {
            return this.First(item => item.Key == key).Value;
        }

        public bool StepExists(string key)
        {
            return ContainsKey(key);
        }

        public ListingCreatorStep GotoNextStep(string key)
        {
            var s = this[key];

            foreach (var listingCreatorStep in Values)
            {
                if (listingCreatorStep.Index > s.Index && !listingCreatorStep.Completed)
                    return listingCreatorStep;
            }

            return null;
        }

        public ListingCreatorStep GotoPreviousStep(string key)
        {
            var s = this[key];
            if (s.Index <= 0)
                return null;

            foreach (var listingCreatorStep in Values)
            {
                if (listingCreatorStep.Index == (s.Index - 1))
                    return listingCreatorStep;
            }

            return null;
        }

        public ListingCreatorStep FirstAvailableStep()
        {
            return this.First(item => item.Value.Completed == false).Value;
        }
    }
}
