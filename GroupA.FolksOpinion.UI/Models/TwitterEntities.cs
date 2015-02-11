/* File:        TwitterEntities.cs
 * Purpose:     Represents the various entities in Twitter.
 * Version:     1.0
 * Created:     10th February 2015
 * Author:      Gary Fernie
 * Exposes:     Tweet, Places
 * 
 * Description: - Various structs to represent entities.
 *              - Not a complete list of entities.
 *              - Entities represented here may be have an imcomplete
 *                list of fields.
 *                
 * INCOMPLETE!
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    /* Twitter entity: Tweet
     * https://dev.twitter.com/overview/api/tweets
     */
    public struct Tweet
    {
    }

    /* Twitter entity: Place
     * https://dev.twitter.com/overview/api/places
     */
    public struct Place
    {
        Hashtable attributes;
        string country;
        string country_code;
    }
}