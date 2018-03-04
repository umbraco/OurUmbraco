using Newtonsoft.Json;
using OurUmbraco.Community.People;
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

                //Even with a deletion - we get a blank date sent to us in payload
                var createdDate = SentDate.ToString("D");

                //Return the edited dates
                return $"Sent at {createdDate}";
            }
        }

        [JsonProperty("memberName")]
        public string MemberName
        {
            get
            {
                //When we get a delete message payload
                //It would crash the realtime server as it would not contain a User JSON nested object
                if (User == null)
                    return null;

                if (string.IsNullOrWhiteSpace(User.Username) == false)
                {
                    var peopleService = new PeopleService();
                    var memberId = peopleService.GetMemberIdFromGithubName(User.Username);
                    if (memberId != null)
                        return $"<a href=\"/member/{memberId}\">{User.Username}</a>";
                }

                return User.Username;
            }
        }
    }
}