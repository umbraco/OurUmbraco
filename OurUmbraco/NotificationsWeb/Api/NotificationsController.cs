using System.Web.Http;
using OurUmbraco.NotificationsWeb.Services;
using Umbraco.Web.WebApi;

namespace OurUmbraco.NotificationsWeb.Api
{
    [Authorize]
    public class NotificationsController : UmbracoApiController
    {
        public NotificationsController()
        {
            _notificationService = new NotificationService(DatabaseContext);
        }

        private readonly NotificationService _notificationService;

        [HttpGet]
        public string SubscribeToForumTopic(int id)
        {
            var currentMemberId = Members.GetCurrentMember().Id;
            if (currentMemberId > 0)
            {
                _notificationService.SubscribeToForumTopic(id, currentMemberId);

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
                _notificationService.UnSubscribeFromForumTopic(id, currentMemberId);

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
                _notificationService.SubscribeToForum(id, currentMemberId);

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
                _notificationService.UnSubscribeFromForum(id, currentMemberId);
                

                return "true";
            }

            return "false";
        }
    }
}
