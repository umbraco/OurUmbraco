using Skybrud.Essentials.Locations;
using Skybrud.Social.Meetup.Models.Events;
using Skybrud.Social.Meetup.Models.Groups;

namespace OurUmbraco.Community.Models {
    
    public class MeetupItem {

        public MeetupGroup Group { get; set; }

        public MeetupEvent Event { get; set; }

        public ILocation Location {
            get { return Event.HasVenue ? (ILocation) Event.Venue : Group; }
        }

        public MeetupItem(MeetupGroup group, MeetupEvent ev) {
            Group = group;
            Event = ev;
        }

    }

}