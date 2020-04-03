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
                var authKey = _keyService.CreateAuthKey(memberId, projectId, description);
                return authKey;
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
                var authKey = _keyService.CreateAuthKey(memberId, projectId, description);
                return authKey;
            }
            return null;
        }
    }
}