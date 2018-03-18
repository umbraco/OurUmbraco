using System.Collections.Generic;
using System.Linq;
using OurUmbraco.Community.Meetup;
using Skybrud.Essentials.Enums;
using Skybrud.Essentials.Locations;
using Skybrud.Essentials.Time;

namespace OurUmbraco.Community.Calendar
{

    public class CommunityCalendarService
    {

        public CommunityCalendarItem[] GetUpcomingItems()
        {
            return GetAllItems().Where(x => x.StartDate >= EssentialsDateTime.Today).ToArray();
        }

        public CommunityCalendarItem[] GetItemsByYear(int year)
        {
            return GetAllItems().Where(x => x.StartDate.Year == year).ToArray();
        }

        public CommunityCalendarItemType[] GetTypes()
        {
            return EnumUtils.GetEnumValues<CommunityCalendarItemType>();
        }

        private IEnumerable<CommunityCalendarItem> GetAllItems()
        {

            // TODO: Implement caching
                // TODO: Rebuild or invalidate when meetups are updated
                // TODO: Rebuild or invalidate when children are created or updated in the content tree

            var meetupService = new MeetupService();

            // TODO: Should we store past meetups so they still show up in the calendar?


            List<CommunityCalendarItem> items = new List<CommunityCalendarItem>();

            items.Add(new CommunityCalendarItem
            {
                Title = "Training",
                SubTitle = "Umbraco Down Under Festival",
                StartDate = new EssentialsDateTime(2018, 2, 20),
                EndDate = new EssentialsDateTime(2018, 2, 21, 23, 59, 59),
                LocationText = "City of Gold Coast, Australia",
                Url = "http://uduf.net/"
            });

            items.Add(new CommunityCalendarItem
            {
                Title = "Hackathon",
                SubTitle = "Umbraco Down Under Festival",
                StartDate = new EssentialsDateTime(2018, 2, 22),
                LocationText = "City of Gold Coast, Australia",
                Url = "http://uduf.net/"
            });

            items.Add(new CommunityCalendarItem(CommunityCalendarItemType.Party)
            {
                Title = "Pre-party",
                SubTitle = "Umbraco Down Under Festival",
                StartDate = new EssentialsDateTime(2018, 2, 22),
                LocationText = "City of Gold Coast, Australia",
                Url = "http://uduf.net/"
            });

            items.Add(new FestivalCalendarItem
            {
                Title = "Umbraco Down Under Festival",
                StartDate = new EssentialsDateTime(2018, 2, 23),
                LocationText = "City of Gold Coast, Australia",
                Url = "http://uduf.net/"
            });

            items.Add(new FestivalCalendarItem
            {
                Title = "Umbraco Festival Deutschland",
                StartDate = new EssentialsDateTime(2018, 4, 27),
                LocationText = "Frankfurt am Main, Germany",
                Url = "http://umbracofestival.de/"
            });

            items.Add(new FestivalCalendarItem
            {
                Title = "Codegarden",
                StartDate = new EssentialsDateTime(2018, 5, 23),
                EndDate = new EssentialsDateTime(2018, 5, 25),
                LocationText = "Odense, Denmark",
                Location = new EssentialsLocation(55.411192, 10.383015),
                Description = "<p>Join us for our annual gathering - the biggest gathering - of the global Umbraco community in Odense, Denmark. At this year's Codegarden we will spoil you with a plethora of high-quality sessions from community experts, the Core team and acclaimed keynote speakers, announcements from Umbraco HQ, in-depth workshops, an exclusive masterclass and the opportunity to influence the future of Umbraco. We hope to see you there.</p>",
                Url = "https://codegarden18.com/"
            });

            items.AddRange(meetupService.GetUpcomingMeetups().Items.Select(x => new MeetupCalendarItem(x)));
            
            return items.OrderBy(x => x.StartDate);

        }

    }

}