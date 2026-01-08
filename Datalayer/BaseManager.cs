using DataRepository;
using Microsoft.Data.SqlClient;
using Shared.Enumerations;
using System.Data;

namespace Datalayer
{
    public class BaseManager
    {
        protected IRepository _repository;

        protected static IDbConnection CreateConnection(DatabaseConnectionType databaseConnectionType, string connectionString)
        {
            return databaseConnectionType switch
            {
                DatabaseConnectionType.MicrosoftSQLServer => new SqlConnection(connectionString)
            };
        }
    }
}
