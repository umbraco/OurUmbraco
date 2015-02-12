using System.Linq;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace NotificationsWeb.Api
{
    public class NotificationsController:UmbracoApiController
    {
        [HttpGet]
        public string SubscribeToForumTopic(int id)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.ForumTopic.Subscribe(id, currentMemberId);

                return "true";
            }

            return "false";
        }

        [HttpGet]
        public string UnSubscribeFromForumTopic(int id)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.ForumTopic.UnSubscribe(id, currentMemberId);

                return "true";
            }

            return "false";
        }

        [HttpGet]
        public string SubscribeToForum(int id)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.Forum.Subscribe(id, currentMemberId);

                return "true";
            }

            return "false";
        }

        [HttpGet]
        public string UnSubscribeFromForum(int id)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                BusinessLogic.Forum.UnSubscribe(id, currentMemberId);

                return "true";
            }

            return "false";
        }
    }
}
