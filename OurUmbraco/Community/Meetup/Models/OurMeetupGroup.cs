using System.Collections.Generic;
using Skybrud.Social.Meetup.Models.GraphQl.Groups;

namespace OurUmbraco.Community.Meetup.Models
{
    public class OurMeetupGroup
    {
        public MeetupGroup Group { get; set; }
        public List<Skybrud.Social.Meetup.Models.GraphQl.Events.MeetupEvent> Events { get; set; }
    }
}