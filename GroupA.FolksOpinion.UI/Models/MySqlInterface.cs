/* File:        MySqlInterface.cs
 * Purpose:     Generic interface to connect to a MySQL backend.
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     Exposition
 * 
 * Description: - Wraps any communication made to a MySql
 *              - backend.
 * 
 * Issues:      - Not threaded
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace GroupA.FolksOpinion.UI.Models
{
    public class MySqlInterface
    {
        /* Enum for representing the current state of the SQL connection */
        private enum State
        {
            Disconnected,
            Connected,
            Error
        }

        /* Session variables */
        private string host;
        private string database;
        private string user;
        private string password;

        /* Connection and associated state */
        private MySqlConnection connection;
        private State state;

        public MySqlInterface(string host, string database, string user, string password)
        {
            state = State.Disconnected;

            this.host = host;
            this.database = database;
            this.user = user;
            this.password = password;
        }

        /* Creates a database with the specified database_name
         * This will likely fail unless the sql user has elevated privileges
         * Returns true on success */
        public bool CreateDatabase(string database_name)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(database_name))
                return false;

            string query = "CREATE DATABASE " + database_name + ";";
            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Drops the database with the specified database_name
         * This will likely fail unless the sql user has elevated privileges
         * Returns true on success */
        public bool DropDatabase(string database_name)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(database_name))
                return false;

            string query = "DROP DATABASE " + database_name + ";";
            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Creates a table with the specified table name. Columns are passed as (Name, Datatype, Parameter) sets
         * Datatype must be a valid SQL type.
         * Parameter is for specifying unique/primary key specifiers. Pass null if not required
         * Returns true on success */
        public bool CreateTable(string table_name, IEnumerable<Tuple<string, string, string>> columns)
        {
            if (!ValidateConnection())
                return false;

            string query_columns = "";
   
            /* Iterate over each column, building the sql query */
            foreach (Tuple<string, string, string> column in columns)
            {
                if (string.IsNullOrEmpty(column.Item1) || string.IsNullOrEmpty(column.Item2)) // Abort query if invalid data passed
                    return false;

                query_columns += column.Item1; // Column Name
                query_columns += " " + column.Item2; // SQL Datatype
                if(!string.IsNullOrEmpty(column.Item3))  // Add SQL properties if required
                    query_columns += " " + column.Item3;

                /* If this isn't last column, add comma for next column */
                if (column != columns.Last())
                    query_columns += ", ";
            }

            // Compile and execute final query
            string query = "CREATE TABLE " + table_name + " (" + query_columns + ");";
            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Drops a table with the specified table_name
         * Returns true on success */
        public bool DropTable(string table_name)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_name))
                return false;

            string query = "DROP TABLE " + table_name + ";";
            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Marks the specified row in the specified table as a foreign key, pointing to the target row */
        public bool SetForeignKey(string table_src, string row_src, string table_target, string row_target)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_src) || string.IsNullOrEmpty(row_src) ||
                   string.IsNullOrEmpty(table_target) || string.IsNullOrEmpty(row_target))
                return false;

            string query = "ALTER TABLE " + table_src + " ADD FOREIGN KEY (" + row_src + ") REFERENCES " +
                    table_target + "(" + row_target + ")";

            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Renames a table from the specified table_old_name to table_new_name 
         * Returns true on success */
        public bool RenameTable(string table_old_name, string table_new_name)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_old_name) || string.IsNullOrEmpty(table_new_name))
                return false;

            string query = "RENAME TABLE " + table_old_name + " TO " + table_new_name + ";";
            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Inserts a row into the specified table_name, columns are passed as an enumerable of
         * ColumnName, Value tuple pairs.
         * Returns true on success */
        public bool InsertRow(string table_name, IEnumerable<Tuple<string, string>> columns)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_name))
                return false;

            string query_columns = "";
            string query_parameters = "";

            /* Iterate over each column, building the sql query */
            foreach (Tuple<string, string> column in columns)
            {
                if (string.IsNullOrEmpty(column.Item1) || string.IsNullOrEmpty(column.Item2)) // Abort query if invalid data passed
                    return false;

                query_columns += column.Item1; // Append column name to query columns
                query_parameters += '\'' + column.Item2 + '\''; // Append column value to query parameters

                /* If this isn't last column, add comma for next column */
                if (column != columns.Last())
                {
                    query_columns += ", ";
                    query_parameters += ", ";
                }
            }

            /* Build and execute final query */
            string query = "INSERT INTO " + table_name +
                        " (" + query_columns + ") " +
                        "VALUES (" + query_parameters + ");";

            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Updates a row in the specified table_name found with the specified identifer (where clause)
         * Columns are passed via ColumnName, Value tuple pairs.
         * Returns true on success */
        public bool UpdateRow(string table_name, string identifier, IEnumerable<Tuple<string, string>> columns)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_name) || string.IsNullOrEmpty(identifier))
                return false;

            string query_columns = "";

            /* Iterate over each column, building the sql query */
            foreach (Tuple<string, string> column in columns)
            {
                if (string.IsNullOrEmpty(column.Item1) || string.IsNullOrEmpty(column.Item2)) // Abort query if invalid data passed
                    return false;

                query_columns += column.Item1 + "='" + column.Item2 + "'"; // Append name/value pair to query columns

                /* If this isn't last column, add comma for next column */
                if (column != columns.Last())
                    query_columns += ", ";
            }

            /* Build and execute final query */
            string query = "UPDATE " + table_name + " SET " + query_columns +
                        " WHERE " + identifier + ";";

            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Deletes a row from the specified table_name using the
         * provided identifier as the where clause.
         * Returns true on success */
        public bool DeleteRow(string table_name, string identifier)
        {
            if (!ValidateConnection())
                return false;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_name) || string.IsNullOrEmpty(identifier))
                return false;

            /* Build and execute query */
            string query = "DELETE FROM " + table_name + " WHERE " + identifier + ";";
            MySqlException exception = ExecuteNonQuery(query);

            return exception == null ? true : false;
        }

        /* Returns the contents of an SQL select query as a MySqlDataReader object.
         * 'where' clause is specified as the identifier. Pass identifier as null to specify no clause
         * The list of columns to return is specified in the columns enumerable.
         * Selects all columns (*) if column enumerable is null.
         * Returns MySqlDataReader object on success, null on failure. */
        public MySqlDataReader Select(string table_name, string identifier, IEnumerable<string> columns)
        {
            if (!ValidateConnection())
                return null;

            /* Basic input validation */
            if (string.IsNullOrEmpty(table_name))
                return null;

            string query_columns = "";

            /* If columns is null, set column criteria to '*', 
             * else Iterate over column list, adding to columns section of query */
            if (columns == null)
                query_columns = "*";
            else
            {
                foreach (string column in columns)
                {
                    query_columns += column;

                    if (column != columns.Last()) // Append comma for next column if not last in list
                        query_columns += ", ";
                }
            }

            /* Build and execute query */
            string query = "SELECT " + query_columns + " FROM " + table_name;
            if (!string.IsNullOrEmpty(identifier)) // Append where clause if necessary
                query += " WHERE " + identifier;
            query += ";";

            return ExecuteReaderQuery(query);
        }

        /* Executes a non-query and returns any MySqlException */
        private MySqlException ExecuteNonQuery(string query)
        {
            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;

            try {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e) {
                return e;
            }

            return null;
        }

        /* Sends a generic command query to the SQL backend which expects a parsable result
         * Returns the MySqlDataReader result for output parsing */
        private MySqlDataReader ExecuteReaderQuery(string query)
        {
            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;

            MySqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        /* Attempts to reconnect if the connection was lost
         * Returns true if current MySql connection is valid */
        private bool ValidateConnection()
        {
            if (state == State.Error) // Return if we are already in error state
                return false;

            if (!Connected()) // If not connected, try to connect
                Connect();
            if (!Connected()) // If we still aren't connected, return false
                return false;

            return true; // Connection is valid
        }

        /* Attempts to establish connection with the MySql server */
        private void Connect()
        {
            string connection_string = "server="   + host + ";"
                                     + "uid="      + user + ";"
                                     + "pwd="      + password + ";"
                                     + "database=" + database + ";";

            try
            {
                connection = new MySqlConnection(connection_string);
                connection.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                state = State.Error;
                return;
            }

            state = State.Connected;
        }

        /* Closes connection to MySql server */
        private void Disconnect()
        {
            if (Connected())
            {
                connection.Close();
                state = State.Disconnected;
            }
        }

        /* Returns boolean validity of the current MySql connection */
        private bool Connected()
        {
            return state == State.Connected && connection.State == System.Data.ConnectionState.Open;
        }
    }
}