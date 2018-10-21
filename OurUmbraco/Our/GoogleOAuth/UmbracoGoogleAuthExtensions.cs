using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web.Hosting;
using Microsoft.Owin;
using Microsoft.Owin.Security.Google;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Logging;
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
            var whitelist = GetWhitelist();

            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                //In order to allow using different google providers on the front-end vs the back office,
                // these settings are very important to make them distinguished from one another.
                SignInAsAuthenticationType = Umbraco.Core.Constants.Security.BackOfficeExternalAuthenticationType,
                //  By default this is '/signin-google', you will need to change that default value in your
                //  Google developer settings for your web-app in the "REDIRECT URIS" setting
                CallbackPath = new PathString("/umbraco-google-signin"),
                
                Provider = new GoogleOAuth2AuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        if(whitelist.Contains(context.Email))
                            return Task.FromResult(0);

                        // Whoa, this one doesn't belong in our backoffice, throw exception, this will return a nulled out Auth Ticket
                        var errorMessage = $"User tried to log in with {context.Email}, which does not exist in the whitelist.";
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

        public static List<string> GetWhitelist()
        {
            try
            {
                var whitelistPath = HostingEnvironment.MapPath("~/config/authwhitelist.txt");
                if(whitelistPath == null)
                    return new List<string>();

                if (File.Exists(whitelistPath) == false)
                {
                    LogHelper.Debug(typeof(UmbracoGoogleAuthExtensions), "Whitelist file was not found: " + whitelistPath);
                    return new List<string>();
                }
            
                var whitelist = File.ReadAllLines(whitelistPath).Where(x => x.Trim() != "").Distinct().ToArray();
                return whitelist.ToList();
            }
            catch (Exception e)
            {
                LogHelper.Error(typeof(UmbracoGoogleAuthExtensions), "Error reading whitelist", e);
                return new List<string>();
            }
        }
    }
}
