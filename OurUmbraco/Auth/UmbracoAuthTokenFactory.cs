using System;
using System.Collections.Generic;
using System.Globalization;
using JWT;
using JWT.Algorithms;
using JWT.Builder;

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

            //Encode the JWT token with JSON payload, algorithm & our secret in constant
            var encodedToken = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(_secretKey)
                .AddClaim("date_created", dateCreatedToString)
                .AddClaim("member_id", authToken.MemberId)
                .AddClaim("project_id", authToken.ProjectId)
                .Build();

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
            //Decode & verify token was signed with our secret
            var userAuth = new JwtBuilder()
                .WithSecret(_secretKey)
                .MustVerifySignature() //Throws ex if not signed correctly or expired token
                .Decode<UmbracoAuthToken>(jwtToken);

            // If we have not thrown - assign it back to object
            userAuth.AuthToken = jwtToken;

            //Return the object
            return userAuth;
        }

    }
}
