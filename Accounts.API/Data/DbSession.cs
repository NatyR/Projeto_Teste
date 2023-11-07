using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Accounts.API.Data
{
    public class DbSession : IDisposable
    {
        public IDbConnection Connection { get; }
        public DbSession(string connectionString)
        {
            Connection = new OracleConnection(connectionString);
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        public void Dispose()
        {
            Connection?.Close();
        }
    }
}
