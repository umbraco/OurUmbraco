using Newtonsoft.Json;
using Message = GitterSharp.Model.Message;

namespace OurUmbraco.Gitter
{
    public class UmbracoMessage : Message
    {

        [JsonProperty("friendlyDate")]
        public string FriendlyDate
        {
            get
            {
                if (EditedDate.HasValue)
                {
                    var editedDate = EditedDate.Value.ToString("D");

                    //Return the edited date
                    return $"Edited at {editedDate}";
                }

                var createdDate = SentDate.ToString("D");

                //Return the edited dates
                return $"Sent at {createdDate}";
            }
        }
    }
}