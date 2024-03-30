using System.Data;
using System.Data.SqlClient;

namespace MS.DataManagementLibrary
{
    public class DataManager
    {
        public SqlDataReader SQLGetData(string sqlString, string connString)
        {
            SqlConnection conn = new SqlConnection(connString);
            string CommandText = sqlString;
            SqlCommand cmd = new SqlCommand(CommandText, conn);
            cmd.CommandType = CommandType.Text; /* or StoredPro. */
            conn.Open();
            SqlDataReader result = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return result;
        }

        public int SQLNonQuery(string sqlString, string connString)
        {
            SqlConnection conn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand(sqlString, conn);
            cmd.Connection.Open();
            return cmd.ExecuteNonQuery();
        }  
    }
}
