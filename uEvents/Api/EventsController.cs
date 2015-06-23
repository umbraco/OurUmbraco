using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Xml.XPath;
using uEvents.Meetup;
using umbraco;
using umbraco.cms.businesslogic.cache;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.relation;
using umbraco.NodeFactory;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace uEvents.Api
{
    public class EventsController : UmbracoApiController
    {
        [HttpGet]
        public string Toggle(int eventId)
        {
            var umbracoEvent = new Event(eventId);

            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                if (Relation.IsRelated(eventId, currentMemberId) == false)
                    umbracoEvent.SignUp(currentMemberId, "no comment");
                else
                    umbracoEvent.Cancel(currentMemberId, "no comment");
            }

            return "true";
        }

        [HttpGet]
        public string Sync(int eventId)
        {
            var umbracoEvent = new Event(eventId);
            umbracoEvent.syncCapacity();
            return "true";
        }

        [HttpGet]
        public string SignUp(int eventId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;

            if (currentMemberId > 0)
            {
                var umbracoEvent = new Event(eventId);
                umbracoEvent.SignUp(currentMemberId, "no comment");
            }

            return "true";
        }

        [HttpGet]
        public string Cancel(int eventId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;

            if (currentMemberId > 0)
            {
                var umbracoEvent = new Event(eventId);
                umbracoEvent.Cancel(currentMemberId, "no comment");
            }

            return "true";
        }

        public static XPathNodeIterator UpcomingEvents()
        {
            object eventCacheSyncLock = new object();
            return Cache.GetCacheItem<XPathNodeIterator>("ourEvents", eventCacheSyncLock, new TimeSpan(1, 0, 0),
                delegate
                {
                    return UpcomingEvents();
                });
        }

        public static List<Models.Event> UpcomingEvents(int parentNode)
        {
            var events = new List<Models.Event>();

            var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            var eventOverview = umbracoHelper.TypedContent(parentNode);


            foreach (var publishedEvent in eventOverview.Children.Where(e => e.DocumentTypeAlias == "Event"))
            {
                var startDateTime = publishedEvent.GetPropertyValue<DateTime>("start");
                if (startDateTime.Date < DateTime.Today)
                    continue;

                var endDateTime = publishedEvent.GetPropertyValue<DateTime>("end");

                var venueCapacity = publishedEvent.GetPropertyValue<int>("capacity");
                var signedUpCount = publishedEvent.GetPropertyValue<int>("signedup");
                var ownerId = publishedEvent.GetPropertyValue<int>("owner");

                var ownerName = string.Empty;

                var member = Member.GetMemberFromCache(ownerId);
                if (member != null)
                    ownerName = member.Text;
                
                var ev = new Models.Event
                {
                    Id = publishedEvent.Id.ToString(CultureInfo.InvariantCulture),
                    Name = publishedEvent.Name,
                    Description = publishedEvent.GetPropertyValue<string>("description"),
                    StartDateTime = startDateTime,
                    EndDateTime = endDateTime,
                    Link = library.NiceUrl(int.Parse(publishedEvent.Id.ToString(CultureInfo.InvariantCulture))),
                    Venue = publishedEvent.GetPropertyValue<string>("venue"),
                    VenueLongitude = publishedEvent.GetPropertyValue<string>("longitude"),
                    VenueLatitude = publishedEvent.GetPropertyValue<string>("latitude"),
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
