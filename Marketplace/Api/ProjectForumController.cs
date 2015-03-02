using Marketplace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    Id = obj.Id
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
                Description = content.GetPropertyValue<string>("forumDescription")
            };
        }

        public ProjectForum PostProjectForum(ProjectForum forum, int parentId)
        {
            var cs = Services.ContentService;
            var content = cs.CreateContent(forum.Title, parentId, "Forum");
            content.Name = forum.Title;
            content.SetValue("forumDescription", forum.Description);
            cs.SaveAndPublish(content);
            forum.Id = content.Id;
            return forum;

        }

        public ProjectForum PutProjectForum(ProjectForum forum)
        {
            var cs = Services.ContentService;
            var content = cs.GetById(forum.Id);
            content.Name = forum.Title;
            content.SetValue("forumDescription", forum.Description);
            cs.SaveAndPublish(content);
            return forum;

        }

        public void DeleteProjectForum(int forumId)
        {
            var cs = Services.ContentService;
            var content = cs.GetById(forumId);
            cs.Delete(content);
        }
    }
}