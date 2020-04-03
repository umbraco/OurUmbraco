using OurUmbraco.Our.Extensions;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Core;

namespace OurUmbraco.Auth
{
    
    /// <summary>
    /// HMAC Authentication utilities
    /// </summary>
    public static class HMACAuthentication
    {
        /// <summary>
        /// Validates the input token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestUrl"></param>
        /// <param name="apiKey"></param>
        /// <param name="timestamp"></param>
        public static bool ValidateToken(string token, string requestUrl, ProjectAuthKey apiKey, out DateTime timestamp)
        {
            if (apiKey is null) throw new ArgumentNullException(nameof(apiKey));

            //assign MinValue to expire immediately if the TryParse were to fail
            timestamp = DateTime.MinValue;

            if (!apiKey.IsEnabled)
                return false;

            var decodedToken = GetDecodedToken(token, requestUrl);
            
            double timeStampDouble;

            if (double.TryParse(decodedToken.Timestamp, NumberStyles.Any, CultureInfo.InvariantCulture, out timeStampDouble))
            {
                timestamp = timeStampDouble.FromUnixTime();
            }

            //generate a signature and verify it matches the token signature
            var validationSignature = GetSignature(requestUrl, decodedToken.Timestamp, decodedToken.Nonce, apiKey.AuthKey);
            if (validationSignature != decodedToken.RequestSignature)
                return false;

            return true;
        }

        private static DecodedToken GetDecodedToken(string token, string requestUrl)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            if (string.IsNullOrWhiteSpace(requestUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(requestUrl));

            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            //try getting the authorization header
            var headerParts = decoded.Split(':');
            if (headerParts.Length != 3)
                throw new InvalidOperationException("Authorization header is invalid");

            var decodedToken = new DecodedToken
            {
                RequestSignature = headerParts[0],
                Nonce = headerParts[1],
                Timestamp = headerParts[2]
            };

            return decodedToken;
        }

        private static string GetSignature(string requestUri, string timestamp, string nonce, string secret)
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);

            using (var hmac = new HMACSHA256(secretBytes))
            {
                var signatureString = $"{requestUri}{timestamp}{nonce}";
                var signatureBytes = Encoding.UTF8.GetBytes(signatureString);
                var computedHashBytes = hmac.ComputeHash(signatureBytes);
                var computedString = Convert.ToBase64String(computedHashBytes);
                return computedString;
            }
        }

        private class DecodedToken
        {
            public string RequestSignature { get; set; }
            public string Nonce { get; set; }
            public string Timestamp { get; set; }
        }
    }
}
