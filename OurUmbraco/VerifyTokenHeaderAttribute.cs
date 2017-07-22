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

        public VerifyTokenHeaderAttribute(string tokenKey)
        {
            _tokenKey = tokenKey;
        }

        protected override bool IsAuthorized(HttpActionContext context)
        {
            var request = context.Request;

            var tokenHeaders = Enumerable.Empty<string>();
            var tokenAppSetting = ConfigurationManager.AppSettings[_tokenKey];

            request.Headers.TryGetValues("token", out tokenHeaders);

            if (tokenHeaders.Any() && !string.IsNullOrWhiteSpace(tokenAppSetting))
            {
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