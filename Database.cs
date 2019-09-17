using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace CoinAnalysisWebApi
{
    class Database
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        public Database()
        {
            InitializeServerDb();
        }
        public MySqlConnection getConnection()
        {
            return connection;
        }
        private void InitializeServerDb()
        {
            server = "Your DB Server Address";
            database = "coinanalysis";
            uid = "xxxxxxxx";
            password = "xxxxxxxxxx";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";" +
            "persistsecurityinfo =True;SslMode=none;";
            connection = new MySqlConnection(connectionString);
        }        

        public ConnectionState ConnectionState()
        {            
           return connection.State; 
        }
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException)
            {                
                return false;
            }
        }
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException)
            {                
                return false;
            }
        }
    }
}