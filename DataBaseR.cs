using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Data.SqlClient;

namespace TelegramBotTest1
{
    public class DataBaseR
    {
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=DESKTOP-Q85L5BK\IKOSQL;Initial Catalog=schediule;Integrated Security=True;TrustServerCertificate=True");

        public void openConnection()
        {
            if(sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }

        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }

        public SqlConnection getSqlConnection()
        {
            return sqlConnection;
        }
    }
}
