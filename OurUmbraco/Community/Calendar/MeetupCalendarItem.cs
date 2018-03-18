using OurUmbraco.Community.Models;

namespace OurUmbraco.Community.Calendar
{

    public class MeetupCalendarItem : CommunityCalendarItem
    {

        public MeetupItem Meetup { get; private set; }

        public MeetupCalendarItem(MeetupItem meetup)
        {

            Meetup = meetup;
            Type = CommunityCalendarItemType.Meetup;
            StartDate = Meetup.Event.Time;
            Title = Meetup.Event.Name;
            Description = Meetup.Event.Description;

            SubTitle = Meetup.Group.Name;

            if (Meetup.Event.HasVenue)
            {
                LocationText = Meetup.Event.Venue.City + ", " + Meetup.Event.Venue.LocalizedCountryName;
                Location = Meetup.Location;
            }

            Url = Meetup.Event.Link;

        }

    }

}