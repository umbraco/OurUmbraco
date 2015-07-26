using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marketplace.BusinessLogic.ListingCreator
{
    public class ListingCreatorStepCollection : Dictionary<String, ListingCreatorStep>
    {
        public void Add(ListingCreatorStep step,bool isDeliVendor)
        {
            step.Index = this.Count;

            if (!step.CommercialOnly)
                this.Add(step.Alias, step);
            else if(step.CommercialOnly && isDeliVendor)
                this.Add(step.Alias, step);
        }

        public ListingCreatorStep Get(string key)
        {
            return this.First(item => item.Key == key).Value;
        }

        public bool StepExists(string key)
        {
            return this.ContainsKey(key);
        }

        public ListingCreatorStep GotoNextStep(string key)
        {
            ListingCreatorStep s = this[key];
            foreach (ListingCreatorStep i in this.Values)
            {

                if (i.Index > s.Index && !i.Completed)
                {
                    return i;
                }
            }

            return null;
        }

        public ListingCreatorStep GotoPreviousStep(string key)
        {
            ListingCreatorStep s = this[key];
            if (s.Index > 0)
            {
                foreach (ListingCreatorStep i in this.Values)
                {

                    if (i.Index == (s.Index - 1))
                    {
                        return i;
                    }
                }
            }

            return null;
        }


        public ListingCreatorStep FirstAvailableStep()
        {
            return this.First(item => item.Value.Completed == false).Value;
        }
    }
}
