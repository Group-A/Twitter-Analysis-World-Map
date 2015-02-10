using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Caching;

namespace GroupA.FolksOpinion.UI.Models
{
    public class Analyser
    {
        private static ObjectCache cache = MemoryCache.Default; // FIXME: Probably needs moved elsewhere

        private static String dictionary_path = "Content/Dictionary/";
        private static double scalar = 3d;

        private static void LoadDictionaries()
        {
            LoadDictionary("dictionary_en_pos", dictionary_path + "en_positive.txt");
            LoadDictionary("dictionary_en_neg", dictionary_path + "en_negative.txt");
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

        public static double GetTweetOpinion(String tweet_text)
        {
            if (!DictionaryLoaded())
                LoadDictionaries();

            String[] dictionary_positive = (String[])cache.Get("dictionary_en_positive"); // Get postive dictionary
            String[] dictionary_negative = (String[])cache.Get("dictionary_en_negative"); // Get negative dictionary
            String[] words = tweet_text.Trim().Split(); // Split tweet into words

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