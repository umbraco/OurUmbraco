using System;

namespace OurUmbraco.Events.Models
{
    public class Event
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int SignedUpCount { get; set; }
        public string OwnerName { get; set; }
        public string Venue { get; set; }
        public int VenueCapacity { get; set; }
        public string VenueLongitude { get; set; }
        public string VenueLatitude { get; set; }
        public string Link { get; set; }
        public bool IsExternal { get; set; }
    }
}
