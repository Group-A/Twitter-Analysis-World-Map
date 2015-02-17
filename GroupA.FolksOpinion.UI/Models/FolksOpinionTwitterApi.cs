/* File:        FolksOpinionTwitterapi.cs
 * Purpose:     Specialised TwitterApi class for the FolksOpinion application.
 * Version:     1.1
 * Created:     9th February 2015
 * Author:      Gary Fernie
 * Exposes:     FolksOpinionTwitterApi
 * Base:        TwitterApi
 * 
 * Description: - Extends from the TwitterApi class.
 *              - Loads keys from application config file.
 *              - Makes key obfuscation checks.
 *              
 * Changes:     17th February 2015, 1.1
 *              - Added GetTweets method.
 *              
 * Bugs:        - Config loading code does not appear to access app's Web.config file.
 */

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;

namespace GroupA.FolksOpinion.UI.Models
{
    public class FolksOpinionTwitterApi : TwitterApi
    {
        public FolksOpinionTwitterApi()
        {
            var keysLoaded = LoadKeysFromConfig();
            if (keysLoaded)
            {
                // Encode credentials and get token.
                bearerTokenCredentialsBase64Encoded = EncodeConsumerKeyAndSecret(consumerKey, consumerSecret);
                bearerToken = GetBearerToken(bearerTokenCredentialsBase64Encoded);
            }
        }

        /* Returns a colection of Tweets matching a search term. */
        public IEnumerable<Tweet> GetTweets (string searchTerm)
        {
            return JsonConvert.DeserializeObject<IEnumerable<Tweet>>(GetTweetsJson(searchTerm));
        }

        /* Loads keys from config file.
         * Checks if keys are valid. 
         * Returns results of validation.
         * Will not retain invalid keys; Returns before saving keys to object.
         */
        private bool LoadKeysFromConfig()
        {
            // Load keys.
            // TODO: Config access broken; Will retrieve null.
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