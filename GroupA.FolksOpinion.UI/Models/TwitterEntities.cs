/* File:        TwitterEntities.cs
 * Purpose:     Represents the various entities in Twitter.
 * Version:     2.0
 * Created:     10th February 2015
 * Author:      Gary Fernie
 * Exposes:     Tweet, Place, GetSearchTweetsResponse
 * 
 * Description: - Various classes to represent entities and responses.
 *              - Not a complete list of Twitter entities.
 *              - Entities represented here may be have an imcomplete
 *                list of fields.
 *                
 * Changes:     17th February 2015, 2.0
 *              - File now also models API responses.
 */

using System.Collections.Generic;

namespace GroupA.FolksOpinion.UI.Models
{
    /* Twitter entity: Tweet
     * https://dev.twitter.com/overview/api/tweets
     */
    public class Tweet
    {
        public string id_str { get; set; }
        public string lang { get; set; }
        public Place place { get; set; }
        public string text { get; set; }
    }

    /* Twitter entity: Place
     * https://dev.twitter.com/overview/api/places
     */
    public class Place
    {
        public string country { get; set; }
        public string country_code { get; set; }
    }

    /* Twitter response: GET search/tweets
     * https://dev.twitter.com/rest/reference/get/search/tweets
     */
    public class GetSearchTweetsResponse
    {
        public IEnumerable<Tweet> statuses { get; set; }
        public SearchMetadata search_metadata { get; set; }

        public class SearchMetadata
        {
            public string refresh_url { get; set; }
            public string next_results { get; set; }
            public string since_id_str { get; set; }
            public string query { get; set; }
            public string max_id_str { get; set; }
        }
    }
}