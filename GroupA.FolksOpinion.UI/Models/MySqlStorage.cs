/* File:        MySqlStorage.cs
 * Purpose:     
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     MySqlStorage
 *
 * Description: 
 * 
 * Changes:     17th February 2015, Gary Fernie
 *              - Changed to use new opinion entities.
 *              - Stubbed to allow build.
 *              25th February 2015, Jamie Aitken
 *              - Fleshed out stubs
 *              5th March 2015, Michael Rodenhurst
 *              - Changed everything
 *              27th March 2015, Michael Rodenhurst
 *              - Changed everything. Aren't these comments helpful?
 *              
 * Issues:      - 
 */

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public partial class MySqlStorage : IFolksOpinionStorage
    {
        MySqlInterface sql;

        public MySqlStorage()
        {
            /* Get SQL config variables */
            String host = ConfigurationManager.AppSettings["MySQL_CacheHost"];
            String database = ConfigurationManager.AppSettings["MySQL_CacheDatabase"];
            String user = ConfigurationManager.AppSettings["MySQL_CacheUser"];
            String password = ConfigurationManager.AppSettings["MySQL_CachePassword"];

            /* Initialise SQL interface */
            sql = new MySqlInterface(host, database, user, password);
        }

        public void AddTweetOpinion(TweetOpinion tweetOpinion)
        {
            /* Insert Tweet and Opinion, returning their id's */
            int tweet_id = InsertTweet(tweetOpinion.Tweet);
            int opinion_id = InsertOpinion(tweetOpinion.Opinion);

            /* Buid TweetOpinion table set, and insert object */
            List<Tuple<string, string>> tweetopinion_columns = new List<Tuple<string, string>>();
            tweetopinion_columns.Add(new Tuple<string, string>("tweet", "" + tweet_id));
            tweetopinion_columns.Add(new Tuple<string, string>("opinion", "" + opinion_id));

            sql.InsertRow("TweetOpinion", tweetopinion_columns);
        }

        public TweetOpinion GetTweetOpinion(string tweetId)
        {
             int tweet_uid = -1;

            MySqlDataReader reader = sql.Select("tweet", "id_str = " + tweetId, null);
            if (reader.HasRows)
                tweet_uid = reader.GetInt32(0); // Fixme: lazy hack

            TweetOpinion tweet_opinion = new TweetOpinion();
            tweet_opinion.Tweet = GetTweet(reader);
            tweet_opinion.Opinion = GetOpinion(tweet_uid);

            return tweet_opinion;
        }

        public void AddSubjectTweets(SubjectTweets subjectTweets)
        {
            foreach(Tweet tweet in subjectTweets.Tweets)
            {
                int tweet_id = GetTweetID(tweet);

                /* Build table set and insert new row */
                List<Tuple<string, string>> subjecttweet_columns = new List<Tuple<string, string>>();
                subjecttweet_columns.Add(new Tuple<string, string>("subject", subjectTweets.Subject));
                subjecttweet_columns.Add(new Tuple<string, string>("tweet", "" + tweet_id));

                sql.InsertRow("SubjectTweet", subjecttweet_columns);
            }
        }

        public SubjectTweets GetSubjectTweets(string subject)
        {
            throw new NotImplementedException();
        }

        /* Private helper functions */

        private Tweet GetTweet(MySqlDataReader reader)
        {
            if (!reader.HasRows)
                return null;

            Tweet tweet = new Tweet();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                String name = reader.GetName(i);
                String val = reader.GetString(i);

                switch (name)
                {
                    case "id_str": tweet.id_str = val; break;
                    case "place": tweet.place = GetPlace(val); break;
                    case "text": tweet.text = val; break;
                }
            }

            return tweet;
        }

        private Place GetPlace(String country_code)
        {
            Place place = new Place();

            MySqlDataReader reader = sql.Select("place", "countryCode = " + country_code, null);

            if(reader.HasRows)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    String name = reader.GetName(i);
                    String val = reader.GetString(i);

                    switch (name)
                    {
                        case "countryCode": place.country_code = val; break;
                        case "name": place.country = val; break;
                    }
                }
            }

            return place;
        }

        private Opinion GetOpinion(int tweet_uid)
        {
            /* Fetch the TweetOpinion and retrieve the opinion database id first */
            MySqlDataReader reader = sql.Select("TweetOpinion", "tweet = " + tweet_uid, null);
            int opinion_uid = -1;

            if(reader.HasRows)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    String name = reader.GetName(i);
                    String val = reader.GetString(i);

                    switch (name)
                    {
                        case "opinion": opinion_uid = Convert.ToInt32(val); break;
                    }
                }
            }

            if (opinion_uid < 0)
                return null;

            /* Get the opinion */
            Opinion opinion = new Opinion();

            reader = sql.Select("Opinion", "id = " + opinion_uid, null);

            if(reader.HasRows)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    String name = reader.GetName(i);
                    String val = reader.GetString(i);

                    switch (name)
                    {
                        case "posBias": opinion.PositiveBias = Convert.ToDouble(val); break;
                        case "negBias": opinion.NegativeBias = Convert.ToDouble(val); break;
                    }
                }
            }

            return opinion;
        }

        /* Fetches the PK id of a Tweet from the DB.
         * Inserts Tweet into DB if not already present */
        private int GetTweetID(Tweet tweet)
        {
            /* Fetch tweet from DB where id_str matches */
            List<string> column = new List<string>();
            column.Add("id");
            MySqlDataReader reader = sql.Select("Tweet", "id_str = " + tweet.id_str, column);

            /* Return ID if the reader contains the query result */
            if (reader.HasRows)
                return reader.GetInt32(0);

            /* If we get this far, InsertTweet and return result */
            return InsertTweet(tweet);
        }

        /* Returns the ID of the inserted tweet */
        private int InsertTweet(Tweet tweet)
        {
            string place = GetPlaceCountryCode(tweet.place); // Get countryCode for Place from DB

            /* Build table set and insert new row */
            List<Tuple<string, string>> tweet_columns = new List<Tuple<string, string>>();
            tweet_columns.Add(new Tuple<string, string>("created_at", DateTime.Now.ToString()));
            tweet_columns.Add(new Tuple<string, string>("id_str", tweet.id_str));
            tweet_columns.Add(new Tuple<string, string>("place", place));
            tweet_columns.Add(new Tuple<string, string>("text", tweet.text));

            sql.InsertRow("Tweet", tweet_columns);

            /* Get ID of row we just inserted. Note: Probably not thread safe */
            List<string> column = new List<string>();
            column.Add("MAX(id)");
            MySqlDataReader reader = sql.Select("Tweet", null, column);

            return reader.GetInt32(0); // TODO: null handling
        }

        /* Returns the ID of the inserted opinion */
        private int InsertOpinion(Opinion opinion)
        {
            List<Tuple<string, string>> opinion_columns = new List<Tuple<string, string>>();
            opinion_columns.Add(new Tuple<string, string>("posBias", "" + opinion.PositiveBias));
            opinion_columns.Add(new Tuple<string, string>("negBias", "" + opinion.NegativeBias));

            sql.InsertRow("Opinion", opinion_columns);

            /* Get ID of row we just inserted. Note: Probably not thread safe */
            List<string> column = new List<string>();
            column.Add("MAX(id)");
            MySqlDataReader reader = sql.Select("Opinion", null, column);

            return reader.GetInt32(0);
        }

        /* Simply returns countrycode of place. Inserting place into DB if required */
        private string GetPlaceCountryCode(Place place)
        {
            /* Check if place already exists in database */
            List<string> column = new List<string>();
            column.Add("countryCode");
            MySqlDataReader reader = sql.Select("Place", "countryCode=" + place.country_code, column);

            if (reader.HasRows)
                return reader.GetString(0);

            /* Entry not found. Create place entry in database */
            List<Tuple<string, string>> place_columns = new List<Tuple<string, string>>();
            place_columns.Add(new Tuple<string, string>("name", place.country));
            place_columns.Add(new Tuple<string, string>("countryCode", place.country_code));

            sql.InsertRow("Place", place_columns);

            return place.country_code; // FIXME: lazy and bad for obvious reasons
        }
    }
}
