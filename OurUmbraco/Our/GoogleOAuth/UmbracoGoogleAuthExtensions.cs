using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Google;
using Owin;
using Umbraco.Core;
using Umbraco.Web.Security.Identity;

namespace OurUmbraco.Our.GoogleOAuth
{
    public static class UmbracoGoogleAuthExtensions
    {
        ///  <summary>
        ///  Configure google sign-in
        ///  </summary>
        ///  <param name="app"></param>
        ///  <param name="clientId"></param>
        ///  <param name="clientSecret"></param>
        /// <param name="caption"></param>
        /// <param name="style"></param>
        /// <param name="icon"></param>
        /// <remarks>
        ///  
        ///  Nuget installation:
        ///      Microsoft.Owin.Security.Google
        /// 
        ///  Google account documentation for ASP.Net Identity can be found:
        ///  
        ///  http://www.asp.net/web-api/overview/security/external-authentication-services#GOOGLE
        ///  
        ///  Google apps can be created here:
        ///  
        ///  https://developers.google.com/accounts/docs/OpenIDConnect#getcredentials
        ///  
        ///  </remarks>
        public static void ConfigureBackOfficeGoogleAuth(this IAppBuilder app, string clientId, string clientSecret,
            string caption = "Google", string style = "btn-google-plus", string icon = "fa-google-plus")
        {
            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                //In order to allow using different google providers on the front-end vs the back office,
                // these settings are very important to make them distinguished from one another.
                SignInAsAuthenticationType = Constants.Security.BackOfficeExternalAuthenticationType,
                //  By default this is '/signin-google', you will need to change that default value in your
                //  Google developer settings for your web-app in the "REDIRECT URIS" setting
                CallbackPath = new PathString("/umbraco-google-signin"),

                Provider = new GoogleOAuth2AuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        var emailParts = context.Email.Split('@');
                        if (emailParts.Last() == "umbraco.dk" || emailParts.Last() == "umbraco.com")
                            // All good, return as usual
                            return Task.FromResult(0);

                        // Whoa, this one doesn't belong in our backoffice, throw exception, this will return a nulled out Auth Ticket
                        var errorMessage = string.Format("User tried to log in with {0}, which does not end with an accepted domain name.", context.Email);
                        throw new AuthenticationException(errorMessage);
                    }
                }
            };
            googleOptions.ForUmbracoBackOffice(style, icon);
            googleOptions.Caption = caption;

            googleOptions.SetExternalSignInAutoLinkOptions(
                new ExternalSignInAutoLinkOptions(
                    autoLinkExternalAccount: true, 
                    defaultUserType: "admin", 
                    defaultAllowedSections: new[] { "content", "media", "settings", "developer", "users", "member" }));

            app.UseGoogleAuthentication(googleOptions);
        }
    }
}