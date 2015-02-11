using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace GroupA.FolksOpinion.UI.Models
{
    public class DatabaseModel
    {
        private String tweetID;
        private double analysisValue;
        private String coords;
        private String hashtag;
        MySqlConnection conn;

        public MySqlConnection connectToDB()
        {
            String myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;" + "pwd=12345;database=test;";
            try
            {
                conn = new MySqlConnection(myConnectionString);
                conn.Open();
                return conn;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public void cacheTweet(double value, String Coords, String hash)
        {
            MySqlCommand insertTweet = new MySqlCommand("INSERT INTO tweet(analysisVal) VALUES(@val0)", connectToDB());
            insertTweet.Parameters.AddWithValue("val", value);
            insertTweet.ExecuteNonQuery();
            MySqlCommand insertCoords = new MySqlCommand("INSERT INTO geocode(coords) VALUES(@val1)", connectToDB());
            insertCoords.Parameters.AddWithValue("val0", Coords);
            insertCoords.ExecuteNonQuery();
            MySqlCommand insertHash = new MySqlCommand("INSERT INTO entities(hashtag) VALUES(@val2)", connectToDB());
            insertHash.Parameters.AddWithValue("val2", hash);
            insertHash.ExecuteNonQuery();
        }

        public void displayTweets(String hash)
        {
            MySqlCommand displayTweets = new MySqlCommand("SELECT * FROM tweet LEFT JOIN geocode ON (geocode.tweetIDstr = tweet.tweetIDstr) LEFT JOIN entities ON (entities.tweetIDstr = tweet.tweetIDstr)",connectToDB());
            displayTweets.ExecuteNonQuery();
        }



        public String TweetID()
        {
            return this.tweetID;
        }

        public void setTweetID(String tweet)
        {
            this.tweetID = tweet;
        }

        public String Coords()
        {
            return this.coords;
        }

        public void setCoords(String coord)
        {
            this.coords = coord;
        }

        public String Hashtag()
        {
            return this.hashtag;
        }

        public void setHashtag(String hash)
        {
            this.hashtag = hash;
        }

        public double AnalysisValue()
        {
            return this.analysisValue;
        }

        public void setAnalysisValue(double value)
        {
            this.analysisValue = value;
        }
    }
}