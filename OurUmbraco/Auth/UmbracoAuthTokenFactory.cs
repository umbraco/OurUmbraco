using System;
using System.Collections.Generic;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace OurUmbraco.Auth
{
    public class UmbracoAuthTokenFactory
    {
        private string _secretKey;

        public UmbracoAuthTokenFactory()
        {
            _secretKey = new UmbracoAuthTokenSecret().GetSecret();
        }
        
        /// <summary>
        /// This generates a JWT Auth Token and sets as an PetaPoco object to save to DB
        /// </summary>
        /// <param name="authToken">Pass in existing params on authToken object such as identity id & type</param>
        /// <returns>A new authToken object with the date and actual token data in</returns>
        public UmbracoAuthToken GenerateAuthToken(UmbracoAuthToken authToken)
        {
            //Date Time
            var dateCreated = DateTime.UtcNow;
            var dateCreatedToString = dateCreated.ToString("u");

            var payload = new Dictionary<string, object>
            {
                { "date_created", dateCreatedToString },
                { "member_id", authToken.MemberId },
                { "project_id", authToken.ProjectId }
            };
            var secret = _secretKey;

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var encodedToken = encoder.Encode(payload, secret);

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
        public UmbracoAuthToken DecodeUserAuthToken(string jwtToken)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);

            var userAuth = decoder.DecodeToObject<UmbracoAuthToken>(jwtToken, _secretKey, verify: true);

            // If we have not thrown - assign it back to object
            userAuth.AuthToken = jwtToken;

            //Return the object
            return userAuth;
        }
    }
}
