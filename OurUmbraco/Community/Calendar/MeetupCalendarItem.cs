using OurUmbraco.Community.Models;
using OurUmbraco.Community.People;
using Skybrud.Essentials.Security;

namespace OurUmbraco.Community.Calendar
{
    public class MeetupCalendarItem : CommunityCalendarItem
    {
        public MeetupItem Meetup { get; }

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

            var avatarService = new AvatarService();
            string img;

            if (string.IsNullOrEmpty(Meetup?.Group?.GroupPhoto?.ThumbLink))
            {
                var fakeHash = SecurityUtils.GetMd5Hash(Meetup.Group.Id + "");

                var avatarPath = avatarService.GetFakeAvatar(fakeHash);
                img = avatarService.GetImgWithSrcSet(avatarPath, Meetup.Group.Name, 30);
            }
            else
            {
                img = $"<img src=\"{avatarService.GetRemoteAvatarUrl(Meetup.Group.GroupPhoto.ThumbLink, 30)}\" />";
            }

            Icon = img;
        }
    }
}