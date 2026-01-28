using System.Data;

namespace DataRepository
{
    public interface IRepository
    {
        Task<T> GetAsync<T>(IDbConnection dbConnection, object id) where T : class;
        Task<T> GetAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class;
        Task<T> GetAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction transaction) where T : class;
        Task<T?> GetSumOrCountAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType);
        Task<T?> GetSumOrCountAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction transaction);
        Task<bool> GetAnyAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class;
        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection) where T : class;
        Task<long> InsertAsync<T>(IDbConnection dbConnection, T t) where T : class;
        Task<long> InsertAsync<T>(IDbConnection dbConnection, T t, IDbTransaction transaction) where T : class;
        Task<bool> UpdateAsync<T>(IDbConnection dbConnection, T t) where T : class;
        Task<bool> UpdateAsync<T>(IDbConnection dbConnection, T t, IDbTransaction transaction) where T : class;
        Task<int> ExecuteAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType) where T : class;
        Task<IEnumerable<T>> QueryListAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType) where T : class;
        Task<IEnumerable<T>> QueryListAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class;
        Task<int> ExecuteAsync(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType);
        Task<int> ExecuteAsync(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction dbTransaction);
        Task<int> ExecuteAsync<T>(IDbConnection dbConnection, IDbTransaction dbTransaction, string commandText, object parameters, CommandType commandType);
        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType) where T : class;
        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType, IDbTransaction dbTransaction) where T : class;
        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, object parameters, CommandType commandType, IDbTransaction dbTransaction) where T : class;
        Task<IEnumerable<T>> GetListAsync<T>(IDbConnection dbConnection, string commandText, CommandType commandType) where T : class;
        Task<Dictionary<string, string>> ExecuteAsync<T>(IDbConnection dbConnection, IDbTransaction dbTransaction, string commandText, Dictionary<string, object> inputParameters, Dictionary<string, object> outputParameters, CommandType commandType) where T : class;
    }
}
