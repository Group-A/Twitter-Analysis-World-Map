using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class MySQLTwitterCacheEngine : TwitterCacheEngine
    {
        MySQLInterface sql;

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

        public void CacheTweet(Tweet tweet)
        {

        }

        public void UncacheTweet(String id)
        {

        }

        public TweetCacheObject GetTweet(String id)
        {

        }

        public CacheResult GetTweetsFromCache(String subject) // Empty subject gets all tweets
        {

        }

        public void FlushCache() // Clears the cache
        {

        }

        private bool CreateTableStructure()
        {
            return false;
        }

        private bool ValidateTableStructure()
        {
            return false;
        }

    }
}