using System.Configuration;
using System.Data.SqlClient;

namespace _123ISIP_Prokofiev_UP.Data
{

    public static class Db
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["ReadWriteDB"].ConnectionString;

        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
