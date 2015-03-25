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
            // TODO: use storage to decide if we should go to api
            var tweets = new List<Tweet>();
            tweets.AddRange(GetFromApi(subject));
            return tweets;
        }

        public IEnumerable<Tweet> GetFromApi(string subject)
        {
            var source = FolksOpinionTwitterApi.Instance;
            return source.GetTweets(subject);
        }

        public IEnumerable<Tweet> GetFromStorage(string subject)
        {
            throw new NotImplementedException();
        }

        private void SaveToStorage(SubjectTweets subjectTweets)
        {
            throw new NotImplementedException();
        }
    }
}