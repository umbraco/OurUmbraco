using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OurUmbraco.Auth;
using Umbraco.Web;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Project.Api
{
    [MemberAuthorize(AllowType = "member")]
    public class ProjectApiKeyController : UmbracoApiController
    {
        private readonly ProjectAuthKeyService _keyService;
        
        public ProjectApiKeyController()
        {
            _keyService = new ProjectAuthKeyService(ApplicationContext.DatabaseContext);
        }
        
        [HttpPost]
        public ProjectAuthKey AddKey(int projectId, int contribId, string description)
        {
            var project = Umbraco.TypedContent(projectId);
            var memberId = Members.GetCurrentMemberId();
            
            if (project.GetPropertyValue<int>("owner") == memberId )
            {
                try
                {
                    var authKey = _keyService.CreateAuthKey(contribId, projectId, description);
                    return authKey;
                }
                catch (InvalidOperationException)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    response.Content = new StringContent("A key already exists for the user");
                    throw new HttpResponseException(response);
                }
                
            }
            return null;
        }
        
        [HttpPost]
        public ProjectAuthKey UpdateKey(int projectId)
        {
            var project = Umbraco.TypedContent(projectId);
            var memberId = Members.GetCurrentMemberId();
            
            if (project.GetPropertyValue<int>("owner") == memberId )
            {
                var authKey = _keyService.CreateAuthKey(memberId, projectId);
                return authKey;
            }
            return null;
        }
    }
}