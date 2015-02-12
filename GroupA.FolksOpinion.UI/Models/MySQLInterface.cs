/* File:        MySQLInterface.cs
 * Purpose:     Generic abstract interface to connect to a MySQL backend.
 * Version:     0.1
 * Created:     12th February 2015
 * Author:      Michael Rodenhurst
 * Exposes:     Public: Nothing, Protected: SendQuery
 * 
 * Description: - Allows a child class to communicate with a
 *              - MySQL backend without any MySQL specific code.
 *              
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace GroupA.FolksOpinion.UI.Models
{
    public abstract class MySQLInterface
    {
        /* Enum for representing the current state of the SQL connection */
        private enum State
        {
            Disconnected,
            Connected,
            Error
        }

        /* Session variables */
        private String host;
        private String database;
        private String user;
        private String password;

        /* Connection and associated state */
        private MySqlConnection connection;
        private State state;

        public MySQLInterface(String host, String database, String user, String password)
        {
            state = State.Disconnected;

            this.host = host;
            this.database = database;
            this.user = user;
            this.password = password;
        }

        /* Sends a query to the SQL backend. Returns the MySqlDataReader result for output parsing */
        protected MySqlDataReader SendQuery(String query)
        {
            if (state == State.Error) // Don't continue if we are in an error state
                return null;

            if (!Connected()) // If not connected, try to connect
                Connect();
            if (!Connected()) // If we still aren't connected (error), don't continue
                return null;

            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;

            MySqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        private void Connect()
        {
            String connection_string = "server="   + host + ";"
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
            }
        }

        private void Disconnect()
        {
            if (Connected())
            {
                connection.Close();
                state = State.Disconnected;
            }
        }

        private Boolean Connected()
        {
            return state == State.Connected && connection.State == System.Data.ConnectionState.Open;
        }
    }
}