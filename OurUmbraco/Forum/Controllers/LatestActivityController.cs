using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Forum.Services;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Forum.Controllers
{
    public class LatestActivityController : SurfaceController
    {
        public ActionResult LatestActivity(int numberOfTopics = 10)
        {
            var discourseService = new DiscourseService();
            var discourseTopics = discourseService.GetLatestTopics("questions", 5);
            var topics = new List<Models.DiscourseTopic>();
            if(discourseTopics != null)
            {
                topics = discourseTopics.Take(numberOfTopics).ToList();
            }

            return PartialView("~/Views/Partials/Home/LatestForumActivity.cshtml", topics);
        }
    }
}
