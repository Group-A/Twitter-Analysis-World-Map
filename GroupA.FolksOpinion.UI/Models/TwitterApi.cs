﻿/* File:        TwitterApi.cs
 * Purpose:     Provides functionality to issue application-only
 *              Oauth2 requests to the Twitter API.
 * Version:     1.4
 * Created:     3rd February 2015
 * Author:      Gary Fernie
 * Exposes:     TwitterApi, BearerTokenNotGrantedException
 * 
 * Description: - Encodes consumer key and secret.
 *              - Requests bearer token using encoded credentials.
 *              - Makes http API requests using bearer token.
 *              - Much of the class' functionality can be used in a static context
 * 
 * Changes:     9th February 2015, ver1.1
 *              - Removed application-specific code (generalised class).
 *              17th February 2015, ver 1.2
 *              - Added GetTweets method.
 *              18th February 2015, ver 1.3
 *              - Added more error checking to GetTweetsJson method.
 *              2nd March 2015, ver 1.4
 *              - Separated API requests and response error checking.
 *              - Added GET trends/place functionality.
 *              - Added response cache.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class TwitterApi
    {
        protected string consumerKey;
        protected string consumerSecret;
        protected string bearerTokenCredentialsBase64Encoded;
        protected string bearerToken;

        // Twitter API and token request details.
        private static string twitterUrl = "https://api.twitter.com";
        private static string oauthUrn = "/oauth2/token";
        private static string oathUri = twitterUrl + oauthUrn;
        private static string bearerTokenHeaderAuthorisationFormat = "Basic {0}";
        private static string bearerTokenHeaderAuthorisation;
        private static string bearerTokenHeaderContentType = "application/x-www-form-urlencoded";
        private static string bearerTokenRequestBody = "grant_type=client_credentials";

        // Memory cache for caching responses.
        private static ObjectCache cache = MemoryCache.Default;

        protected TwitterApi() { }
        
        public TwitterApi(string consumerKey, string consumerSecret)
        {
            var keysValid = ValidateKeys(consumerKey, consumerSecret);
            if (keysValid)
            {
                // Encode credentials and get token.
                bearerTokenCredentialsBase64Encoded = EncodeConsumerKeyAndSecret(consumerKey, consumerSecret);
                bearerToken = GetBearerToken(bearerTokenCredentialsBase64Encoded);
            }
        }

        /* Turn consumer key and consumer secret into encoded
         * bearer token credentials. 
         * Bearer token cedentials are required to exchange, with 
         * Twitter API, for a bearer token.
         */
        public static string EncodeConsumerKeyAndSecret(string consumerKey, string consumerSecret)
        {
            // Encode key and secret to RFC 1738 standard.
            // Dubious encoding method - may not be RFC 1738 compliant.
            // May cause problems later if key/secret format changes.
            var consumerKeyUrlEncoded = HttpUtility.UrlEncode(consumerKey);
            var consumerSecretUrlEncoded = HttpUtility.UrlEncode(consumerSecret);

            var bearerTokenCredentials = consumerKeyUrlEncoded + ":" + consumerSecretUrlEncoded;

            // Convert credentials string to bytes then re-encode as base64 string.
            var bearerTokenCredentialsBytes = System.Text.Encoding.UTF8.GetBytes(bearerTokenCredentials);
            return System.Convert.ToBase64String(bearerTokenCredentialsBytes);       
        }

        /* Requests bearer token from Twitter using encoded credentials.
         * Can throw BearerTokenNotGrantedException.
         */
        public static string GetBearerToken(string base64EncodedBearerTokenCredentials)
        {
            string bearerToken = null;
            
            // Build authorisation header using supplied credentials.
            bearerTokenHeaderAuthorisation = string.Format(
                bearerTokenHeaderAuthorisationFormat, base64EncodedBearerTokenCredentials);
            
            using (var client = new HttpClient())
            {
                // Build request message.
                var request = new HttpRequestMessage(HttpMethod.Post, oathUri);
                request.Headers.Add("Authorization", bearerTokenHeaderAuthorisation);
                request.Content = new StringContent(bearerTokenRequestBody, Encoding.UTF8, bearerTokenHeaderContentType);

                // Get response message by sending request.
                var response = client.SendAsync(request).Result;

                // Check if response is 200 code.
                // I.e. Doesn't actually check if bearer token was issued.
                if (!response.IsSuccessStatusCode)
                    throw new BearerTokenNotGrantedException(response.Content.ReadAsStringAsync().Result);

                // Validate and parse token from Json response.
                var bearerTokenResponse = response.Content.ReadAsStringAsync().Result;
                var bearerTokenResponseObject = JsonConvert.DeserializeObject<TwitterBearerTokenResponse>(bearerTokenResponse);
                if (bearerTokenResponseObject.token_type.Equals("bearer"))
                    bearerToken = bearerTokenResponseObject.access_token;
            }

            return bearerToken;
        }

        /* Make request to Twitter API for supplied resource.
         * Returns response body as string.
         * Resouce e.g.: "/1.1/statuses/user_timeline.json?count=100&screen_name=twitterapi"
         * Gets: https://api.twitter.com/1.1/statuses/user_timeline.json?count=100&screen_name=twitterapi
         */
        public static string MakeApiGetRequest(string bearerToken, string resource)
        {
            string response = "";
            if (bearerToken == null) return response;
            if (bearerToken.Equals("")) return response;

            using (var client = new HttpClient())
            {
                // Build request.
                var request = new HttpRequestMessage(HttpMethod.Get, twitterUrl + resource);
                request.Headers.Add("Authorization", "Bearer " + bearerToken);

                // Get response content.
                var responseMessage = client.SendAsync(request).Result;
                response = responseMessage.Content.ReadAsStringAsync().Result;
            }

            return response;
        }

        /* Uses this object's token. */
        public string MakeApiGetRequest(string resource)
        {
            return MakeApiGetRequest(bearerToken, resource);
        }

        /* Make request to Twitter API for supplied resource.
         * Returns Json response.
         * Uses MakeApiGetRequest.
         * Validates response, returns empty string if invalid or error.
         */
        public string GetApiResource(string resource)
        {
            // Get resource and validate.
            var response = MakeApiGetRequest(resource);
            if (response == null) return "";
            if (response.Equals("")) return "";

            // Check for errors and return results.
            dynamic responseObject = JObject.Parse(response);
            if (responseObject.errors == null)
                return response;
            else return "";
        }

        /* Gets Tweets matching a search term, using GetApiResource. */
        public string GetTweetsJson(string searchTerm)
        {
            return GetApiResource("/1.1/search/tweets.json?q=" + searchTerm + "&count=100");
        }

        /* Gets trends for a WOEID (location).
         * https://dev.twitter.com/rest/reference/get/trends/place
         * WEOID: http://zourbuth.com/tools/woeid/
         */
        public string GetTrendsForPlaceJson(int woeid)
        {
            return GetApiResource("/1.1/trends/place.json?id=" + woeid);
        }

        /* Gets global trends.
         * WOEID: 1 (global)
         */
        public string GetTrendsGlobalJson()
        {
            return GetTrendsForPlaceJson(1);
        }

        /* Checks if keys are null or empty.
         * Returns true if valid; false if invalid.
         */
        protected static bool ValidateKeys(string consumerKey, string consumerSecret)
        {
            if (consumerKey == null || consumerSecret == null) return false;
            if (consumerKey.Equals("") || consumerSecret.Equals("")) return false;

            // Tests passed, keys valid.
            return true;
        }

        /* Checks this object's keys. */
        protected bool ValidateKeys()
        {
            return ValidateKeys(consumerKey, consumerSecret);
        }

        /* Describes the Json object that Twitter sends 
         * in exchange for bearer token credentials. 
         */
        private struct TwitterBearerTokenResponse
        {
            public string token_type {get; set;}
            public string access_token {get; set;}
        }
    }

    [Serializable]
    public class BearerTokenNotGrantedException : Exception
    {
        public BearerTokenNotGrantedException() 
            : base() { }

        public BearerTokenNotGrantedException(string message)
            : base(message) { }

        public BearerTokenNotGrantedException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public BearerTokenNotGrantedException(string message, Exception innerException)
            : base(message, innerException) { }

        public BearerTokenNotGrantedException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected BearerTokenNotGrantedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}

