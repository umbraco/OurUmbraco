using System;
using Newtonsoft.Json;
using OurUmbraco.Community.People;
using OurUmbraco.Forum.Extensions;
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
                    return EditedDate.Value.ConvertToRelativeTime();
                }

                //Even with a deletion - we get a blank date sent to us in payload
                return SentDate.ConvertToRelativeTime();
            }
        }

        [JsonProperty("fullFriendlyDate")]
        public string FullDate
        {
            get
            {
                if (EditedDate.HasValue)
                {
                    var editedDate = string.Format("{0:ddd, dd MMM yyyy} {0:HH:mm:ss} UTC", EditedDate.Value);
                    return editedDate;
                }

                var sentDate = string.Format("{0:ddd, dd MMM yyyy} {0:HH:mm:ss} UTC", SentDate);

                //Even with a deletion - we get a blank date sent to us in payload
                return sentDate;
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