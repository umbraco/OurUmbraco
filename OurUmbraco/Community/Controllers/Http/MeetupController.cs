using OurUmbraco.Community.Meetup;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Community.Controllers.Http
{
    public class MeetupController : UmbracoApiController
    {
        private readonly MeetupService _meetupService;

        public MeetupController()
        {
            _meetupService = new MeetupService();
        }

        [HttpGet]
        public IHttpActionResult Index()
        {
            var meetups = _meetupService.GetCachedUpcomingMeetups();

            return Ok(meetups);
        }
    }
}
