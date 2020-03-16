using Microsoft.AspNet.Identity;
using OurUmbraco.Auth;
using System;
using System.Security.Claims;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi;

namespace OurUmbraco.Project.Api
{
    /// <summary>
    /// Abstract controller ensuring that all endpoints are authorized with the correct Claims
    /// </summary>
    [ProjectBearerTokenAuthorize]
    public abstract class UmbracoMemberAuthApiController : UmbracoApiController
    {
        private IMember _authenticatedMember;
        private int? _projectId;

        /// <summary>
        /// Returns the currently authenticated member in the request
        /// </summary>
        public IMember AuthenticatedMember
        {
            get
            {
                if (_authenticatedMember != null) return _authenticatedMember;

                var identity = RequestContext.Principal?.Identity as ClaimsIdentity;
                if (identity == null) return null;

                var memberId = identity.GetUserId<int>();
                _authenticatedMember = Services.MemberService.GetById(memberId);
                if (_authenticatedMember == null)
                    throw new InvalidOperationException($"No member was found by id {memberId}");

                return _authenticatedMember;
            }
        }

        /// <summary>
        /// Returns the currently authenticated project id in the request
        /// </summary>
        public int ProjectNodeId
        {
            get
            {
                if (_projectId != null) return _projectId.Value;

                var identity = RequestContext.Principal?.Identity as ClaimsIdentity;
                if (identity == null)
                    throw new InvalidOperationException("The current identity is not a ClaimsIdentity");

                var projId = identity.FindFirstValue(ProjectAuthConstants.ProjectIdClaim).TryConvertTo<int?>();
                _projectId = projId.Success ? projId.Result : null;

                if (_projectId == null)
                    throw new InvalidOperationException("The project id claim was not found");

                return _projectId.Value;
            }
        }


    }
}
