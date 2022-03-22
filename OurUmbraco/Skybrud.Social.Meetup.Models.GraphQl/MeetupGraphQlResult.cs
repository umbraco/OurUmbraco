using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Social.Meetup.Models.GraphQl.Groups;

namespace Skybrud.Social.Meetup.Models.GraphQl
{
    public class MeetupGraphQlGroupResult : MeetupObject
    {
        public MeetupGraphQlGroupData Data { get; }

        private MeetupGraphQlGroupResult(JObject json) : base(json)
        {
            Data = json.GetObject("data", MeetupGraphQlGroupData.Parse);
        }


        /// <summary>
        /// Parses the specified <paramref name="json"/> object into an instance of <see cref="MeetupGraphQlGroupResult"/>.
        /// </summary>
        /// <param name="json">The instance of <see cref="JObject"/> to be parsed.</param>
        /// <returns>An instance of <see cref="MeetupGraphQlGroupResult"/>.</returns>
        public static MeetupGraphQlGroupResult Parse(JObject json)
        {
            return json == null ? null : new MeetupGraphQlGroupResult(json);
        }
    }

    public class MeetupGraphQlGroupData : MeetupObject
    {
        public MeetupGroup GroupByUrlname { get; }


        private MeetupGraphQlGroupData(JObject json) : base(json)
        {
            GroupByUrlname = json.GetObject("groupByUrlname", MeetupGroup.Parse);
        }


        /// <summary>
        /// Parses the specified <paramref name="json"/> object into an instance of <see cref="MeetupGraphQlGroupData"/>.
        /// </summary>
        /// <param name="json">The instance of <see cref="JObject"/> to be parsed.</param>
        /// <returns>An instance of <see cref="MeetupGraphQlGroupData"/>.</returns>
        public static MeetupGraphQlGroupData Parse(JObject json)
        {
            return json == null ? null : new MeetupGraphQlGroupData(json);
        }
    }
}