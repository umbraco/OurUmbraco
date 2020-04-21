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
        
        /// <summary>
        /// Used by clientside code to create API keys for a member, a key needs a description, member id and project id
        /// </summary>
        /// <param name="projectId">The package id, corresponds to the content node id in the backoffice</param>
        /// <param name="description">A description of the key set by the member to differentiate their keys</param>
        /// <returns>Returns a ProjectAuthKey model which contains the key and other needed info</returns>
        /// <exception cref="HttpResponseException">Returns exception if the member id is not registered as the owner of the package</exception>
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
        
        /// <summary>
        /// Used by clientside code to remove API keys for a member
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="contribId">This is the member id</param>
        /// <param name="primaryKey">PK in the db table - used to differentiate several keys with the same member and project id</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">Returns exception if the member id is not registered as the owner of the package</exception>
        [HttpPost]
        public void RemoveKey(int projectId, int contribId, int primaryKey)
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