using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Utilities;
using System.Data;

namespace Datalayer.Implementations
{
    public class GenericManager : BaseManager, IGenericManager
    {
        private readonly ILogger<GenericManager> _logger;

        public GenericManager(ILogger<GenericManager> logger, IRepository repository, IAppSettingsManager connection) : base(connection)
        {
            _logger = logger;
            _repository = repository;
            _connection = connection;
        }

        public async Task<long> GetNextTableId(IDbConnection dbConnection, IDbTransaction transaction, string tableName)
        {
            var inputParam = new Dictionary<string, object> { { "@TableName", tableName } };
            var outputParam = new Dictionary<string, object> { { "@OutNextId", string.Empty } };

            // Let exceptions propagate — do NOT swallow.
            var resp = await _repository.ExecuteAsync<object>(
                dbConnection, transaction, DatabaseScripts.GetNextRowId,
                inputParam, outputParam, CommandType.StoredProcedure);

            var nextId = Convert.ToInt64(resp["@OutNextId"]);

            if (nextId <= 0)
                throw new InvalidOperationException(
                    $"sp_GetNextRowId returned an invalid id ({nextId}) for table '{tableName}'.");

            return nextId;
        }
    }
}
