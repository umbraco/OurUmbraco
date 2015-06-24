using System;
using System.Collections.Generic;
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
                else
                {
                    //Return a HTTP 200 OK
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.OK);
                }

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
        /// <param name="body"></param>
        /// <param name="signature"></param>sB
        /// <returns></returns>
        private bool IsValidToken(string payloadToken, HttpRequestMessage request)
        {
            //Get token stored on enviroment
            var serverToken = Environment.GetEnvironmentVariable("githubToken");
            //var serverToken = "superSecretFOO";

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
