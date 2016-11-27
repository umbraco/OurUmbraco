using OurUmbraco.Forum;
using OurUmbraco.Forum.Extensions;
using OurUmbraco.Forum.Services;
using OurUmbraco.Powers.BusinessLogic;
using Umbraco.Core;
using Umbraco.Web;

namespace OurUmbraco.Our.CustomHandlers {
    public class ForumPostsCounter : ApplicationEventHandler {

        /*
         * This handler updates the members forum posts counter
         * So the forum doesnt know about where and how the member stores these counts
         */
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TopicService.Created += TopicService_Created;
            CommentService.Created += CommentService_Created;
        }

        void CommentService_Created(object sender, CommentEventArgs e)
        {
            if (e.Comment != null && e.Comment.MemberId > 0)
            {
                var ms = ApplicationContext.Current.Services.MemberService;
                var member = ms.GetById(e.Comment.MemberId);
                member.IncreaseForumPostCount();
                ms.Save(member);

                Action a = new Action("NewComment");
                a.Perform(member.Id, e.Comment.Id, "New comment created");  
            }
        }

        void TopicService_Created(object sender, TopicEventArgs e)
        {
            if (e.Topic != null && e.Topic.MemberId > 0)
            {
                var ms = ApplicationContext.Current.Services.MemberService;
                var member = ms.GetById(e.Topic.MemberId);
                member.IncreaseForumPostCount();
                ms.Save(member);

                Action a = new Action("NewTopic");
                a.Perform(member.Id, e.Topic.Id, "New topic created");
            }
        }

    }
}
