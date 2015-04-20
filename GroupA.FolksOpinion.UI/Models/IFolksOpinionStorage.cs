using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupA.FolksOpinion.UI.Models
{
    public interface IFolksOpinionStorage
    {
        void AddTweetOpinion(TweetOpinion tweetOpinion);
        TweetOpinion GetTweetOpinion(string tweetId);

        void AddSubjectTweets(SubjectTweets subjectTweets);
        SubjectTweets GetSubjectTweets(string subject);
    }
}
