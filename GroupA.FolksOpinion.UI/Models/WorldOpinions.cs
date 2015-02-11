/* File:        WorldOpinions.cs
 * Purpose:     Represents the opinions of any countries with a reaction towards a subject.
 * Version:     1.0
 * Created:     10th February 2015
 * Author:      Gary Fernie
 * Exposes:     WorldOpinions, CountryOpinion, Opinion
 * 
 * Description: - Gets bias and reaction metrics for the countries of the world.
 *              - Any countries which do not have any reaction towards the subject
 *                will not be returned in the results.
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
    public class WorldOpinions
    {
        // Subject of opinions.
        private string subject;
        public string Subject 
        {
            get { return subject;  }
            private set { subject = value; }
        }

        // Opinion data - hashtable of country codes with bias value.
        private List<CountryOpinion> opinions;
        public List<CountryOpinion> Opinions
        {
            get { return opinions; }
            private set { opinions = value; }
        }
        
        public WorldOpinions(string subject)
        {
            if (subject != null && !subject.Equals(""))
            {
                Subject = subject;
                GetOpinions();
            }
        }

        /* Finds the opinions of any countries with subject bias.
         * Countries with no reaction will not be retured.
         */
        public static List<CountryOpinion> GetOpinions(string subject)
        {
            List<CountryOpinion> opinions = new List<CountryOpinion>();

            // TODO: Implement logic to retrieve opinions.

            return opinions;
        }

        /* Uses this object's Subject. */
        private List<CountryOpinion> GetOpinions()
        {
            return GetOpinions(Subject);
        }

        public class CountryOpinion
        {
            // Country of opinion. e.g. 'GB'.
            public string Country { get; set; }

            public Opinion Opinion { get; set; }
        }

        public class Opinion
        {
            // Bias towards subject. -1 to 1.
            public float Bias { get; set; }

            // Reaction to subject. 0 to 1.
            public float Yield { get; set; }
        }
    }
}