/* File:        TweetAnalyser.cs
 * Purpose:     Analyses a Tweet for opinion.
 * Created:     17th February 2015
 * Author:      Gary Fernie
 * Exposes:     TweetAnalyser
 * 
 * Description: - Uses a given implementation of lexical bias analysis.
 *              - Has a set default analyser.
 *              - Extracts the text from tweets to get Opinion.
 */

namespace GroupA.FolksOpinion.UI.Models
{
    public class TweetAnalyser
    {
        protected ILexicalBiasAnalyser analyser = new CumulativeBiasAnalyser();
        public TweetOpinion TweetOpinion { get; private set; }

        public TweetAnalyser() { }
        
        public TweetAnalyser (Tweet tweet)
        {
            this.TweetOpinion = AnalyseTweet(tweet);
        }

        public TweetAnalyser(Tweet tweet, ILexicalBiasAnalyser analyser)
        {
            this.analyser = analyser;
            this.TweetOpinion = AnalyseTweet(tweet);
        }

        public TweetOpinion AnalyseTweet (Tweet tweet)
        {
            return new TweetOpinion
            {
                Tweet = tweet,
                Opinion = analyser.Analyse(tweet.text)
            };
        }
    }
}