/* File:        FolksOpinionTwitterapi.cs
 * Purpose:     Specialised TwitterApi class for the FolksOpinion application.
 * Created:     9th February 2015
 * Author:      Gary Fernie
 * Exposes:     FolksOpinionTwitterApi
 * Base:        TwitterApi
 * 
 * Description: - Extends from the TwitterApi class.
 *              - Loads keys from application config file.
 *              - Makes key obfuscation checks.
 *              
 * Changes:     17th February 2015
 *              - Added GetTweets method.
 *              17th February 2015
 *              - Changed GetTweets method to better handle responses.
 *              - Config bug seems resolved.
 *              18th February 2015
 *              - Added functionality to perform multiple consecutive
 *                  searches to amass more subject Tweets.
 *              3rd March 2015
 *              - Validation improvements.
 *              - Get trends functionality.
 *              9th March 2015
 *              - Implemented singleton pattern.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;

namespace GroupA.FolksOpinion.UI.Models
{
    public sealed class FolksOpinionTwitterApi : TwitterApi
    {
        // Singleton instance.
        private static readonly FolksOpinionTwitterApi instance = new FolksOpinionTwitterApi();
        
        private FolksOpinionTwitterApi()
        {
            var keysLoaded = LoadKeysFromConfig();
            if (keysLoaded)
            {
                // Encode credentials and get token.
                bearerTokenCredentialsBase64Encoded = EncodeConsumerKeyAndSecret(consumerKey, consumerSecret);
                bearerToken = GetBearerToken(bearerTokenCredentialsBase64Encoded);
            }
        }

        // Gets a singleton instance of the class.
        public static FolksOpinionTwitterApi Instance
        {
            get { return instance; }
        }

        /* Returns a colection of Tweets matching a search term. */
        public IEnumerable<Tweet> GetTweets(string searchTerm, string sinceId = "0")
        {
            // Try to get Tweets
            var tweetsJson = GetTweetsJson(searchTerm, sinceId);
            if (string.IsNullOrEmpty(tweetsJson))
                return new List<Tweet>();

            // Deserialise Tweets
            var tweetSearchResponse = JsonConvert.DeserializeObject<GetSearchTweetsResponse>(tweetsJson);
            return tweetSearchResponse.statuses;
        }

        /* Returns a collection of trending topics for a given place. 
         * Default: worldwide (WOEID 1) 
         */
        public IEnumerable<Trend> GetTrends(int woeid = 1)
        {
            var trends = new List<Trend>();
            var trendsJson = GetTrendsJson(woeid);

            // Validate Json response.
            if (string.IsNullOrEmpty(trendsJson))
                return trends;

            // Parse Trends.
            // Trends responses are returned as a single cell JsonArray, for some reason.
            var trendsPlaceResponse = JsonConvert.DeserializeObject<List<GetTrendsPlaceResponse>>(trendsJson);
            trends.AddRange(trendsPlaceResponse[0].trends);

            return trends;
        }

        /* Loads keys from config file.
         * Checks if keys are valid. 
         * Returns results of validation.
         * Will not retain invalid keys; Returns before saving keys to object.
         */
        private bool LoadKeysFromConfig()
        {
            // Load keys.
            var newConsumerKey = ConfigurationManager.AppSettings["TwitterApiConsumerKey"];
            var newConsumerSecret = ConfigurationManager.AppSettings["TwitterApiConsumerSecret"];

            // Validate loaded keys.
            var keysValid = ValidateKeys(newConsumerKey, newConsumerSecret);
            if (!keysValid)
                return false;

            consumerKey = newConsumerKey;
            consumerSecret = newConsumerSecret;
            return true;
        }

        /* Checks if keys are null, empty or obfuscated.
         * Keys are obfuscated in public repo.
         * Returns true if valid; false if invalid.
         */
        protected new bool ValidateKeys(string consumerKey, string consumerSecret)
        {
            if (!TwitterApi.ValidateKeys(consumerKey, consumerSecret)) return false;
            if (consumerKey.Equals("OBFUSCATED") || consumerSecret.Equals("OBFUSCATED")) return false;

            // Tests passed, keys valid.
            return true;
        }
    }
}
