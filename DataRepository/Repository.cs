using Dapper;
using Dapper.Contrib.Extensions;
using System.Data;

namespace DataRepository
{
    public class Repository : IRepository
    {
        public async Task<int> ExecuteAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType) where T : class
        {
            return await dbConnection.ExecuteAsync(commandText, commandType);
        }

        public async Task<T> GetAsync<T>(IDbConnection dbConnection, object id) where T : class
        {
            return await dbConnection.GetAsync<T>(id);
        }

        public async Task<T> GetAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class
        {
            return await dbConnection.QueryFirstOrDefaultAsync<T>(commandText, parameters, commandType: commandType);
        }

        public async Task<T> GetAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction transaction) where T : class
        {
            return await dbConnection.QueryFirstOrDefaultAsync<T>(commandText, parameters, transaction, commandType: commandType);
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection) where T : class
        {
            return await dbConnection.GetAllAsync<T>();
        }

        public async Task<long> InsertAsync<T>(IDbConnection dbConnection, T t) where T : class
        {
            return Convert.ToInt64(await dbConnection.InsertAsync(t));
        }

        public async Task<long> InsertAsync<T>(IDbConnection dbConnection, T t, IDbTransaction transaction) where T : class
        {
            return Convert.ToInt64(await dbConnection.InsertAsync(t, transaction));
        }

        public async Task<IEnumerable<T>> QueryListAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType) where T : class
        {
            return await dbConnection.QueryAsync<T>(commandText, commandType: commandType);
        }

        public async Task<bool> UpdateAsync<T>(IDbConnection dbConnection, T t) where T : class
        {
            return await dbConnection.UpdateAsync(t);
        }

        public async Task<int> ExecuteAsync(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType)
        {
            return await dbConnection.ExecuteAsync(commandText, param: parameters, commandType: commandType);
        }

        public async Task<int> ExecuteAsync(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction dbTransaction)
        {
            return await dbConnection.ExecuteAsync(commandText, param: parameters, dbTransaction, commandType: commandType);
        }

        public async Task<IEnumerable<T>> QueryListAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class
        {
            return await dbConnection.QueryAsync<T>(commandText, param: parameters, commandType: commandType);
        }

        public async Task<bool> GetAnyAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class
        {
            return dbConnection.ExecuteScalar<bool>(commandText, parameters, commandType: commandType);
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class
        {
            return await dbConnection.QueryAsync<T>(commandText, commandType: commandType, param: parameters);
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction dbTransaction) where T : class
        {
            return await dbConnection.QueryAsync<T>(commandText, param: parameters, dbTransaction, commandType: commandType);
        }

        public async Task<T?> GetSumOrCountAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType)
        {
            return await dbConnection.ExecuteScalarAsync<T>(commandText, parameters, commandType: commandType);
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType) where T : class
        {
            return await dbConnection.QueryAsync<T>(commandText, commandType: commandType);
        }

        public async Task<int> ExecuteAsync<T>(IDbConnection dbConnection, IDbTransaction dbTransaction, string commandText, object parameters, CommandType commandType)
        {
            return await dbConnection.ExecuteAsync(commandText, param: parameters, dbTransaction, commandType: commandType);
        }

        public async Task<bool> UpdateAsync<T>(IDbConnection dbConnection, T t, IDbTransaction transaction) where T : class
        {
            return await dbConnection.UpdateAsync(t, transaction);
        }

        public async Task<Dictionary<string, string>> ExecuteAsync<T>(IDbConnection dbConnection, IDbTransaction dbTransaction, string commandText, Dictionary<string, object> inputParameters, Dictionary<string, object> outputParameters, CommandType commandType) where T : class
        {
            var dynamicParameters = new DynamicParameters();

            foreach (var (key, value) in inputParameters)
            {
                dynamicParameters.Add(key, value);
            }

            foreach (var (key, value) in outputParameters)
            {
                dynamicParameters.Add(key, value, direction: ParameterDirection.Output);
            }

            await dbConnection.ExecuteAsync(commandText, dynamicParameters, transaction: dbTransaction, commandType: commandType);

            return outputParameters.ToDictionary(returnOutputParameter => returnOutputParameter.Key, returnOutputParameter => dynamicParameters.Get<string>(returnOutputParameter.Key));
        }
    }
}
