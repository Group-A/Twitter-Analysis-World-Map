/* File:        Config.cs
 * Purpose:     Loads config from filesystem.
 * Version:     0.1
 * Created:     13th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     GetVariable
 * 
 * Description: - Reads and stores preferences from a config file
 *              - so that various settings are not hard-coded
 *              - eg sql database settings. Format consists
 *              - of "key = value" pairs separated by new lines.
 *              
 * Issues:      - Should likely store config in MemoryCache for
 *              - server persistence.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class Config
    {
        private static Dictionary<String, String> variables; // Contains the Key / Value pairs

        private static String config_filepath = "Content/config.ini";

        /* Loads config from filesystem. This is automatically called when required. */
        private static void LoadConfig()
        {
            variables = new Dictionary<String, String>();

            /* Create StreamReader object to read config file line by line */
            StreamReader reader = new StreamReader(config_filepath);
            String line;
            while((line = reader.ReadLine()) != null)
            {
                line = line.Trim(); // Trim whitespace from each end

                /* Split line into Key / Value pair and do a basic sanity check */
                String[] pair = line.Split(' ');
                if (pair.Length != 2)
                    continue;

                /* Fetch Key / Value and trim whitespace */
                String key = pair[0].Trim();
                String val = pair[1].Trim();

                variables.Add(key, val); // Store variable pair
            }
        }

        /* Returns the requested variable from the config file */
        public static String GetVariable(String key)
        {
            /* Load the config file if necessary */
            if (variables == null)
                LoadConfig();
            
            /* Return variable if it exists in config, otherwise return null */
            String value;
            if (variables.TryGetValue(key, out value))
                return value;

            return null;
        }
    }
}