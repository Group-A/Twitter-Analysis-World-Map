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

        public struct CacheResult
        {
            TweetOpinion[] tweets;
        }

        protected ObjectCache cache = MemoryCache.Default;

        public abstract void CacheTweet(TweetOpinion tweet);

        public abstract void UncacheTweet(String id);

        public abstract TweetOpinion GetTweet(String id);

        public abstract CacheResult GetTweetsFromCache(String subject); // Empty subject gets all tweets

        public abstract void FlushCache(); // Clears the cache
    }
}