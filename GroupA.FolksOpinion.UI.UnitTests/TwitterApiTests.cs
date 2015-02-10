using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GroupA.FolksOpinion.UI.Models;

namespace GroupA.FolksOpinion.UI.UnitTests
{
    [TestClass]
    public class TwitterApiTests
    {
        // Using Twitter API documentation example keys.
        // Will not authorise.
        private static string consumerKey = "xvz1evFS4wEEPTGEFPHBog";
        private static string consumerSecret = "L8qq9PZyRg6ieKGEKhZolGC0vJWLw8iEJ88DRdyOg";
        private static string consumerKeyUrlEncoded = "xvz1evFS4wEEPTGEFPHBog";
        private static string consumerSecretUrlEncoded = "L8qq9PZyRg6ieKGEKhZolGC0vJWLw8iEJ88DRdyOg";
        private static string bearerTokenCredentials = consumerKeyUrlEncoded + ":" + consumerSecretUrlEncoded;
        private static string bearerTokenCredentialsBase64Encoded = "eHZ6MWV2RlM0d0VFUFRHRUZQSEJvZzpMOHFxOVBaeVJnNmllS0dFS2hab2xHQzB2SldMdzhpRUo4OERSZHlPZw==";
        
        // Cases to induce error.
        string consumerKeyNull = null;
        string consumerSecretNull = null;
        string consumerKeyEmpty = "";
        string consumerSecretEmpty = "";
        string consumerKeyObfuscated = "OBFUSCATED";
        string consumerSecretObfuscated = "OBFUSCATED";
        
        [TestMethod]
        public void TestTwitterApiConstructor()
        {
            // Will not try to get keys - fails validation.
            TwitterApi tWithNull = new TwitterApi(consumerKeyNull, consumerSecretNull);
            TwitterApi tWithEmpty = new TwitterApi(consumerKeyEmpty, consumerSecretEmpty);

            // Will throw exception because of invalid keys.
            try { TwitterApi tWIthObfuscated = new TwitterApi(consumerKeyObfuscated, consumerSecretObfuscated); }
            catch (BearerTokenNotGrantedException) { }
            try { TwitterApi tWithKeys = new TwitterApi(consumerKey, consumerSecret); }
            catch (BearerTokenNotGrantedException) { }
        }

        [TestMethod]
        public void TestFolksOpinionTwitterApiConstructor()
        {
            FolksOpinionTwitterApi tWithConfig = new FolksOpinionTwitterApi();
        }

        [TestMethod]
        public void TestEncodeConsumerKeyAndSecret()
        {
            var k = TwitterApi.EncodeConsumerKeyAndSecret(consumerKey, consumerSecret);
            Assert.AreEqual(k, bearerTokenCredentialsBase64Encoded);
        }

        [TestMethod]
        public void TestApiRequest()
        {
            TwitterApi t = new FolksOpinionTwitterApi();
            var result = t.GetApiResource("/1.1/statuses/user_timeline.json?count=100&screen_name=twitterapi");
        }
    }
}
