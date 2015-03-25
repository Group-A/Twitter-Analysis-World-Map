using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class StorageManager : IFolksOpinionStorage
    {
        protected IFolksOpinionStorage Store { get; set; }

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