using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uForum.Services;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;
using uForum;

namespace our.custom_Handlers {
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

        void CommentService_Created(object sender, uForum.CommentEventArgs e)
        {
            if (e.Comment != null && e.Comment.MemberId > 0)
            {
                var ms = UmbracoContext.Current.Application.Services.MemberService;
                var member = ms.GetById(e.Comment.MemberId);
                member.IncreaseForumPostCount();
                ms.Save(member);

                uPowers.BusinessLogic.Action a = new uPowers.BusinessLogic.Action("NewComment");
                a.Perform(member.Id, e.Comment.Id, "New comment created");  
            }
        }

        void TopicService_Created(object sender, uForum.TopicEventArgs e)
        {
            if (e.Topic != null && e.Topic.MemberId > 0)
            {
                var ms = UmbracoContext.Current.Application.Services.MemberService;
                var member = ms.GetById(e.Topic.MemberId);
                member.IncreaseForumPostCount();
                ms.Save(member);

                uPowers.BusinessLogic.Action a = new uPowers.BusinessLogic.Action("NewTopic");
                a.Perform(member.Id, e.Topic.Id, "New topic created");
            }
        }

    }
}
