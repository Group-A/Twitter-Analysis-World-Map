using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class StorageManager : IFolksOpinionStorage
    {
        private IFolksOpinionStorage _Store = new MySqlStorage();
        protected IFolksOpinionStorage Store 
        {
            get { return _Store; }
            set { _Store = value; }
        }

        public void AddTweetOpinion(TweetOpinion tweetOpinion)
        {
            Store.AddTweetOpinion(tweetOpinion);
        }

        public TweetOpinion GetTweetOpinion(string tweetId)
        {
            return Store.GetTweetOpinion(tweetId);
        }

        public void AddSubjectTweets(SubjectTweets subjectTweets)
        {
            Store.AddSubjectTweets(subjectTweets);
        }

        public SubjectTweets GetSubjectTweets(string subject)
        {
            return Store.GetSubjectTweets(subject);
        }
    }
}