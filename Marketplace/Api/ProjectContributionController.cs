using Marketplace.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace Marketplace.Api
{
    [MemberAuthorize(AllowType = "member")]
    public class ProjectContributionController : UmbracoApiController
    {
        [HttpPut]
        public HttpResponseMessage UpdateCollaborationStatus(int projectId, bool status)
        {
            var cs = Services.ContentService;
            var project = cs.GetById(projectId);

            if (project.GetValue<int>("owner") == Members.GetCurrentMemberId())
            {
                project.SetValue("openForCollab", status);

                cs.Save(project);

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }

        [HttpGet]
        public ExpandoObject AddContributor(int projectId, string email)
        {

           dynamic o = new ExpandoObject();
           var member = Members.GetByEmail(email) ;
           if(member == null)
           {
               o.success = false;
               o.error = "Email not found";
               return o;

           }
           
            using(var cs = new  ContributionService())
            {
                cs.AddContributor(projectId, member.Id);
                o.success = true;
                o.memberName = member.Name;
                o.memberId = member.Id;
                return o;
            }




        }

        public HttpResponseMessage DeleteContributor(int projectId, int memberId)
        {
            UmbracoHelper help = new UmbracoHelper(UmbracoContext);
            var project = help.TypedContent(projectId);

            if(project.GetPropertyValue<int>("owner") == Members.GetCurrentMemberId())
            { 
                using (var cs = new ContributionService())
                {
                    cs.DeleteContributor(projectId, memberId);
                }

                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
        }
    }
}