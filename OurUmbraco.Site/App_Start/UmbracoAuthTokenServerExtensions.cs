using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Umbraco.IdentityExtensions;

namespace OurUmbraco.Site
{
    /// <summary>
    /// Extension methods to configure Umbraco for issuing and processing tokens for authentication
    /// </summary>    
    public static class UmbracoAuthTokenServerExtensions
    {

        /// <summary>
        /// Configures Umbraco to issue and process authentication tokens
        /// </summary>
        /// <param name="app"></param>
        /// <param name="backofficeAuthServerProviderOptions"></param>
        /// <remarks>
        /// This is a very simple implementation of token authentication, the expiry below is for a single day and with
        /// this implementation there is no way to force expire tokens on the server however given the code below and the additional
        /// callbacks that can be registered for the BackOfficeAuthServerProvider these types of things could be implemented. Additionally the
        /// BackOfficeAuthServerProvider could be overridden to include this functionality instead of coding the logic into the callbacks.
        /// </remarks>
        /// <example>
        /// 
        /// An example of using this implementation is to use the UmbracoStandardOwinSetup and execute this extension method as follows:
        /// 
        /// <![CDATA[
        /// 
        ///   public override void Configuration(IAppBuilder app)
        ///   {
        ///       //ensure the default options are configured
        ///       base.Configuration(app);
        ///   
        ///       //configure token auth
        ///       app.UseUmbracoBackOfficeTokenAuth();
        ///   }
        /// 
        /// ]]>
        /// 
        /// Then be sure to read the details in UmbracoStandardOwinSetup on how to configure Owin to startup using it.
        /// </example>
        public static void UseUmbracoBackOfficeTokenAuth(this IAppBuilder app, BackOfficeAuthServerProviderOptions backofficeAuthServerProviderOptions = null)
        {
            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //generally you wouldn't allow this unless on SSL!
#if DEBUG
                AllowInsecureHttp = true,
#endif
                
                TokenEndpointPath = new PathString("/umbraco/oauth/token"),
                //set as different auth type to not interfere with anyone doing this on the front-end
                AuthenticationType = Umbraco.Core.Constants.Security.BackOfficeTokenAuthenticationType,
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new BackOfficeAuthServerProvider(backofficeAuthServerProviderOptions)
                {
                    OnValidateClientAuthentication = context =>
                    {
                        // Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
                        // present on the request. If the web application accepts Basic authentication credentials, 
                        // context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web 
                        // application accepts "client_id" and "client_secret" as form encoded POST parameters, 
                        // context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
                        // If context.Validated is not called the request will not proceed further. 

                        //** Currently we just accept everything globally
                        context.Validated();
                        return Task.FromResult(0);

                        // Example for checking registered clients:

                        //** Validate that the data is in the request
                        //string clientId;
                        //string clientSecret;
                        //if (context.TryGetFormCredentials(out clientId, out clientSecret) == false)
                        //{
                        //    context.SetError("invalid_client", "Form credentials could not be retrieved.");
                        //    context.Rejected();
                        //    return Task.FromResult(0);
                        //}

                        //var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();

                        //** Check if this client id is allowed/registered
                        // - lookup in custom table

                        //** Verify that the client id and client secret match 
                        //if (client != null && userManager.PasswordHasher.VerifyHashedPassword(client.ClientSecretHash, clientSecret) == PasswordVerificationResult.Success)
                        //{
                        //    // Client has been verified.
                        //    context.Validated(clientId);
                        //}
                        //else
                        //{
                        //    // Client could not be validated.
                        //    context.SetError("invalid_client", "Client credentials are invalid.");
                        //    context.Rejected();
                        //}
                    }
                }
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }
    }
}
