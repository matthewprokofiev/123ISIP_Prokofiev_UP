using System.Configuration;
using System.Data.SqlClient;

namespace _123ISIP_Prokofiev_UP.Data
{
    /// <summary>Доступ к строке подключения и создание соединений с БД ReadWriteDB.</summary>
    public static class Db
    {
        public static string ConnectionString =>
            ConfigurationManager.ConnectionStrings["ReadWriteDB"].ConnectionString;

        /// <summary>Открывает новое соединение с базой данных.</summary>
        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
