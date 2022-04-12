using System.Web.Mvc;
using OurUmbraco.Community.Meetup;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Community.Controllers
{

    public class MeetupsController : SurfaceController
    {
        public ActionResult GetEvents()
        {
            var meetupService = new MeetupService();
            var meetups = meetupService.GetCachedUpcomingMeetups();
            return PartialView("~/Views/Partials/Home/Meetups.cshtml", meetups);
        }
    }
}