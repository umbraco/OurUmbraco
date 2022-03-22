using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Community.Meetup;
using Umbraco.Web.Mvc;
using OurUmbraco.Community.Meetup.Models;
using Skybrud.Social.Meetup.Models.GraphQl.Events;

namespace OurUmbraco.Community.Controllers
{

    public class MeetupsController : SurfaceController
    {
        public ActionResult GetEvents()
        {
            var meetupService = new MeetupService();
            var meetupGroups = meetupService.GetUpcomingMeetups();

            var meetupEvents = new List<OurMeetupGroup>();
            
            foreach (var groupResult in meetupGroups.Where(x => x.Data.GroupByUrlname != null))
            {
                var group = groupResult.Data.GroupByUrlname;
                var groupEvents = new List<MeetupEvent>();
                if (group.UpcomingEvents.Edges.Any())
                {
                    groupEvents.AddRange(group.UpcomingEvents.Edges.Select(eventsEdge => eventsEdge.Node));
                }

                meetupEvents.Add(new OurMeetupGroup() { Group = group, Events = groupEvents });
            }
            
            return PartialView("~/Views/Partials/Home/Meetups.cshtml", meetupEvents);
        }


    }
}