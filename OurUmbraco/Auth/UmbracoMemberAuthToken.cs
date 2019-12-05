using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using JWT;

namespace OurUmbraco.Auth
{
    public class UmbracoMemberAuthToken : ActionFilterAttribute
    {
        private UmbracoAuthTokenFactory _umbracoAuthTokenFactory;
        private UmbracoAuthTokenDbHelper _umbracoAuthTokenDbHelper;

        /// <summary>
        /// Assign this attribute to protect a WebAPI call for an Umbraco member
        /// </summary>
        public UmbracoMemberAuthToken()
        {
            _umbracoAuthTokenFactory = new UmbracoAuthTokenFactory();
            _umbracoAuthTokenDbHelper = new UmbracoAuthTokenDbHelper();
        }

        /// <summary>
        /// When the attribute is decorated on an Umbraco WebApi Controller
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                //Auth the member from the request (HTTP headers) with the JWT as an Auth header
                var member = Authenticate(actionContext.Request, out int projectId);

                //Member details not correct (as member obj null)
                if (member == null)
                {
                    //Return a HTTP 401 Unauthorised header
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                //Set the user in route data so the WebAPI controller can use this user object & do what needed with it
                actionContext.ControllerContext.Request.Properties.Add("umbraco-member", member);

                //Set the project-nodeid in route data so the WebAPI controller can use it
                actionContext.ControllerContext.Request.Properties.Add("project-nodeid", projectId);
            }
            catch (Exception ex)
            {
                //Return a HTTP 401 Unauthorised header
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            //Continue as normal
            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// Try and auth the user from the HTTP headers on the request to the API
        /// Look for bearer token aka JWT token & try to verify & deserialise it
        /// </summary>
        /// <param name="request"></param>
        /// <returns>If success auth'd return the associated Umbraco backoffice user</returns>
        private IMember Authenticate(HttpRequestMessage request, out int projectId)
        {
            //Try to get the Authorization header in the request
            var ah = request.Headers.Authorization;
            projectId = int.MinValue;

            //If no Auth header sent or the scheme is not bearer aka TOKEN
            if (ah == null || ah.Scheme.ToLower() != "bearer")
            {
                //Return null (by returning null, base method above will return it as HTTP 401)
                return null;
            }

            //Get the JWT token from auth HTTP header param  param (Base64 encoded - username:password)
            var jwtToken = ah.Parameter;

            try
            {
                //Decode & verify token was signed with our secret
                var decodeJwt = _umbracoAuthTokenFactory.DecodeUserAuthToken(jwtToken);

                //Ensure our token is not null (was decoded & valid)
                if (decodeJwt != null)
                {
                    //Just the presence of the token & being deserialised with correct SECRET key is a good sign
                    //Get the member from userService from it's id
                    var member = ApplicationContext.Current.Services.MemberService.GetById(decodeJwt.MemberId);

                    //If user is NOT Approved OR the user is Locked Out
                    if (!member.IsApproved || member.IsLockedOut)
                    {
                        //Return null (by returning null, base method above will return it as HTTP 401)
                        return null;
                    }

                    //Verify token is what we have on the user
                    var isTokenValid = _umbracoAuthTokenDbHelper.IsTokenValid(decodeJwt);

                    //Token matches what we have in DB
                    if (isTokenValid == false)
                    {
                        //Token does not match in DB (by returning null, base method above will return it as HTTP 401)
                        return null;
                    }

                    //Get the project ID in the decoded JWT
                    projectId = decodeJwt.ProjectId;

                    // Project ID is an int for a content node in backoffice
                    // It contains a property called `owner` which stores the int of the memberID
                    var projectNode = UmbracoContext.Current.ContentCache.GetById(projectId);
                    var projectOwner = projectNode.GetPropertyValue<int>("owner");

                    if(projectOwner != decodeJwt.MemberId)
                    {
                        //Return null (by returning null, base method above will return it as HTTP 401)
                        return null;
                    }

                    //Lets return the member
                    return member;
                }


                //JWT token could not be serialised to AuthToken object
                return null;
            }
            catch (TokenExpiredException ex)
            {
                //Bubble exception up
                throw ex;
            }
            catch (SignatureVerificationException ex)
            {
                //Bubble exception up
                throw ex;
            }
        }
    }
}
