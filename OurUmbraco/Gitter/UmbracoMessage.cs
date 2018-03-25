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
                var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);

                if (EditedDate.HasValue)
                {
                    return EditedDate.Value.Add(utcOffset).ConvertToRelativeTime();
                }

                //Even with a deletion - we get a blank date sent to us in payload
                return SentDate.Add(utcOffset).ConvertToRelativeTime();
            }
        }

        [JsonProperty("fullFriendlyDate")]
        public string FullDate
        {
            get
            {
                var utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                if (EditedDate.HasValue)
                {
                    var editedDate = string.Format($"{EditedDate.Value.Add(utcOffset):ddd, dd MMM yyyy} {EditedDate.Value.Add(utcOffset):HH:mm:ss} UTC+{utcOffset}");
                    return editedDate;
                }

                var sentDate = string.Format($"{SentDate.Add(utcOffset):ddd, dd MMM yyyy} {SentDate.Add(utcOffset):HH:mm:ss} UTC+{utcOffset}");

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