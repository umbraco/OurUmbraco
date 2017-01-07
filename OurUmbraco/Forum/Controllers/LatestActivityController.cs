using System.Linq;
using System.Web.Mvc;
using OurUmbraco.Forum.Services;
using Umbraco.Web.Mvc;

namespace OurUmbraco.Forum.Controllers
{
    public class LatestActivityController : SurfaceController
    {
        public ActionResult LatestActivity(int numberOfTopics = 6)
        {
            if (numberOfTopics > 30)
                numberOfTopics = 6;

            var ts = new TopicService(ApplicationContext.DatabaseContext);
            var topics = ts.GetLatestTopics(numberOfTopics).ToArray();
            foreach (var topic in topics)
            {
                var category = Umbraco.TypedContent(topic.ParentId);
                if(category != null)
                    topic.CategoryName = category.Name;

                var member = Members.GetById(topic.LatestReplyAuthor) ?? Members.GetById(topic.MemberId);
                if (member != null)
                    topic.LastReplyAuthorAvatar = Our.Utils.GetMemberAvatar(member, 100);
            }

            return PartialView("~/Views/Partials/Home/LatestForumActivity.cshtml", topics);
        }
    }
}
