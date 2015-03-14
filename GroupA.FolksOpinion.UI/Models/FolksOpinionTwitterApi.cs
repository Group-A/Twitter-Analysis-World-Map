/* File:        FolksOpinionTwitterapi.cs
 * Purpose:     Specialised TwitterApi class for the FolksOpinion application.
 * Version:     1.5
 * Created:     9th February 2015
 * Author:      Gary Fernie
 * Exposes:     FolksOpinionTwitterApi
 * Base:        TwitterApi
 * 
 * Description: - Extends from the TwitterApi class.
 *              - Loads keys from application config file.
 *              - Makes key obfuscation checks.
 *              
 * Changes:     17th February 2015, ver 1.1
 *              - Added GetTweets method.
 *              17th February 2015, ver 1.2
 *              - Changed GetTweets method to better handle responses.
 *              - Config bug seems resolved.
 *              18th February 2015, ver 1.3
 *              - Added functionality to perform multiple consecutive
 *                  searches to amass more subject Tweets.
 *              3rd March 2015, ver 1.4
 *              - Validation improvements.
 *              - Get trends functionality.
 *              9th March 2015, ver 1.5
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
        
        private static int numTweetsToRetrieve = 100; // Tweets per request.
        private static int numRequestsToPerform = 10; // Requests per subject.
        
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
        // TODO: Refactor this mess.
        public IEnumerable<Tweet> GetTweets(string searchTerm)
        {
            var tweets = new List<Tweet>();
            var tweetsJson = GetTweetsJson(searchTerm);

            if (!string.IsNullOrEmpty(tweetsJson))
            {
                var tweetSearchResponse = JsonConvert.DeserializeObject<GetSearchTweetsResponse>(tweetsJson);
                tweets.AddRange(tweetSearchResponse.statuses);

                for (var i=1; i<numRequestsToPerform; i++)
                {
                    tweetsJson = GetApiResource("/1.1/search/tweets.json"
                        + tweetSearchResponse.search_metadata.next_results);
                    tweetSearchResponse = JsonConvert.DeserializeObject<GetSearchTweetsResponse>(tweetsJson);
                    try { tweets.AddRange(tweetSearchResponse.statuses); }
                    catch (System.ArgumentNullException) { break; }
                        
                }
            }
            
            return tweets;
        }

        /* Returns a collection of trending topics for a given place. */
        public IEnumerable<Trend> GetTrends(int woeid)
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

        /* Returns a collection of globally trending topics. 
         * WOEID: 1 (worldwide)
         */
        public IEnumerable<Trend> GetTrends()
        {
            return GetTrends(1);
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