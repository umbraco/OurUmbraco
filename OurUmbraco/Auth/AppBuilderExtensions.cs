using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Umbraco.Core;
using umbraco;
using OurUmbraco.Our.Extensions;
using Microsoft.Owin.Security.Provider;

namespace OurUmbraco.Auth
{
    public static class AppBuilderExtensions
    {
        public static void ConfigureHmacBearerTokenAuthentication(this IAppBuilder appBuilder, string[] authenticatedPaths)
        {
            appBuilder.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                Provider = new OAuthBearerAuthenticationProvider
                {
                    //This is the first callback made when OWIN starts
                    //to process the request, here we need to set the token
                    //to null if we don't want it to be processed - basically
                    //this is where we need to:
                    // * check the current request URL to see if we should auth the request (only deploy end points)
                    OnRequestToken = context =>
                    {
                        var umbPath = GlobalSettings.UmbracoMvcArea;

                        var applicationBasePath = (context.Request.PathBase.HasValue ? context.Request.PathBase.Value : string.Empty).EnsureEndsWith('/');
                        
                        var requestPath = context.Request.Uri.CleanPathAndQuery();

                        //Only authenticated endpoints to be authenticated and these must have the projectid
                        //and memberid headers in the request
                        if (authenticatedPaths.Any(s => requestPath.StartsWith($"{applicationBasePath}{umbPath}/{s}", StringComparison.InvariantCultureIgnoreCase))
                            && EnsureHeaderValues(context, ProjectAuthConstants.ProjectIdHeader, ProjectAuthConstants.MemberIdHeader))
                        {                            
                            return Task.FromResult(0);                         
                        }   

                        context.Token = null;
                        return Task.FromResult(0);
                    }
                },
                AccessTokenProvider = new AuthenticationTokenProvider
                {
                    //Callback used to parse the token in the request,
                    //if the token parses correctly then we should assign a ticket
                    //to the request, this is the "User" that will get assigned to
                    //the request with Claims.
                    //If the token is invalid, then don't assign a ticket and OWIN
                    //will take care of the rest (not authenticated)
                    OnReceive = context =>
                    {
                        var requestPath = context.Request.Uri.CleanPathAndQuery();
                        if (!TryGetHeaderValue(context, ProjectAuthConstants.ProjectIdHeader, out var projectId))
                            throw new InvalidOperationException("No project Id found in request"); // this will never happen
                        if (!TryGetHeaderValue(context, ProjectAuthConstants.MemberIdHeader, out var memberId))
                            throw new InvalidOperationException("No project Id found in request"); // this will never happen

                        //Get the stored auth key for this project and member
                        var tokenService = new ProjectAuthKeyService(ApplicationContext.Current.DatabaseContext);
                        var authKey = tokenService.GetAuthKey(memberId, projectId);

                        if (!HMACAuthentication.ValidateToken(context.Token, requestPath, authKey, out DateTime timestamp))
                            throw new InvalidOperationException("Token validation failed");

                        //If ok, create a ticket here with the Claims we need to check for in AuthZ
                        var ticket = new AuthenticationTicket(
                            new ClaimsIdentity(
                                new List<Claim>
                                {
                                    new Claim(ProjectAuthConstants.BearerTokenClaimType, ProjectAuthConstants.BearerTokenClaimValue),
                                    new Claim(ProjectAuthConstants.ProjectIdClaim, projectId.ToInvariantString()),
                                    new Claim(ProjectAuthConstants.MemberIdClaim, memberId.ToInvariantString()),
                                },

                                //The authentication type = this is important, if not set
                                //then the ticket's IsAuthenticated property will be false
                                authenticationType: ProjectAuthConstants.BearerTokenAuthenticationType),
                            new AuthenticationProperties
                            {
                                //Expires after 5 minutes in case there are some long running operations
                                ExpiresUtc = timestamp.AddMinutes(5)
                            });

                        context.SetTicket(ticket);
                    }
                }
            });
        }

        private static bool EnsureHeaderValues(BaseContext context, params string[] headers)
        {
            foreach(var h in headers)
            {
                if (!TryGetHeaderValue(context, h, out _))
                    return false;
            }
            return true;
        }

        private static bool TryGetHeaderValue(BaseContext context, string header, out int value)
        {
            var rawValue = context.Request.Headers.Get(header);
            var converted = rawValue.TryConvertTo<int>();
            if (converted.Success) { 
                value = converted.Result;
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }
    }
}
