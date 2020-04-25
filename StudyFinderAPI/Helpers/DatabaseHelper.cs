using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace StudyFinderAPI.Helpers
{
    public class DBHelper
    {
        public static readonly HttpClient client = new HttpClient();

        public static IEnumerable<Dictionary<string, object>> queryDatabase(SqlCommand command)
        {

            IEnumerable<Dictionary<string, object>> serialized = null;

            string connectionString = "Server=localhost; database=StudyFinder; UID=root; password=430330; Allow User Variables=True";
            //string connectionString = "server=172.31.23.250;uid=root;pwd=0laf_u4Ehva;database=" + DBName + ";Allow User Variables=True";
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

            // Convert the SqlCommand to a MySqlCommand, and copy any parameters.
            MySqlCommand c = new MySqlCommand(command.CommandText, connection);

            foreach (SqlParameter p in command.Parameters)
            {
                c.Parameters.AddWithValue(p.ParameterName, p.Value);
            }

            MySqlDataReader reader = c.ExecuteReader();
            serialized = SerializeMySQL(reader);
            return serialized;
        }

        public static int editDatabase(string DBName, SqlCommand command)
        {

            string connectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=" + DBName + ";User ID=webaccess;password=baremin";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                command.Connection = connection;
                command.Connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static IEnumerable<Dictionary<string, object>> Serialize(SqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeRow(cols, reader));

            return results;
        }
        private static Dictionary<string, object> SerializeRow(IEnumerable<string> cols, SqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }

        public static IEnumerable<Dictionary<string, object>> SerializeMySQL(MySqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
                cols.Add(reader.GetName(i));

            while (reader.Read())
                results.Add(SerializeMySQLRow(cols, reader));

            return results;
        }
        private static Dictionary<string, object> SerializeMySQLRow(IEnumerable<string> cols, MySqlDataReader reader)
        {
            var result = new Dictionary<string, object>();
            foreach (var col in cols)
                result.Add(col, reader[col]);
            return result;
        }
    }
}