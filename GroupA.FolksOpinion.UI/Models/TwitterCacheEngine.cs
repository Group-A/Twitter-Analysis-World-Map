/* Purpose:     Abstract class to define how Twitter data will be cached
 * Created:     12th February 2015
 *
 * Description: - Exposes a set of entry points that must be implemented by
 *                any TwitterCacheEngine subclass so that they may be used elsewhere in
 *                the application.
 *
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
        public struct TweetCacheObject // TODO probably shouldn't exist, as this 'object' should be defined elsewhere?
        {
            Tweet tweet;
            double analysis_value; // FIXME bias/popularity etc
        }

        public struct CacheResult
        {
            TweetCacheObject[] tweets;
        }

        protected ObjectCache cache = MemoryCache.Default;

        public abstract void CacheTweet(Tweet tweet);

        public abstract void UncacheTweet(String id);

        public abstract TweetCacheObject GetTweet(String id);

        public abstract CacheResult GetTweetsFromCache(String subject); // Empty subject gets all tweets

        public abstract void FlushCache(); // Clears the cache
    }
}