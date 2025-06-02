using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using OurUmbraco.Forum.Services;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Forum.Controllers
{
    public class LatestActivityController : SurfaceController
    {
        public async Task<ActionResult> LatestActivity(int numberOfTopics = 10)
        {
            var discourseService = new DiscourseService();
            var discourseQuestions = await discourseService.GetLatestTopicsAsync("questions", 5);
            var discourseTopics = discourseQuestions.Take(numberOfTopics).ToList();

            return PartialView("~/Views/Partials/Home/LatestForumActivity.cshtml", discourseTopics);
        }
    }
}
