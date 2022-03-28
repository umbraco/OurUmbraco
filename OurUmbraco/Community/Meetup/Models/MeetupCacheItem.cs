using Skybrud.Essentials.Time;
using Skybrud.Social.Meetup.Models.Events;

namespace OurUmbraco.Community.Meetup.Models
{
    public class MeetupCacheItem
    {
        public EssentialsTime Created { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public EssentialsTime Time { get; set; }
        public EssentialsTime Updated { get; set; }
        public bool HasVenue { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public MeetupEventVisibility Visibility { get; set; }
    }
}
