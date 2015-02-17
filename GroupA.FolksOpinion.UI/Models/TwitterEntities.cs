/* File:        TwitterEntities.cs
 * Purpose:     Represents the various entities in Twitter.
 * Version:     1.1
 * Created:     10th February 2015
 * Author:      Gary Fernie
 * Exposes:     Tweet, Place
 * 
 * Description: - Various classes to represent entities.
 *              - Not a complete list of entities.
 *              - Entities represented here may be have an imcomplete
 *                list of fields.
 */

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
}