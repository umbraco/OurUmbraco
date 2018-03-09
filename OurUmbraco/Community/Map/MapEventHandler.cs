using Examine;
using Lucene.Net.Documents;
using System;
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

            //Add lower level Document Writing Examine Event - when we are saving/storing the Lucene Document
            //Allows us to add a new field type - but also set its type (which cant be done in GatheringNodeData even)
            var indexer = (UmbracoContentIndexer)ExamineManager.Instance.IndexProviderCollection["InternalMemberIndexer"];
            indexer.DocumentWriting += Indexer_DocumentWriting;
        }

        private void Indexer_DocumentWriting(object sender, Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            SetKarmaAsNumericField(e);
            SetLocationAsDoubleField(e);
        }

        private static void SetKarmaAsNumericField(Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            //Get existing field karma field - some members may not have a value set - so check in case
            var existingField = e.Document.GetField("reputationCurrent");
            if (existingField != null)
            {
                //Add new field that is numeric
                var karmaField = new NumericField("karma", Field.Store.YES, true);

                //Get the existing value that is stored on the vanilla property that stores as strings
                var existingValue = existingField.StringValue();

                //Convert to int
                var valAsInt = Convert.ToInt32(existingValue);

                //Set it on the karma field
                karmaField.SetIntValue(valAsInt);

                //Add the field to the document
                e.Document.Add(karmaField);
            }
        }

        private static void SetLocationAsDoubleField(Examine.LuceneEngine.DocumentWritingEventArgs e)
        {
            //Get existing field - some members may not have a valueset - so check in case
            var existingLatField = e.Document.GetField("latitude");
            var existingLonField = e.Document.GetField("longitude");

            if (existingLatField != null && existingLonField != null)
            {
                var latitude = Convert.ToDouble(existingLatField.StringValue());
                var longitude = Convert.ToDouble(existingLonField.StringValue());

                var latNumnber = new NumericField("latitudeNumber", Field.Store.YES, true).SetDoubleValue(latitude);
                var lonNumnber = new NumericField("longitudeNumber", Field.Store.YES, true).SetDoubleValue(longitude);
                
                e.Document.Add(latNumnber);
                e.Document.Add(lonNumnber);
            }
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
