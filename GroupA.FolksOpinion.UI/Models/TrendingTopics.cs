/* File:        TrendingTopics.cs
 * Purpose:     Encapsulates trending topics state and functionality.
 * Created:     9th March 2015
 * Author:      Gary Fernie
 * Exposes:     TrendingTopics
 * 
 * Description: - Represents a list of trending topics.
 *              - Provides functionality to maintin list.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class TrendingTopics
    {
        private static FolksOpinionTwitterApi api = FolksOpinionTwitterApi.Instance;
        
        public IEnumerable<Trend> CurrentTrends
        {
            get { return GetTrends(); }
        }

        private IEnumerable<Trend> GetTrends()
        {
            return api.GetTrends();
        }
    }
}