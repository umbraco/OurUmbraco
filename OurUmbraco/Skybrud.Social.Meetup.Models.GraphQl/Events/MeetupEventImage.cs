using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;

namespace Skybrud.Social.Meetup.Models.GraphQl.Events
{
    /// <summary>
    /// Class representing an image of an event.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#EventImage</cref>
    /// </see>
    public class MeetupEventImage : MeetupObject
    {
        /// <summary>
        /// Gets the ID of the image.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the base url of the image
        /// </summary>
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; }


        private MeetupEventImage(JObject json) : base(json)
        {
            Id = json.GetString("id");
            BaseUrl = json.GetString("baseUrl");
        }


        /// <summary>
        /// Parses the specified <paramref name="json"/> object into an instance of <see cref="MeetupEventImage"/>.
        /// </summary>
        /// <param name="json">The instance of <see cref="JObject"/> to be parsed.</param>
        /// <returns>An instance of <see cref="MeetupEventImage"/>.</returns>
        public static MeetupEventImage Parse(JObject json)
        {
            return json == null ? null : new MeetupEventImage(json);
        }
    }
}