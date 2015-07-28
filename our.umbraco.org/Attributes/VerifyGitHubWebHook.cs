using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace our.Attributes
{
    public class VerifyGitHubWebHook : ActionFilterAttribute
    {
        /// <summary>
        /// When the attribute is decorated on an Umbraco WebApi Controller
        /// First check payload was posted from GitHub & not spoofed
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            try
            {
                //HTTP Request
                var request = actionContext.Request;

                //Get the auth header that
                var authHeader = request.Headers.GetValues("X-Hub-Signature").FirstOrDefault();

                //If auth header does not exist - unathorised
                if (authHeader == null)
                {
                    //Return a HTTP 401 Unauthorised header
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }


                //Now we have the auth header value - try & decode to verify against env variable
                if (!IsValidToken(authHeader, request))
                {
                    //Return a HTTP 401 Unauthorised header
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                
                //All is good - do not need to set response 200 header here
                //As we now need to carry on running our API method

            }
            catch (Exception)
            {
                //Return a HTTP 401 Unauthorised header
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            //Continue as normal
            base.OnActionExecuting(actionContext);
        }

        /// <summary>
        /// http://chris.59north.com/post/Integrating-with-Github-Webhooks-using-OWIN
        /// </summary>
        /// <returns></returns>
        private bool IsValidToken(string payloadToken, HttpRequestMessage request)
        {
            //Get token stored in appsetting
            var serverToken = ConfigurationManager.AppSettings["gitHubWebHookSecret"];

            //Need to get the actual content of the request - JSON payload
            //As this payload is signed/encoded with our key
            var jsonPayload = request.Content.ReadAsStringAsync().Result;

            //Verify the payloadToken starts with sha1
            var vals = payloadToken.Split('=');
            if (vals[0] != "sha1")
            {
                return false;
            }
                

            var encoding = new System.Text.ASCIIEncoding();
            var keyByte = encoding.GetBytes(serverToken);

            var hmacsha1 = new HMACSHA1(keyByte);

            var messageBytes = encoding.GetBytes(jsonPayload);
            var hashmessage = hmacsha1.ComputeHash(messageBytes);
            var hash = hashmessage.Aggregate("", (current, t) => current + t.ToString("X2"));

            return hash.Equals(vals[1], StringComparison.OrdinalIgnoreCase);
        }
    }

}
