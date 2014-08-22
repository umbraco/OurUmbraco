using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using uEvents.Meetup;
using umbraco;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.relation;
using System.Web.Security;
using System.Web;
using umbraco.NodeFactory;

namespace uEvents.Library
{
    public class Rest
    {
        public static string Toggle(int eventId)
        {
            Event e = new Event(eventId);
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                if (!Relation.IsRelated(eventId, _currentMember))
                    e.SignUp(_currentMember, "no comment");
                else
                    e.Cancel(_currentMember, "no comment");
            }

            return "true";
        }

        public static string Sync(int eventId)
        {
            Event e = new Event(eventId);
            e.syncCapacity();
            return "true";
        }

        public static string SignUp(int eventId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                Event e = new Event(eventId);
                e.SignUp(_currentMember, "no comment");
            }
            return "true";
        }

        public static string Cancel(int eventId)
        {
            int _currentMember = HttpContext.Current.User.Identity.IsAuthenticated ? (int)Membership.GetUser().ProviderUserKey : 0;

            if (_currentMember > 0)
            {
                Event e = new Event(eventId);
                e.Cancel(_currentMember, "no comment");
            }
            return "true";
        }

        public static XPathNodeIterator UpcomingEvents()
        {
            object eventCacheSyncLock = new object();
            return Cache.GetCacheItem<XPathNodeIterator>("ourEvents", eventCacheSyncLock, new TimeSpan(1, 0, 0),
                delegate
                {
                    return Rest.UpcomingEvents();
                });
        }

        public static List<Models.Event> UpcomingEvents(int parentNode)
        {
            var events = new List<Models.Event>();
            var eventOverview = new Node(parentNode);

            foreach (var publishedEvent in eventOverview.ChildrenAsList.Where(e => e.NodeTypeAlias == "Event"))
            {
                var start = publishedEvent.GetProperty("start").Value;
                DateTime startDateTime;
                if (DateTime.TryParse(start, out startDateTime) == false)
                    continue;
                if (startDateTime.Date < DateTime.Today)
                    continue;

                var end = publishedEvent.GetProperty("end").Value;
                DateTime endDateTime;
                DateTime.TryParse(end, out endDateTime);

                var capacity = publishedEvent.GetProperty("capacity").Value;
                int venueCapacity;
                int.TryParse(capacity, out venueCapacity);

                var signedUp = publishedEvent.GetProperty("signedup").Value;
                int signedUpCount;
                int.TryParse(signedUp, out signedUpCount);

                var owner = publishedEvent.GetProperty("owner").Value;
                int ownerId;
                var ownerName = string.Empty;
                if (int.TryParse(owner, out ownerId))
                {
                    var member = Member.GetMemberFromCache(ownerId);
                    if (member != null)
                        ownerName = member.Text;
                }

                var ev = new Models.Event
                {
                    Id = publishedEvent.Id.ToString(CultureInfo.InvariantCulture),
                    Name = publishedEvent.Name,
                    Description = publishedEvent.GetProperty("description").Value,
                    StartDateTime = startDateTime,
                    EndDateTime = endDateTime,
                    Link = library.NiceUrl(int.Parse(publishedEvent.Id.ToString(CultureInfo.InvariantCulture))),
                    Venue = publishedEvent.GetProperty("venue").Value,
                    VenueLongitude = publishedEvent.GetProperty("longitude").Value,
                    VenueLatitude = publishedEvent.GetProperty("latitude").Value,
                    VenueCapacity = venueCapacity,
                    SignedUpCount = signedUpCount,
                    OwnerName = ownerName
                };

                events.Add(ev);
            }

            var meetupController = new MeetupController();
            var meetups = meetupController.GetAllFromFile();
            if (meetups != null)
            {
                foreach (var meetupEventSearchResult in meetups.Results.Where(m => m.Time.Date >= DateTime.Now.Date))
                {
                    var venueNameAddress = string.Empty;
                    if (meetupEventSearchResult.Venue != null)
                    {
                        venueNameAddress = meetupEventSearchResult.Venue.Name;
                        if (string.IsNullOrWhiteSpace(meetupEventSearchResult.Venue.Address_1) == false)
                            venueNameAddress = venueNameAddress + ", " + meetupEventSearchResult.Venue.Address_1;
                        if (string.IsNullOrWhiteSpace(meetupEventSearchResult.Venue.Zip) == false)
                            venueNameAddress = venueNameAddress + ", " + meetupEventSearchResult.Venue.Zip;
                        if (string.IsNullOrWhiteSpace(meetupEventSearchResult.Venue.City) == false)
                            venueNameAddress = venueNameAddress + ", " + meetupEventSearchResult.Venue.City;
                        if (string.IsNullOrWhiteSpace(meetupEventSearchResult.Venue.Country) == false)
                            venueNameAddress = venueNameAddress + ", " + meetupEventSearchResult.Venue.Country;
                    }

                    var ev = new Models.Event();

                    ev.Id = meetupEventSearchResult.Id;
                    ev.Name = meetupEventSearchResult.Name ?? string.Empty;
                    ev.Description = meetupEventSearchResult.Description ?? string.Empty;
                    ev.StartDateTime = meetupEventSearchResult.Time;
                    ev.EndDateTime = meetupEventSearchResult.Time;
                    ev.IsExternal = true;
                    ev.Link = meetupEventSearchResult.Event_Url ?? string.Empty;
                    ev.OwnerName = meetupEventSearchResult.Group.Name ?? string.Empty;
                    ev.SignedUpCount = meetupEventSearchResult.Yes_Rsvp_Count;
                    ev.Venue = venueNameAddress;
                    ev.VenueLongitude = meetupEventSearchResult.Venue == null ? string.Empty : meetupEventSearchResult.Venue.Lon.ToString(CultureInfo.InvariantCulture);
                    ev.VenueLatitude = meetupEventSearchResult.Venue == null ? string.Empty : meetupEventSearchResult.Venue.Lat.ToString(CultureInfo.InvariantCulture);
                    ev.VenueCapacity = meetupEventSearchResult.Rsvp_Limit;

                    events.Add(ev);
                }
            }

            return events;
        }
    }
}
