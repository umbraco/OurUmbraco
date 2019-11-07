using System;
using System.Collections.Generic;
using System.Globalization;
using JWT;

namespace OurUmbraco.Auth
{
    public static class UmbracoAuthTokenFactory
    {
        static string _secretKey = UmbracoAuthTokenSecret.GetSecret();

        /// <summary>
        /// This generates a JWT Auth Token and sets as an PetaPoco object to save to DB
        /// </summary>
        /// <param name="authToken">Pass in existing params on authToken object such as identity id & type</param>
        /// <returns>A new authToken object with the date and actual token data in</returns>
        public static UmbracoAuthToken GenerateAuthToken(UmbracoAuthToken authToken)
        {
            //Date Time
            var dateCreated = DateTime.UtcNow;
            var dateCreatedToString = dateCreated.ToString("u");

            //Create JSON payload for JWT token
            var payload = new Dictionary<string, object>() {
                { "member_id", authToken.MemberId },
                { "project_id", authToken.ProjectId },
                { "date_created", dateCreatedToString }
            };

            //Encode the JWT token with JSON payload, algorithm & our secret in constant
            var encodedToken = JsonWebToken.Encode(payload, _secretKey, JwtHashAlgorithm.HS256);

            //Return same object we passed in (Now with Date Created & Token properties updated)
            authToken.DateCreated = dateCreated;
            authToken.AuthToken = encodedToken;

            //Return the updated object
            return authToken;
        }

        /// <summary>
        /// Takes in a JWT string to decode
        /// </summary>
        /// <param name="jwtToken">THe JWT string to decode to an authToken object</param>
        /// <returns>A decoded authToken object from the JWT</returns>
        public static UmbracoAuthToken DecodeUserAuthToken(string jwtToken)
        {
            //Object to return
            var userAuth = new UmbracoAuthToken();

            //Decode & verify token was signed with our secret
            var jsonPayload = JsonWebToken.DecodeToObject(jwtToken, _secretKey) as IDictionary<string, object>;

            //Just the presence of the token & being deserialised with correct SECRET key is a good sign
            if (jsonPayload != null)
            {
                //Do DateTime conversion from u type back into DateTime object
                DateTime dateCreated;
                DateTime.TryParseExact(jsonPayload["date_created"].ToString(), "u", null, DateTimeStyles.AdjustToUniversal, out dateCreated);

                //Get the details of the user from the JWT payload
                userAuth.MemberId = Convert.ToInt32(jsonPayload["member_id"]);
                userAuth.ProjectId = Convert.ToInt32(jsonPayload["project_id"]);
                userAuth.DateCreated = dateCreated;
                userAuth.AuthToken = jwtToken;
            }

            //Return the object
            return userAuth;
        }

    }
}
