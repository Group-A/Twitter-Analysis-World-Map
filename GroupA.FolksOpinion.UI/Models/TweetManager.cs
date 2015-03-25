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
            throw new NotImplementedException();
        }

        protected TweetManager() { }

        public IEnumerable<Tweet> GetTweets(string subject)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tweet> GetFromApi(string subject)
        {
            throw new NotImplementedException();
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