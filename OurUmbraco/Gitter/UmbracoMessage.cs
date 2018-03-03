using System;
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
                    return string.Format("Edited at {0}", editedDate);
                }

                var createdDate = SentDate.ToString("D");

                //Return the edited dates
                return string.Format("Sent at {0}", createdDate);
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
                        return string.Format("<a href=\"/member/{0}\">{1}</a>", member.Id, User.Username);
                }

                return User.Username;
            }
        }
    }
}