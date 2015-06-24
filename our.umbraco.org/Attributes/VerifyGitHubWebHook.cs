using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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


        //http://stackoverflow.com/questions/4390543/facebook-real-time-update-validating-x-hub-signature-sha1-signature-in-c-sharp#13857878
        public bool IsValidToken(string payloadToken, HttpRequestMessage request)
        {
            //Get token stored on enviroment
            var serverToken = Environment.GetEnvironmentVariable("githubToken");

            //Need to get the actual content of the request - JSON payload
            //As this payload is signed/encoded with our key
            var jsonPayload = request.Content.ToString();

            //Sign our payload we have got with our secret
            var hmac = SignWithHmac(UTF8Encoding.UTF8.GetBytes(jsonPayload), UTF8Encoding.UTF8.GetBytes(serverToken));
            
            //Do the conversion
            var hmacHex = ConvertToHexadecimal(hmac);

            //Check what we have signed is what is sent as header
            //Token example: sha1=fooBarLongEncodedTokenHere
            //Split on the equal as we not worried about the sha1= stuff
            bool isValid = payloadToken.Split('=')[1] == hmacHex;

            return isValid;
        }

        private static byte[] SignWithHmac(byte[] dataToSign, byte[] keyBody)
        {
            using (var hmacAlgorithm = new System.Security.Cryptography.HMACSHA1(keyBody))
            {
                return hmacAlgorithm.ComputeHash(dataToSign);
            }
        }

        private static string ConvertToHexadecimal(IEnumerable<byte> bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

    }

}
