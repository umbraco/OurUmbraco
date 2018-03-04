using Examine;
using Umbraco.Core;
using UmbracoExamine;

namespace OurUmbraco.Community.Map
{
    public class MapEventHandler : ApplicationEventHandler
    {
        private const string hasLocationIndexField = "hasLocationSet";

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Setup Gathering Node Data Examine Event for the Internal Member Index
            //This is so that we can add 'null' to the lat & lon member fields
            //Or add new computed field that is a bool - that indicates that the member has lat/lon info
            //To help filter/exclude members who does not have this property set

            ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"].GatheringNodeData += MapEventHandler_GatheringNodeData;
        }

        private void MapEventHandler_GatheringNodeData(object sender, IndexingNodeDataEventArgs e)
        {
            //Even though the index is the internal members one
            //Lets be certain that the indextype is member
            if (e.IndexType != IndexTypes.Member)
                return;

            var hasLocationSet = false;

            //Check if the fields
            if (e.Fields.ContainsKey("longitude") && e.Fields.ContainsKey("latitude"))
            {
                //Next check its not null/empty value
                if (string.IsNullOrEmpty(e.Fields["longitude"]) == false &&
                    string.IsNullOrEmpty(e.Fields["latitude"]) == false)
                {
                    //We add this field into the index
                    //We can then search for this field - to find all members who has lat & lon set
                    hasLocationSet = true;
                }

            }

            var valueToStore = hasLocationSet ? "1" : "0";  

            //Check if the field exists already or not (to add or update)
            if (e.Fields.ContainsKey(hasLocationIndexField))
            {
                //Update it
                e.Fields[hasLocationIndexField] = valueToStore;
            }
            else
            {
                //Add/push it in
                e.Fields.Add(hasLocationIndexField, valueToStore);
            }

        }
    }
}
