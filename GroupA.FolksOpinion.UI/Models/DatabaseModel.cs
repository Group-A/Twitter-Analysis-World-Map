using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace GroupA.FolksOpinion.UI.Models
{
    public class DatabaseModel
    {
        private String tweetID;
        private double analysisValue;
        private String coords;
        private String hashtag;
        MySql.Data.MySqlClient.MySqlConnection conn;

        public void connectToDB()
        {
            String myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;" + "pwd=12345;database=test;";
            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void cacheTweet(String id, double value ,String Coords, String hash)
        {
            connectToDB();
            
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