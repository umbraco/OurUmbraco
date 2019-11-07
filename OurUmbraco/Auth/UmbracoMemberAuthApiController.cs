using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Auth
{
    public abstract class UmbracoMemberAuthApiController : UmbracoApiController
    {
        /// <summary>
        /// When a user has been authorised with a JWT Auth token
        /// This is the member represents the authorised member
        /// that can then be used in the API controller for Umbraco API Service Calls 
        /// such as creating content
        /// </summary>
        public IMember AuthorisedMember
        {
            get
            {
                return ControllerContext.Request.Properties["umbraco-member"] as IMember;
            }
        }

        /// <summary>
        /// When a user has been authorised with a JWT Auth token
        /// This is the project/package ID from the JWT
        /// that can then be used in the API controller for lookup
        /// </summary>
        public int ProjectNodeId
        {
            get
            {
                return (int)ControllerContext.Request.Properties["project-nodeid"];
            }
        }

    }
}
