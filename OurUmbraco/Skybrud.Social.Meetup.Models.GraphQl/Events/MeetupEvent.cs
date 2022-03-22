using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skybrud.Essentials.Json.Extensions;
using Skybrud.Essentials.Time;
using Skybrud.Social.Meetup.Extensions;
using Skybrud.Social.Meetup.Models.GraphQl.Venues;

namespace Skybrud.Social.Meetup.Models.GraphQl.Events
{
    /// <summary>
    /// Class representing a Meetup.com event.
    /// </summary>
    /// <see>
    ///     <cref>https://www.meetup.com/api/schema/#Event</cref>
    /// </see>
    public class MeetupEvent : MeetupObject
    {
        /// <summary>
        /// Alphanumeric identifier for the event.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Title of the event.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; }

        /// <summary>
        /// Gets the URL of the event.
        /// </summary>
        [JsonProperty("eventUrl")]
        public string EventUrl { get; }

        /// <summary>
        /// Gets the description of the event.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; }

        /// <summary>
        /// Gets the short description of the event.
        /// </summary>
        [JsonProperty("shortDescription")]
        public string ShortDescription { get; }

        /// <summary>
        /// Gets information on how to find the event.
        /// </summary>
        [JsonProperty("howToFindUs")]
        public string HowToFindUs { get; }

        /// <summary>
        /// Gets a reference to the venue of the event.
        /// </summary>
        [JsonProperty("venue")]
        public MeetupVenue Venue { get; }

        /// <summary>
        /// Gets the status of the event.
        /// </summary>
        [JsonProperty("status")]
        public MeetupEventStatus? Status { get; }

        /// <summary>
        /// Gets the time status of the event.
        /// </summary>
        [JsonProperty("status")]
        public MeetupEventTimeStatus? TimeStatus { get; }

        /// <summary>
        /// Gets a timestamp for when the event starts.
        /// </summary>
        [JsonProperty("dateTime")]
        public EssentialsTime DateTime { get; }

        /// <summary>
        /// Gets the duration of the event.
        /// </summary>
        [JsonProperty("duration")]
        public TimeSpan? Duration { get; }

        /// <summary>
        /// Gets the time zone of the event.
        /// </summary>
        [JsonProperty("timezone")]
        public string TimeZone { get; }

        /// <summary>
        /// Gets a timestamp for when the event ends.
        /// </summary>
        [JsonProperty("endTime")]
        public EssentialsTime EndTime { get; }

        /// <summary>
        /// Gets a timestamp for when the event was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public EssentialsTime CreatedAt { get; }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        [JsonProperty("eventType")]
        public MeetupEventType? EventType { get; }

        /// <summary>
        /// Gets a shortened link for the event.
        /// </summary>
        [JsonProperty("shortUrl")]
        public string ShortUrl { get; }

        /// <summary>
        /// Gets whether the event is happening online.
        /// </summary>
        [JsonProperty("isOnline")]
        public bool IsOnline { get; }


        private MeetupEvent(JObject json) : base(json)
        {
            Id = json.GetString("id");
            Title = json.GetString("title");
            EventUrl = json.GetString("eventUrl");
            Description = json.GetString("description");
            ShortDescription = json.GetString("shortDescription");
            HowToFindUs = json.GetString("howToFindUs");
            Venue = json.GetObject("venue", MeetupVenue.Parse);
            Status = json.GetEnumNullable<MeetupEventStatus>("status");
            TimeStatus = json.GetEnumNullable<MeetupEventTimeStatus>("timeStatus");
            try
            {
                DateTime = json.GetEssentialsTime("dateTime");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed parsing 'dateTime': " + json.GetString("dateTime"), ex);
            }

            Duration = json.GetTimeSpanNullable("duration");
            TimeZone = json.GetString("timezone");
            try
            {
                EndTime = json.GetEssentialsTime("endTime");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed parsing 'endTime': " + json.GetString("endTime"), ex);
            }

            try
            {
                CreatedAt = json.GetEssentialsTime("createdAt");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed parsing 'createdAt': " + json.GetString("createdAt"), ex);
            }

            EventType = json.GetEnumNullable<MeetupEventType>("eventType");
            ShortUrl = json.GetString("shortUrl");
            IsOnline = json.GetBoolean("isOnline");
        }


        /// <summary>
        /// Parses the specified <paramref name="json"/> object into an instance of <see cref="MeetupEvent"/>.
        /// </summary>
        /// <param name="json">The instance of <see cref="JObject"/> to be parsed.</param>
        /// <returns>An instance of <see cref="MeetupEvent"/>.</returns>
        public static MeetupEvent Parse(JObject json)
        {
            return json == null ? null : new MeetupEvent(json);
        }
    }
}