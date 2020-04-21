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
        public ProjectAuthKey AddKey(int projectId, string description)
        {
            var project = Umbraco.TypedContent(projectId);
            var memberId = Members.GetCurrentMemberId();
            
            if (project.GetPropertyValue<int>("owner") == memberId )
            {
                try
                {
                    var authKey = _keyService.CreateAuthKey(memberId, projectId, description);
                    return authKey;
                }
                catch (InvalidOperationException)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    response.Content = new StringContent("We could not create a new key, please try again later");
                    throw new HttpResponseException(response);
                }
            }
            var resp = new HttpResponseMessage(HttpStatusCode.Forbidden);
            resp.Content = new StringContent("Only the owner of the package are allowed to create keys");
            throw new HttpResponseException(resp);
        }
        
        [HttpPost]
        public ProjectAuthKey RemoveKey(int projectId, int contribId, int primaryKey)
        {
            var project = Umbraco.TypedContent(projectId);
            var memberId = Members.GetCurrentMemberId();
            
            if (project.GetPropertyValue<int>("owner") == memberId )
            {
                try
                {
                    _keyService.DeleteAuthKey(contribId, projectId, primaryKey);
                }
                catch (InvalidOperationException)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    response.Content = new StringContent("Your API key couldn't be deleted, please give it a moment then try again.");
                    throw new HttpResponseException(response);
                }
            }
            var resp = new HttpResponseMessage(HttpStatusCode.Forbidden);
            resp.Content = new StringContent("Only the owner of the package can remove keys");
            throw new HttpResponseException(resp);
        }
    }
}