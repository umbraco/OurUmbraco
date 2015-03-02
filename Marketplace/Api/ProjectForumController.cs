using Marketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Marketplace.Api
{
    [MemberAuthorize(AllowType = "member")]
    public class ProjectForumController : UmbracoApiController
    {
        public IEnumerable<ProjectForum> GetAllForums(int parentId)
        {
            UmbracoHelper help = new UmbracoHelper(UmbracoContext);
            return help.TypedContent(parentId)
                .Children
                .Where(c => c.DocumentTypeAlias == "Forum")
                .Select(obj => new ProjectForum()
                {
                    Title = obj.Name,
                    Description = obj.GetPropertyValue<string>("forumDescription"),
                    Id = obj.Id,
                    ParentId = obj.Parent.Id
                });
        }

        public ProjectForum GetForum(int id)
        {
            UmbracoHelper help = new UmbracoHelper(UmbracoContext);
            var content = help.TypedContent(id);
            return new ProjectForum
            {
                Id = content.Id,
                Title = content.Name,
                Description = content.GetPropertyValue<string>("forumDescription"),
                ParentId = content.Parent.Id
            };
        }

        public ProjectForum PostProjectForum(ProjectForum forum)
        {
            UmbracoHelper help = new UmbracoHelper(UmbracoContext);
            var project = help.TypedContent(forum.ParentId);

            if (project.GetPropertyValue<int>("owner") == Members.GetCurrentMemberId())
            {
                var cs = Services.ContentService;
                var content = cs.CreateContent(forum.Title, forum.ParentId, "Forum");
                content.Name = forum.Title;
                content.SetValue("forumDescription", forum.Description);
                cs.SaveAndPublish(content);
                forum.Id = content.Id;
                return forum;
            }

            return null;

        }

        public ProjectForum PutProjectForum(ProjectForum forum)
        {
             UmbracoHelper help = new UmbracoHelper(UmbracoContext);
            var project = help.TypedContent(forum.ParentId);

            if (project.GetPropertyValue<int>("owner") == Members.GetCurrentMemberId())
            {
                var cs = Services.ContentService;
                var content = cs.GetById(forum.Id);
                content.Name = forum.Title;
                content.SetValue("forumDescription", forum.Description);
                cs.SaveAndPublish(content);
                return forum;
            }
            return null;

        }

        public void DeleteProjectForum(int forumId)
        {
             UmbracoHelper help = new UmbracoHelper(UmbracoContext);
             var project = help.TypedContent(forumId).Parent;

            if (project.GetPropertyValue<int>("owner") == Members.GetCurrentMemberId())
            {
                var cs = Services.ContentService;
                var content = cs.GetById(forumId);
                cs.Delete(content);
            }
        }
    }
}