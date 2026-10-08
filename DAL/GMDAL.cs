using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ERP.DAL
{
	public class GMDAL
	{
        public static string Hash(string input)
        {
            input = "Sadique" + input + "Sarwar";
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        public static DataTable GetDataTable(string sSql, List<SqlParameter> sqlParameters)
		{
			DataTable dt = new DataTable();

			using (var repo = new GMContext())
			{
				SqlCommand sqlCommand = new SqlCommand(sSql, (SqlConnection)repo.Database.Connection);
				sqlCommand.CommandType = CommandType.Text;
				sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
				SqlDataAdapter da = new SqlDataAdapter(sqlCommand);
				da.Fill(dt);
				da.Dispose();
			}
			return dt;
		}

	}
}