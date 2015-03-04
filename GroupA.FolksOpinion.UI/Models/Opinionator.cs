/* File:        Opinioniator.cs
 * Purpose:     Finds the world's opinion on a subject.
 * Created:     17th February 2015
 * Author:      Gary Fernie
 * Exposes:     Opinionator
 * 
 * Description: - Gets tweets on a given subject.
 *              - Finds each tweets opinion using lexical analysis.
 *              - Organises tweets into countries.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class Opinionator
    {
        public WorldSubjectOpinion WorldSubjectOpinion { get; private set; }

        public Opinionator() { }

        public Opinionator (string subject)
        {
            this.WorldSubjectOpinion = GetWorldOpinion(subject);
        }

        public WorldSubjectOpinion GetWorldOpinion (string subject)
        {
            return new WorldSubjectOpinion
            {
                Subject = subject,
                CountryOpinions = 
                    OpinionateCountries(OpinionateTweets(GetTweets(subject)))
            };
        }

        private IEnumerable<Tweet> GetTweets (string subject)
        {
            var source = new FolksOpinionTwitterApi();
            return source.GetTweets(subject);
        }

        private IEnumerable<TweetOpinion> OpinionateTweets (IEnumerable<Tweet> tweets)
        {
            var analyser = new TweetAnalyser();

            // Only tweets containing place information.
            var tweetsWithPlace = tweets.Where(t => t.place != null);

            foreach (var tweet in tweetsWithPlace)
                yield return analyser.AnalyseTweet(tweet);
        }

        private IEnumerable<CountryOpinion> OpinionateCountries (IEnumerable<TweetOpinion> tweetOpinions)
        {
            // Sort tweets into countries.
            var countryTweets = 
                from t in tweetOpinions
                group t by t.Tweet.place.country_code into country
                orderby country.Key
                select country;
            
            // Average opinions, return countries.
            foreach (var country in countryTweets)
            {
                List<double> countryPosBias = new List<double>();
                List<double> countryNegBias = new List<double>();

                foreach (var tweet in country)
                {
                    countryPosBias.Add(tweet.Opinion.PositiveBias);
                    countryNegBias.Add(tweet.Opinion.NegativeBias);
                }

                yield return new CountryOpinion
                {
                    Country = country.Key,
                    Opinion = new Opinion
                    {
                        PositiveBias = countryPosBias.Average(),
                        NegativeBias = countryNegBias.Average()
                    }
                };
            }
        }
    }
}