/* File:        TwitterCacheEngine.cs
 * Purpose:     Abstract class to define how Twitter data will be cached
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     TwitterCacheEngine, TwitterCacheEngine.CacheResult
 *
 * Description: - Exposes a set of entry points that must be implemented by
 *                any TwitterCacheEngine subclass so that they may be used elsewhere in
 *                the application.
 * 
 * Changes:     17th February 2015, Gary Fernie
 *              - Changed to use new opinion entities.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;

namespace GroupA.FolksOpinion.UI.Models
{
    public abstract class TwitterCacheEngine
    {
        /* Caches the provided array of TweetOpinions */
        public abstract void CacheTweets(IEnumerable<TweetOpinion> tweets);

        /* Returns all cached tweets associated with the provided subject */
        public abstract IEnumerable<TweetOpinion> GetTweets(string subject);

        /* Checks the cache validity against the provided data structure
         * Returns true on successful validation */
        public abstract bool ValidateCache(Type type);
    }
}