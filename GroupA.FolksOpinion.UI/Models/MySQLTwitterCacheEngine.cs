/* File:        MySQLTwitterCacheEngine.cs
 * Purpose:     
 * Version:     1.2
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     MySQLTwitterCacheEngine
 *
 * Description: 
 * 
 * Changes:     17th February 2015, ver 1.1, Gary Fernie
 *              - Changed to use new opinion entities.
 *              - Stubbed to allow build.
 *              
 *              25th February 2015, ver 1.2, Jamie Aitken
 *              - Fleshed out stubs
 *              
 * Still to Do: Make Insert Dynamic
 *              Remove the prefix from types i.e System.String, FolksOpinion.Place
 *              Decide on design of initial database, I'm thinking Place table and Opinion table should have an id_str
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class MySQLTwitterCacheEngine : TwitterCacheEngine
    {
        MySQLInterface sql;
        PropertyInfo[] tweetProp = typeof(Tweet).GetProperties();
        PropertyInfo[] placeProp = typeof(Place).GetProperties();
        PropertyInfo[] TWProp = typeof(TweetOpinion).GetProperties();
        PropertyInfo[] opinionProp = typeof(Opinion).GetProperties();

        public MySQLTwitterCacheEngine()
        {
            /* Get SQL config variables */
            String host = Config.GetVariable("mysql_cache_host");
            String database = Config.GetVariable("mysql_cache_database");
            String user = Config.GetVariable("mysql_cache_user");
            String password = Config.GetVariable("mysql_cache_password");

            /* Initialise SQL interface */
            sql = new MySQLInterface(host, database, user, password);
        }

        public override void CacheTweet(TweetOpinion tweet)
        {
            string place = "INSERT INTO " + typeof(Place) + " VALUES (";
            string tweetO = "INSERT INTO " + typeof(Tweet) + " VALUES (";
            string opinion = "INSERT INTO " + typeof(Opinion) + " VALUES (";
            place += "" + tweet.Tweet.place.country + ", "+tweet.Tweet.place.country_code;
            tweetO += "" + tweet.Tweet.id_str + ", " + tweet.Tweet.lang + ", " + tweet.Tweet.place + ", " + tweet.Tweet.text;
            opinion += "" + tweet.Opinion.PositiveBias + ", " + tweet.Opinion.NegativeBias;
          

            place += ");";
            tweetO += ");";
            opinion += ");";
            sql.SendQuery(tweetO);
            sql.SendQuery(place);
            sql.SendQuery(opinion);
        }

        public override void UncacheTweet(String id)
        {
            string deleteTweet = "DELETE FROM " + typeof(Tweet).Name + " WHERE ";
            string deletePlace = "DELETE FROM " + typeof(Place).Name + " WHERE ";
            string deleteOpinion = "DELETE FROM " + typeof(Opinion).Name + "WHERE ";
            foreach(var property in tweetProp)
            {
                if(property.Name.Contains("id_str"))
                {
                    deleteTweet += "" + property.Name + " = " + id + ";";
                }
            }
            foreach(var property in placeProp)
            {
                if(property.Name.Contains("id_str"))
                {
                    deletePlace += "" + property.Name + " = " + id + ";";
                }
            }
            foreach(var property in opinionProp)
            {
                if(property.Name.Contains("id_str"))
                {
                    deleteOpinion += "" + property.Name + " = " + id + ";";
                }
            }

            sql.SendQuery(deleteTweet);
            sql.SendQuery(deletePlace);
            sql.SendQuery(deleteOpinion);
        }

        public override TweetOpinion GetTweet(String id)
        {
            return new TweetOpinion();
        }

        public override CacheResult GetTweetsFromCache(String subject) // Empty subject gets all tweets
        {
            if(subject!="" && subject!=null)
            {
                foreach(var prop in tweetProp)
                {
                    if(prop.Name.Contains("subject"))
                    {
                        sql.SendQuery("SELECT * FROM " + typeof(Tweet).Name + " WHERE " + prop.Name + " = " + subject + ";");
                    }
                }
            }
            else
            {
                sql.SendQuery("SELECT * FROM " + typeof(Tweet).Name + ";");
                sql.SendQuery("SELECT * FROM " + typeof(Place).Name + ";");
                sql.SendQuery("SELECT * FROM " + typeof(Opinion).Name + ";");
            }
            return new CacheResult();
        }

        public override void FlushCache() // Clears the cache
        {
            sql.SendQuery("TRUNCATE TABLE " + typeof(Tweet).Name + "");
            sql.SendQuery("TRUNCATE TABLE " + typeof(Place).Name + "");
            sql.SendQuery("TRUNCATE TABLE " + typeof(Opinion).Name + "");
        }

        private bool CreateTableStructure()
        {
            string createTweetTable = "CREATE TABLE " + typeof(Tweet).Name+" (";
            string createPlaceTable = "CREATE TABLE " + typeof(Place).Name +" (";
            string createOpinionTable = "CREATE TABLE " + typeof(Opinion).Name+" (";
            foreach(var rootProperty in TWProp)
            {
                foreach(var tweetPro in tweetProp)
                {
                    if(rootProperty.PropertyType.Equals(tweetPro.ReflectedType))
                    {
                        foreach (var placePro in placeProp)
                        {
                            if(tweetPro.PropertyType.Equals(placePro.ReflectedType))
                            {
                                if(placeProp.Length>1)
                                {
                                    createPlaceTable += "" + placePro.Name + " " + placePro.PropertyType + ",";
                                }
                                else
                                {
                                    createPlaceTable += "" + placePro.Name + " " + placePro.PropertyType + "";
                                }
                            }
                        }
                    }
                    if (tweetProp.Length > 1)
                    {
                        createTweetTable += "" + tweetPro.Name + " " + tweetPro.PropertyType + ",";
                    }
                    else
                    {
                        createTweetTable += "" + tweetPro.Name + " " + tweetPro.PropertyType + "";
                    }
                }
                foreach(var opinProp in opinionProp)
                {
                    if(rootProperty.PropertyType.Equals(opinProp))
                    {
                        if(opinionProp.Length>1)
                        {
                            createOpinionTable += "" + opinProp.Name + " " + opinProp.PropertyType + ",";
                        }
                        else
                        {
                            createOpinionTable += "" + opinProp.Name + " " + opinProp.PropertyType + "";
                        }
                        
                    }
                }
            }
            createTweetTable += ");";
            createPlaceTable += ");";
            createOpinionTable += ");";
            if (createTweetTable != null && createPlaceTable != null && createOpinionTable != null)
            {
                sql.SendQuery(createTweetTable + createPlaceTable + createOpinionTable);
                return true;
            }
            return false;
        }

        private bool ValidateTableStructure()
        {
            return false;
        }

    }
}