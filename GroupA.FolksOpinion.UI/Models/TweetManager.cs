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

            // Tweet created_at age threshold (10 minutes)
            var tweetAgeThreshold = new TimeSpan(0, 10, 0);

            // Check storage
            var storageTweets = (List<Tweet>) GetFromStorage(subject);
            tweets.AddRange(storageTweets);

            // Check Tweets for age
            if (storageTweets.Count > 0)
            {
                var now = DateTimeOffset.Now;
                var latestTweetDate = tweets
                    .Where(t => t.CreatedAt == tweets.Max(x => x.CreatedAt))
                    .FirstOrDefault()
                    .CreatedAt;
                bool tweetsAreUpToDate = (now - latestTweetDate) < tweetAgeThreshold;
                if (tweetsAreUpToDate) return tweets;
            }

            // Get new Tweets from API
            // TODO: get only new tweets
            var apiTweets = (List<Tweet>) GetFromApi(subject);
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

        public IEnumerable<Tweet> GetFromApi(string subject)
        {
            var source = FolksOpinionTwitterApi.Instance;
            return source.GetTweets(subject);
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