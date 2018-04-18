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
            DBUrl = String.Format("Server={0}; Database={1}; Uid={2}; Pwd={3}; CharSet=utf8;",
                DBHost, DBSchema, DBUsername, DBPassword);
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
                if (connection == null)
                {
                    connection = new MySqlConnection(DBUrl);
                    connection.Open();
                }
                return connection;
            }
        }
    }
}