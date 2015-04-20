using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class TweetManager
    {
        public SubjectTweets SubjectTweets { get; protected set; }

        public TweetManager(string subject)
        {
            SubjectTweets = new SubjectTweets
            {
                Subject = subject,
                Tweets = GetTweets(subject)
            };
        }

        protected TweetManager() { }

        public IEnumerable<Tweet> GetTweets(string subject)
        {
            var tweets = new List<Tweet>();
            var apiCallCountLimit = 5;
            var tweetAgeThreshold = new TimeSpan(0, 10, 0); //(10 minutes)
            var latestTweetId = "0"; // default

            // Check storage
            var storageTweets = (List<Tweet>) GetFromStorage(subject);
            tweets.AddRange(storageTweets);

            // Check Tweets for age
            if (storageTweets.Count > 0)
            {
                var now = DateTimeOffset.Now;
                var latestTweet = tweets
                    .Where(t => t.CreatedAt == tweets.Max(x => x.CreatedAt))
                    .FirstOrDefault();
                var latestTweetDate = latestTweet.CreatedAt;
                bool tweetsAreUpToDate = (now - latestTweetDate) < tweetAgeThreshold;
                if (tweetsAreUpToDate) return tweets;
                latestTweetId = latestTweet.id_str;
            }

            // Get new Tweets from API
            var apiTweets = new List<Tweet>();
            for (var i=0; i < apiCallCountLimit; i++)
            {
                var apiTweetsHaul = (List<Tweet>) GetFromApi(subject, latestTweetId);
                apiTweets.AddRange(apiTweetsHaul);

                // Stop if API is exhausted
                if (apiTweetsHaul.Count < 100) continue;

                // Find latest Tweet id, for next iteration, if required
                var apiCallCount = i + 1;
                if (apiCallCount >= apiCallCountLimit) break;
                latestTweetId = apiTweetsHaul
                    .Where(t => t.CreatedAt == apiTweetsHaul.Max(x => x.CreatedAt))
                    .FirstOrDefault()
                    .id_str;
            }
            tweets.AddRange(apiTweets);

            // Save new Tweets
            if (apiTweets.Count > 0)
            {
                var newTweets = new SubjectTweets
                {
                    Subject = subject,
                    Tweets = apiTweets
                };
                SaveToStorage(newTweets);
            }

            return tweets;
        }

        public IEnumerable<Tweet> GetFromApi(string subject, string sinceId = "0")
        {
            var source = FolksOpinionTwitterApi.Instance;
            return source.GetTweets(subject, sinceId);
        }

        public IEnumerable<Tweet> GetFromStorage(string subject)
        {
            var storage = new StorageManager();
            var subjectTweets = storage.GetSubjectTweets(subject);
            return subjectTweets.Tweets;
        }

        private void SaveToStorage(SubjectTweets subjectTweets)
        {
            var storage = new StorageManager();
            storage.AddSubjectTweets(subjectTweets);
        }
    }
}
