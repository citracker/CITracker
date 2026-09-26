using DataRepository;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Shared.Enumerations;
using Shared.Interfaces;
using System.Data;

namespace Datalayer
{
    public class BaseManager
    {
        protected IRepository _repository;
        protected IAppSettingsManager _connection;

        private string _cachedConnectionString;
        private readonly SemaphoreSlim _connectionStringLock = new(1, 1);

        public BaseManager(IAppSettingsManager connection)
        {
            _connection = connection;
        }

        protected static IDbConnection CreateConnection(DatabaseConnectionType databaseConnectionType, string connectionString)
        {
            return databaseConnectionType switch
            {
                DatabaseConnectionType.MicrosoftSQLServer => new SqlConnection(connectionString)
            };
        }

        private async Task<string> GetConnectionStringAsync()
        {
            if (!string.IsNullOrEmpty(_cachedConnectionString))
                return _cachedConnectionString;

            await _connectionStringLock.WaitAsync();
            try
            {
                if (string.IsNullOrEmpty(_cachedConnectionString))
                    _cachedConnectionString = await _connection.SQLDBConnection();

                return _cachedConnectionString;
            }
            finally
            {
                _connectionStringLock.Release();
            }
        }

        public async Task<IDbConnection> OpenConnectionAsync()
        {
            var connStr = await GetConnectionStringAsync();

            var conn = (SqlConnection)CreateConnection(
                DatabaseConnectionType.MicrosoftSQLServer, connStr);

            try
            {
                await conn.OpenAsync();
                return conn;
            }
            catch (SqlException ex) when (IsTransientOpenFailure(ex))
            {
                try { conn.Dispose(); } catch { /* ignore */ }

                await Task.Delay(TimeSpan.FromSeconds(3));

                var retry = (SqlConnection)CreateConnection(
                    DatabaseConnectionType.MicrosoftSQLServer, connStr);

                await retry.OpenAsync();
                return retry;
            }
        }

        private static bool IsTransientOpenFailure(SqlException ex)
        {
            // -2      = timeout expired
            // 4060    = cannot open database
            // 40197   = Azure service error on login
            // 40501   = Azure service busy
            // 49918   = Azure not enough resources
            // 49919   = Azure operation in progress
            // 49920   = Azure too many operations
            return ex.Number == -2
                || ex.Number == 4060
                || ex.Number == 40197
                || ex.Number == 40501
                || ex.Number == 49918
                || ex.Number == 49919
                || ex.Number == 49920;
        }
    }
}
