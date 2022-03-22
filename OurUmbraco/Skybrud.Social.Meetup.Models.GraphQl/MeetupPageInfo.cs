using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;

namespace Skybrud.Social.Meetup.Models.GraphQl
{
    /// <summary>
    /// Information about pagination in a connection
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#PageInfo</cref>
    /// </see>
    public class MeetupPageInfo : MeetupObject
    {
        /// <summary>
        /// A boolean value that indicates whether the end of the list was reached (only relevant when paginating forward through a list).
        /// </summary>
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; }

        /// <summary>
        /// A boolean value that indicates whether the beginning of the list was reached (only relevant when paginating backwards through a list).
        /// </summary>
        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage { get; }

        /// <summary>
        /// A cursor that represents the first edge in the list of edges for this query.
        /// </summary>
        [JsonProperty("startCursor")]
        public string StartCursor { get; }

        /// <summary>
        /// A cursor that represents the last edge in the list of edges for this query.
        /// </summary>
        [JsonProperty("endCursor")]
        public string EndCursor { get; }


        private MeetupPageInfo(JObject json) : base(json)
        {
            HasNextPage = json.GetBoolean("hasNextPage");
            HasPreviousPage = json.GetBoolean("hasPreviousPage");
            StartCursor = json.GetString("startCursor");
            EndCursor = json.GetString("endCursor");
        }


        /// <summary>
        /// Parses the specified <paramref name="json"/> object into an instance of <see cref="MeetupPageInfo"/>.
        /// </summary>
        /// <param name="json">The instance of <see cref="JObject"/> to be parsed.</param>
        /// <returns>An instance of <see cref="MeetupPageInfo"/>.</returns>
        public static MeetupPageInfo Parse(JObject json)
        {
            return json == null ? null : new MeetupPageInfo(json);
        }
    }
}