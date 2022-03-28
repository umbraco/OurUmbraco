using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Social.Meetup.Extensions;
using Skybrud.Social.Meetup.Models.GraphQl.Events;
using Skybrud.Social.Meetup.Models.GraphQl.Images;

namespace Skybrud.Social.Meetup.Models.GraphQl.Groups
{
    /// <summary>
    /// Class representing a Meetup.com group.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#Group</cref>
    /// </see>
    public class MeetupGroup : MeetupObject
    {
        /// <summary>
        /// Gets the ID of the group.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Gets the display name of the group. Can be at most 60 characters.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; }

        /// <summary>
        /// Gets a reference to the logo of the group.
        /// </summary>
        public MeetupImage Logo { get; }

        /// <summary>
        /// Gets the latitude of the group location.
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude { get; }

        /// <summary>
        /// Gets longtitude of the group location.
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude { get; }

        /// <summary>
        /// Gets the description of the group.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; }

        /// <summary>
        /// Gets the urlname used to identify the group on meetup.com.
        /// </summary>
        [JsonProperty("urlname")]
        public string UrlName { get; }

        /// <summary>
        /// Gets the urlname used to identify the group on meetup.com.
        /// </summary>
        [JsonProperty("timezone")]
        public string TimeZone { get; }

        /// <summary>
        /// Gets the city of the group.
        /// </summary>
        [JsonProperty("city")]
        public string City { get; }

        /// <summary>
        /// Gets the state of the group, if in US or Canada.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; }

        /// <summary>
        /// Gets the ISO_3166-1 like country code for the country which contains the city (lower case code).
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; }

        /// <summary>
        /// Gets the zip code.
        /// </summary>
        [JsonProperty("zip")]
        public string Zip { get; }

        /// <summary>
        /// Gets a reference to the group photo of the group.
        /// </summary>
        public MeetupImage GroupPhoto { get; }

        /// <summary>
        /// Gets the link for the group on meetup.com.
        /// </summary>
        [JsonProperty("link")]
        public string Link { get; }

        /// <summary>
        /// Gets the way a new member let into the group.
        /// </summary>
        [JsonProperty("joinMode")]
        public MeetupGroupJoinMode? JoinMode { get; }

        /// <summary>
        /// Gets the welcome message to new members.
        /// </summary>
        [JsonProperty("welcomeBlurb")]
        public string WelcomeBlurb { get; }

        /// <summary>
        /// Gets a list of upcoming events for the group.
        /// </summary>
        [JsonProperty("upcomingEvents")]
        public UpcomingEventsConnection UpcomingEvents { get; }


        private MeetupGroup(JObject json) : base(json)
        {
            Id = json.GetString("id");
            Name = json.GetString("name");
            Logo = json.GetObject("logo", MeetupImage.Parse);
            Latitude = json.GetDouble("latitude");
            Longitude = json.GetDouble("latitude");
            Description = json.GetString("description");
            UrlName = json.GetString("urlname");
            TimeZone = json.GetString("timezone");
            City = json.GetString("city");
            State = json.GetString("state");
            Country = json.GetString("country");
            Zip = json.GetString("zip");
            GroupPhoto = json.GetObject("groupPhoto", MeetupImage.Parse);
            Link = json.GetString("link");
            JoinMode = json.GetEnumNullable<MeetupGroupJoinMode>("joinMode");
            WelcomeBlurb = json.GetString("welcomeBlurb");
            UpcomingEvents = json.GetObject("upcomingEvents", UpcomingEventsConnection.Parse);
        }


        /// <summary>
        /// Parses the specified <paramref name="json"/> object into an instance of <see cref="MeetupGroup"/>.
        /// </summary>
        /// <param name="json">The instance of <see cref="JObject"/> to be parsed.</param>
        /// <returns>An instance of <see cref="MeetupGroup"/>.</returns>
        public static MeetupGroup Parse(JObject json)
        {
            return json == null ? null : new MeetupGroup(json);
        }
    }
}