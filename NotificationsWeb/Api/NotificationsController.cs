using NotificationsWeb.Services;
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
                using(var ns = new NotificationService())
                {
                    ns.SubscribeToForumTopic(id, currentMemberId);
                }

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
                using (var ns = new NotificationService())
                {
                    ns.UnSubscribeFromForumTopic(id, currentMemberId);
                }
                

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
                using(var ns = new NotificationService())
                {
                    ns.SubscribeToForum(id, currentMemberId);
                }

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
                using (var ns = new NotificationService())
                {
                    ns.UnSubscribeFromForum(id, currentMemberId);
                }
                

                return "true";
            }

            return "false";
        }
    }
}
