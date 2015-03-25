/* File:        MySqlStorage.cs
 * Purpose:     
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     MySqlStorage
 *
 * Description: 
 * 
 * Changes:     17th February 2015, Gary Fernie
 *              - Changed to use new opinion entities.
 *              - Stubbed to allow build.
 *              25th February 2015, Jamie Aitken
 *              - Fleshed out stubs
 *              5th March 2015, Michael Rodenhurst
 *              - Changed everything
 *              
 * Issues:      - 
 */

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web;

namespace GroupA.FolksOpinion.UI.Models
{
    public class MySqlStorage : TwitterCacheEngine
    {
        MySqlInterface sql;

        public MySqlStorage()
        {
            /* Get SQL config variables */
            String host = ConfigurationManager.AppSettings["MySQL_CacheHost"];
            String database = ConfigurationManager.AppSettings["MySQL_CacheDatabase"];
            String user = ConfigurationManager.AppSettings["MySQL_CacheUser"];
            String password = ConfigurationManager.AppSettings["MySQL_CachePassword"];

            /* Initialise SQL interface */
            sql = new MySqlInterface(host, database, user, password);
        }

        public override void CacheTweets(IEnumerable<TweetOpinion> tweets)
        {
            if (tweets == null) // Don't handle invalid dataset.
                return;

            foreach (TweetOpinion tweet_opinion in tweets)
                InsertObject(tweet_opinion);
        }

        public override IEnumerable<TweetOpinion> GetTweets(string subject)
        {
            if (string.IsNullOrEmpty(subject)) // Ignore invalid subject
                return null;

            /* FIXME: This is a manual select query - not dynamic. Must find a way to
             * dynamically specify the target field (TweetOpinion.Tweet.Text atm) */
            MySqlDataReader reader = sql.Select(typeof(Tweet).Name, "text LIKE " + subject, null);

            // For each record in reader result
            //   Build Tweet object from Tweet table record
            //   Iterate over TweetOpinion structure
            //      For each (sub)property, select table from database WHERE __id == tweet record id
            //   Append assembled TweetOpinion to TweetOpinion list
            // Return list

            /* Iterate over all fields of TweetOpinion */

            throw new NotImplementedException();

            return null;
        }

        public override bool ValidateCache(Type type)
        {
            throw new NotImplementedException();
        }

        /* Private functions used internally */

        /* Creates the required table structure by converting the passed Type to sql tables.
         * Object references become foreign keys */
        private void CreateTableStructure(Type type)
        {
            CreateTable(type);
        }

        /* Helper functions */

        /* Inserts the provided object into SQL table. Also inserts any sub-objects
         * required during iteration */
        /* Returns the unique ID of the created SQL record for FK association*/
        private int InsertObject(Object item)
        {
            List<Tuple<string, string>> columns = new List<Tuple<string, string>>();

            /* Iterate over all fields of the provided item */
            foreach (PropertyInfo prop in item.GetType().GetProperties())
            {
                string field_name = prop.Name;
                Object field_value = prop.GetValue(item);

                if (UserDefinedStruct(field_value.GetType()))
                    field_value = InsertObject(field_value);

                columns.Add(new Tuple<string, string>(field_name, "" + field_value));
            }

            sql.InsertRow(item.GetType().Name, columns); // Insert row into table

            /* Get ID of row we just inserted. Note: Probably not thread safe */
            List<string> column = new List<string>();
            column.Add("MAX(__UID)");
            MySqlDataReader reader = sql.Select(item.GetType().Name, null, column);

            return reader.GetInt32(0); // TODO: null handling
        }
        
        /* Creates the specified table, also creates any necessary sub-tables */
        private void CreateTable(Type type)
        {
            List<Tuple<string, string, string>> rows = new List<Tuple<string, string, string>>();
            rows.Add(new Tuple<string, string, string>("__UID", "int", "AUTO_INCREMENT")); // DB UID - Not to be confused with data structure ids


            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (UserDefinedStruct(prop.PropertyType))
                    CreateTable(prop.PropertyType);

                rows.Add(new Tuple<string, string, string>(prop.Name, GetSQLDataType(prop.PropertyType), null));
            }

            // Create table
            sql.CreateTable(type.Name, rows);

            /* Mark foreign keys */
            foreach (PropertyInfo prop in type.GetProperties())
                if (UserDefinedStruct(prop.PropertyType))
                    sql.SetForeignKey(type.Name, prop.Name, prop.PropertyType.Name, "__UID");
        }

        /* Returns true if the passed type is a user-defined struct,
         * rather than a "standard" built-in/library type. */
        private bool UserDefinedStruct(Type type)
        {
           return type.IsValueType && !type.IsPrimitive;
        }

        /* Horrible function that converts a C# basic datatype (including box wrappers)
         * to a MySQL string-type (ie. 'int') */
        private string GetSQLDataType(Type type)
        {
            if(UserDefinedStruct(type))
                return "int";   // Foreign keys are ints

            return null;
        }

        /* Recursively iterates over all sub-types in the passed Type (if it's a structure) 
         * and returns a list */
        private List<Tuple<Type, string>> GetObjectTypes(Type type)
        {
            List<Tuple<Type, string>> types = new List<Tuple<Type, string>>();

            foreach(PropertyInfo prop in type.GetProperties())
            {
                if (UserDefinedStruct(prop.PropertyType))
                    types.AddRange(GetObjectTypes(type));
                else
                    types.Add(new Tuple<Type, string>(prop.PropertyType, prop.Name));
            }

            return types;
        }
    }
}