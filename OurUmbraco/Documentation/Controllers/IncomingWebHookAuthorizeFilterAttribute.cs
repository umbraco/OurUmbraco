using System;
using System.Configuration;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core.Logging;

namespace OurUmbraco.Documentation.Controllers
{
    /// <summary>
    /// This is used to verify incoming Webhook payloads are from us by verifying the BasicAuth header key/secret
    /// Currently we only use this with AzureDevOps to be notified when the static site docs are built
    /// In order to download the build artifact ZIPs needed
    /// </summary>
    public class IncomingWebHookAuthorizeFilterAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authorization = actionContext.Request.Headers.Authorization;
            if (authorization == null)
            {
                LogHelper.Info<IncomingWebHookAuthorizeFilterAttribute>("Authorization headers not found, access denied");
                return false;
            }
            
            if (authorization.Scheme != "Basic")
            {
                LogHelper.Info<IncomingWebHookAuthorizeFilterAttribute>("Authorization scheme is not basic, access denied");
                return false;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                LogHelper.Info<IncomingWebHookAuthorizeFilterAttribute>("Authorization parameter is empty, access denied");
                return false;
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization.Parameter));
                if (string.IsNullOrEmpty(decoded))
                {
                    LogHelper.Info<IncomingWebHookAuthorizeFilterAttribute>("Authorization parameter can't be decoded, access denied");
                    return false;
                }

                if (decoded != ConfigurationManager.AppSettings["IncomingWebHookAuthKey"])
                {
                    LogHelper.Info<IncomingWebHookAuthorizeFilterAttribute>("Authorization parameter and configured secret don't match, access denied");
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error<IncomingWebHookAuthorizeFilterAttribute>("Authorization error", exception);
                return false;
            }
        }
    }
}