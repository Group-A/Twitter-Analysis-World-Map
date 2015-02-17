/* File:        MySQLTwitterCacheEngine.cs
 * Purpose:     
 * Version:     1.1
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     MySQLTwitterCacheEngine
 *
 * Description: 
 * 
 * Changes:     17th February 2015, ver 1.1, Gary Fernie
 *              - Changed to use new opinion entities.
 *              - Stubbed to allow build.
 */

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

        public override void CacheTweet(Tweet tweet)
        {

        }

        public override void UncacheTweet(String id)
        {

        }

        public override TweetOpinion GetTweet(String id)
        {
            return new TweetOpinion();
        }

        public override CacheResult GetTweetsFromCache(String subject) // Empty subject gets all tweets
        {
            return new CacheResult();
        }

        public override void FlushCache() // Clears the cache
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