using System;
using System.Configuration;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace OurUmbraco.Documentation.Controllers
{
    public class AppVeyorAuthorizeFilterAttribute : AuthorizeAttribute
    {
        
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authorization = actionContext.Request.Headers.Authorization;
            if (authorization == null) return false;
            
            if (authorization.Scheme != "Basic")
            {
                return false;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                return false;
            }

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization.Parameter));
                if (string.IsNullOrEmpty(decoded))
                {
                    return false;
                }

                if (decoded != ConfigurationManager.AppSettings["AppVeyorWebHookAuthKey"])
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}