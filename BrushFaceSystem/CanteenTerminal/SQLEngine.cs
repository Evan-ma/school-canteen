using MySql.Data.MySqlClient;
using System;

namespace DBLayer
{
    public class SQLEngine
    {
        private String DBHost = "localhost";
        private String DBSchema = "test";
        private String DBUsername = "root";
        private String DBPassword = "123456";
        private String DBUrl;
        private MySqlConnection connection;

        private static SQLEngine instance;

        private SQLEngine()
        {
            DBUrl = String.Format("Server={0}; Uid={1}; Pwd={2}; CharSet=utf8;",
                DBHost, DBUsername, DBPassword);
            Console.WriteLine("ConnectionString： " + DBUrl);
        }

        public static SQLEngine Instance
        {
            get { return instance ?? (instance = new SQLEngine()); }
        }

        public MySqlConnection Connection
        {
            get
            {
                try
                {
                    if (connection == null)
                    {
                        connection = new MySqlConnection(DBUrl);
                        connection.Open();
                    }
                    return connection;
                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }
    }
}