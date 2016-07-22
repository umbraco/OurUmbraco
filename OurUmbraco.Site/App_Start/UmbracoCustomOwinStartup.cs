using Microsoft.Owin;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Security.Identity;
using OurUmbraco.Site;

//To use this startup class, change the appSetting value in the web.config called 
// "owin:appStartup" to be "UmbracoCustomOwinStartup"

[assembly: OwinStartup("UmbracoCustomOwinStartup", typeof(UmbracoCustomOwinStartup))]
namespace OurUmbraco.Site
{
    /// <summary>
    /// A custom way to configure OWIN for Umbraco
    /// </summary>
    /// <remarks>
    /// The startup type is specified in appSettings under owin:appStartup - change it to "UmbracoCustomOwinStartup" to use this class
    /// 
    /// This startup class would allow you to customize the Identity IUserStore and/or IUserManager for the Umbraco Backoffice
    /// </remarks>
    public class UmbracoCustomOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            //Configure the Identity user manager for use with Umbraco Back office

            // *** EXPERT: There are several overloads of this method that allow you to specify a custom UserStore or even a custom UserManager!            
            app.ConfigureUserManagerForUmbracoBackOffice(
                ApplicationContext.Current,
				//The Umbraco membership provider needs to be specified in order to maintain backwards compatibility with the 
				// user password formats. The membership provider is not used for authentication, if you require custom logic
				// to validate the username/password against an external data source you can create create a custom UserManager
				// and override CheckPasswordAsync
                global::Umbraco.Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider().AsUmbracoMembershipProvider());
            
            //Ensure owin is configured for Umbraco back office authentication
            app
                .UseUmbracoBackOfficeCookieAuthentication(ApplicationContext.Current)
                .UseUmbracoBackOfficeExternalCookieAuthentication(ApplicationContext.Current);

            /* 
             * Configure external logins for the back office:
             * 
             * Depending on the authentication sources you would like to enable, you will need to install 
             * certain Nuget packages. 
             * 
             * For Google auth:					Install-Package UmbracoCms.IdentityExtensions.Google
             * For Facebook auth:					Install-Package UmbracoCms.IdentityExtensions.Facebook
             * For Microsoft auth:					Install-Package UmbracoCms.IdentityExtensions.Microsoft
             * For Azure ActiveDirectory auth:		Install-Package UmbracoCms.IdentityExtensions.AzureActiveDirectory
             * 
             * There are many more providers such as Twitter, Yahoo, ActiveDirectory, etc... most information can
             * be found here: http://www.asp.net/web-api/overview/security/external-authentication-services
             * 
             * For sample code on using external providers with the Umbraco back office, install one of the 
             * packages listed above to review it's code samples 
             *  
             */

            /*
             * To configure a simple auth token server for the back office:
             *             
             * By default the CORS policy is to allow all requests
             * 
             *      app.UseUmbracoBackOfficeTokenAuth(new BackOfficeAuthServerProviderOptions());
             *      
             * If you want to have a custom CORS policy for the token server you can provide
             * a custom CORS policy, example: 
             * 
             *      app.UseUmbracoBackOfficeTokenAuth(
             *          new BackOfficeAuthServerProviderOptions()
             *              {
             *             		//Modify the CorsPolicy as required
             *                  CorsPolicy = new CorsPolicy()
             *                  {
             *                      AllowAnyHeader = true,
             *                      AllowAnyMethod = true,
             *                      Origins = { "http://mywebsite.com" }                
             *                  }
             *              });
             */
        }
    }
}
