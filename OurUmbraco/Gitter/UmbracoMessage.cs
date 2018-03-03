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
                if (string.IsNullOrWhiteSpace(User.Username) == false)
                {
                    var peopleService = new PeopleService();
                    var member = peopleService.GetMemberFromGithubName(User.Username);
                    if (member != null)
                        return $"<a href=\"/member/{member.Id}\">{User.Username}</a>";
                }

                return User.Username;
            }
        }
    }
}