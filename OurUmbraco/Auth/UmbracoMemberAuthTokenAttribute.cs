using System.Web.Http.Controllers;
using System.Web.Http;
using System.Security.Claims;

namespace OurUmbraco.Auth
{
    /// <summary>
    /// Ensures the request is authenticated and populated with the correct claims
    /// </summary>
    public class ProjectBearerTokenAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.ControllerContext.RequestContext.Principal;
            var identity = principal?.Identity;

            if (identity == null)
                return false;

            if (identity.IsAuthenticated == false || identity.AuthenticationType != ProjectAuthConstants.BearerTokenAuthenticationType)
                return false;

            var claims = (ClaimsIdentity)identity;
            return claims.HasClaim(ProjectAuthConstants.BearerTokenClaimType, ProjectAuthConstants.BearerTokenClaimValue);
        }
    }
}
