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
            throw new NotImplementedException();
        }

        public TweetOpinion GetTweetOpinion(string tweetId)
        {
            throw new NotImplementedException();
        }

        public void AddSubjectTweets(SubjectTweets subjectTweets)
        {
            throw new NotImplementedException();
        }

        public SubjectTweets GetSubjectTweets(string subject)
        {
            throw new NotImplementedException();
        }
    }
}