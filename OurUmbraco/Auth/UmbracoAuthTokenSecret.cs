using System.Configuration;
using System.Web.Security;

namespace OurUmbraco.Auth
{
    public class UmbracoAuthTokenSecret
    {
        private const string SecretEnvVariable = "Umbraco.AuthToken";

        /// <summary>
        /// This sets the secret as an AppSetting
        /// </summary>
        /// <param name="secret">Secret string to set</param>
        public void SetSecret(string secret)
        {
            //TODO: Don't think we can set/add
            //For now ensure appSetting key exists with a value
            ConfigurationManager.AppSettings.Add(SecretEnvVariable, secret);
        }

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
                //Lets create a random strong password & set env variable
                secret = Membership.GeneratePassword(50, 5);

                //Set it as the Env Var
                SetSecret(secret);
            }

            //Return the secret
            return secret;
        }
    }
}
