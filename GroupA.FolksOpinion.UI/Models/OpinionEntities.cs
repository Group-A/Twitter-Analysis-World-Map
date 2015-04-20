﻿/* File:        OpinionEntities.cs
 * Purpose:     Represents the various opinion entities in our app.
 * Created:     17th February 2015
 * Author:      Gary Fernie
 * Exposes:     Opinion, TweetOpinion, CountryOpinion, WorldSubjectOpinion, SubjectTweets
 * 
 * Description: - Various classes to represent entities.
 *              - Models various opinion states.
 */

using System.Collections.Generic;

namespace GroupA.FolksOpinion.UI.Models
{
    public class Opinion
    {
        public double PositiveBias { get; set; }
        public double NegativeBias { get; set; }
    }

    public class TweetOpinion
    {
        public Tweet Tweet { get; set; }
        public Opinion Opinion { get; set; }
    }

    public class CountryOpinion
    {
        public string Country { get; set; }
        public Opinion Opinion { get; set; }
    }

    public class WorldSubjectOpinion
    {
        public string Subject { get; set; }
        public IEnumerable<CountryOpinion> CountryOpinions { get; set; }
    }

    public class SubjectTweets
    {
        public string Subject { get; set; }
        public IEnumerable<Tweet> Tweets { get; set; }
    }
}