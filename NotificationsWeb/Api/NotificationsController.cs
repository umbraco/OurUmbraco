using System.Linq;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace NotificationsWeb.Api
{
    public class NotificationsController:UmbracoApiController
    {
        [HttpGet]
        public string SubscribeToForumTopic(int topicId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.ForumTopic.Subscribe(topicId, currentMemberId);

                return "true";
            }

            return "false";
        }

        [HttpGet]
        public string UnSubscribeFromForumTopic(int topicId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.ForumTopic.UnSubscribe(topicId, currentMemberId);

                return "true";
            }

            return "false";
        }

        [HttpGet]
        public string SubscribeToForum(int forumId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.Forum.Subscribe(forumId, currentMemberId);

                return "true";
            }

            return "false";
        }

        [HttpGet]
        public string UnSubscribeFromForum(int forumId)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.Forum.UnSubscribe(forumId, currentMemberId);

                return "true";
            }

            return "false";
        }
    }
}
