using System;
using System.Configuration;
using System.Web.Security;

namespace OurUmbraco.Auth
{
    public class UmbracoAuthTokenSecret
    {
        private const string SecretEnvVariable = "Umbraco.AuthToken";

        /// <summary>
        /// Goes & fetchs the secret from the Machine Environment Variables
        /// </summary>
        /// <returns>Returns the string secret</returns>
        public string GetSecret()
        {
            var secret = ConfigurationManager.AppSettings[SecretEnvVariable];

            //If it does not exist or is null/empty then we set a new one
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("The app setting <add key=\"Umbraco.AuthToken\" value=\"#{PackageAuthToken}#\" /> is missing from your web.config");
            }

            //Return the secret
            return secret;
        }
    }
}
