using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public partial class TweetManager
    {
        public SubjectTweets SubjectTweets { get; protected set; }

        public TweetManager(string subject)
        {
            throw new NotImplementedException();
        }

        protected TweetManager() { }

        protected SubjectTweets GetTweets(string subject)
        {
            throw new NotImplementedException();
        }

        private void RetrieveFromApi()
        {
            throw new NotImplementedException();
        }
    }

    public partial class TweetManager
    {

    }
}