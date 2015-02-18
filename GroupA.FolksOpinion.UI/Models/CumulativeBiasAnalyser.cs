/* File:        CumulativeBiasAnalyser.cs
 *              (previously known as "Analyser.cs")
 * Purpose:     Analyses a string for opinion.
 * Version:     1.3
 * Created:     
 * Author:      Michael Rodenhurst
 * Exposes:     CumulativeBiasAnalyser
 * 
 * Description: - Uses a simple word count to find opinion.
 * 
 * Changes:     17th February 2015, ver1.1, Gary Fernie
 *              - Implemented ILexicalBiasAnalyser interface.
 *              - Removed Tweet-specific references.
 *              - Added explicit empty default contructor.
 *              17th February 2015, ver1.2, Gary Fernie
 *              - Added functionality to calculate opinion, based on 
 *                  new pos/neg metrics.
 *              - Code is 99% copied fom old method (still in file).
 *              - Bit of a hack.
 *              17th February 2015, ver1.3, Gary Fernie
 *              - Fixed dictionary paths.
 *              - Moved dictionaries to more appropriate folder.
 *              - Fixed dictionary cache names.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Caching;

namespace GroupA.FolksOpinion.UI.Models
{
    public class CumulativeBiasAnalyser : ILexicalBiasAnalyser
    {
        private static ObjectCache cache = MemoryCache.Default;

        private static String dictionary_path = 
            System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath 
            + "App_Data\\Dictionary\\";

        private static double scalar = 3d;

        // For bias normalisation.
        private static int arbitraryBiasClamp = 10;

        public CumulativeBiasAnalyser() { }

        public Opinion Analyse(string text)
        {
            return GetTextOpinion(text);
        }

        private static void LoadDictionaries()
        {
            LoadDictionary("dictionary_en_positive", dictionary_path + "en_positive.txt");
            LoadDictionary("dictionary_en_negative", dictionary_path + "en_negative.txt");
        }

        private static void LoadDictionary(String name, String filepath)
        {
            StreamReader reader = new StreamReader(filepath);

            /* Read the dictionary into a string array which is placed in the cache */
            List<String> words = new List<String>();

            String line;
            while((line = reader.ReadLine()) != null)
                words.Add(line.Trim());

            // Place loaded string array into preserved cache
            cache[name] = words.ToArray();
        }

        public static Opinion GetTextOpinion(String text)
        {
            if (!DictionaryLoaded())
                LoadDictionaries();

            String[] dictionary_positive = (String[])cache.Get("dictionary_en_positive"); // Get postive dictionary
            String[] dictionary_negative = (String[])cache.Get("dictionary_en_negative"); // Get negative dictionary
            String[] words = text.Trim().Split(); // Split tweet into words

            /* Iterate over each word, match against the dictionary */
            int positive_words = 0;
            int negative_words = 0;
            for (int i = 0; i < words.Length; i++) // For each word
            {
                /* Iterate over each word in the positive and negative dictionary and match it */
                for (int j = 0; j < dictionary_positive.Length; j++)
                {
                    if(dictionary_positive[j] == words[i])
                    {
                        positive_words++;
                        break;
                    }
                }

                for (int j = 0; j < dictionary_negative.Length; j++)
                {
                    if (dictionary_negative[j] == words[i])
                    {
                        negative_words++;
                        break;
                    }
                }
            }

            double negative_bias = negative_words;
            double positive_bias = positive_words;

            if (negative_bias > arbitraryBiasClamp) negative_bias = arbitraryBiasClamp;
            if (positive_bias > arbitraryBiasClamp) positive_bias = arbitraryBiasClamp;

            return new Opinion
            {
                PositiveBias = positive_bias/10,
                NegativeBias = negative_bias/10
            };
        }

        public static double GetTextBias(String text)
        {
            if (!DictionaryLoaded())
                LoadDictionaries();

            String[] dictionary_positive = (String[])cache.Get("dictionary_en_positive"); // Get postive dictionary
            String[] dictionary_negative = (String[])cache.Get("dictionary_en_negative"); // Get negative dictionary
            String[] words = text.Trim().Split(); // Split tweet into words

            /* Iterate over each word, match against the dictionary */
            int positive_words = 0;
            int negative_words = 0;
            for (int i = 0; i < words.Length; i++) // For each word
            {
                /* Iterate over each word in the positive and negative dictionary and match it */
                for (int j = 0; j < dictionary_positive.Length; j++)
                {
                    if(dictionary_positive[j] == words[i])
                    {
                        positive_words++;
                        break;
                    }
                }

                for (int j = 0; j < dictionary_negative.Length; j++)
                {
                    if (dictionary_negative[j] == words[i])
                    {
                        negative_words++;
                        break;
                    }
                }
            }

            double negative_influence = negative_words / words.Length;
            double positive_influence = positive_words / words.Length;

            double bias = (positive_influence - negative_influence) * scalar;
            if (bias > 1.0) bias = 1.0;
            if (bias < -1.0) bias = -1.0; // Clamp bias

            return bias;
        }

        private static bool DictionaryLoaded()
        {
            if (cache.Get("dictionary_en_positive") != null && cache.Get("dictionary_en_negative") != null)
                return true;

            return false;
        }
    }
}