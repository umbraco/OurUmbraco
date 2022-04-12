using System.Collections.Generic;
using Skybrud.Essentials.Time;

namespace OurUmbraco.Community.Meetup.Models
{
    public class OurMeetupGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LogoBaseUrl { get; set; }
        public List<OurMeetupEvent> Events { get; set; }
    }

    public class OurMeetupEvent
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public EssentialsTime DateTime { get; set; }
        public string VenueName { get; set; }
    }
}