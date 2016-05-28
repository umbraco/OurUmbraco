using System;
using System.Configuration;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core.Logging;

namespace OurUmbraco.Documentation.Controllers
{
    public class AppVeyorAuthorizeFilterAttribute : AuthorizeAttribute
    {
        
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authorization = actionContext.Request.Headers.Authorization;
            if (authorization == null)
            {
                LogHelper.Info<AppVeyorAuthorizeFilterAttribute>("Authorization headers not found, access denied");
                return false;
            }
            
            if (authorization.Scheme != "Basic")
            {
                LogHelper.Info<AppVeyorAuthorizeFilterAttribute>("Authorization scheme is not basic, access denied");
                return false;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                LogHelper.Info<AppVeyorAuthorizeFilterAttribute>("Authorization parameter is empty, access denied");
                return false;
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization.Parameter));
                if (string.IsNullOrEmpty(decoded))
                {
                    LogHelper.Info<AppVeyorAuthorizeFilterAttribute>("Authorization parameter can't be decoded, access denied");
                    return false;
                }

                if (decoded != ConfigurationManager.AppSettings["AppVeyorWebHookAuthKey"])
                {
                    LogHelper.Info<AppVeyorAuthorizeFilterAttribute>(string.Format("Authorization parameter and configured secret don't match {0} vs {1}, access denied", decoded, ConfigurationManager.AppSettings["AppVeyorWebHookAuthKey"]));
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                LogHelper.Error<AppVeyorAuthorizeFilterAttribute>("Authorization error", exception);
                return false;
            }
        }
    }
}