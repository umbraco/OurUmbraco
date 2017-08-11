using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace OurUmbraco
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class VerifyTokenHeaderAttribute : AuthorizeAttribute
    {
        private readonly string _tokenKey;

        /// <summary>
        /// Verifies the "token" HTTP header against an appSettings value
        /// </summary>
        /// <param name="tokenKey">The appSettings key to verify the value of the token against</param>
        public VerifyTokenHeaderAttribute(string tokenKey)
        {
            _tokenKey = tokenKey;
        }

        protected override bool IsAuthorized(HttpActionContext context)
        {
            var request = context.Request;

            var tokenHeaders = Enumerable.Empty<string>();
            var tokenAppSetting = ConfigurationManager.AppSettings[_tokenKey];

            // get the token header values
            request.Headers.TryGetValues("token", out tokenHeaders);

            if (tokenHeaders.Any() && !string.IsNullOrWhiteSpace(tokenAppSetting))
            {
                // get the first token header in the list
                var header = tokenHeaders.FirstOrDefault();

                if (header.Equals(_tokenKey, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            return false;
        }
    }
}