using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Shared.Interfaces;
using Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datalayer.Implementations
{
    public class GenericManager : BaseManager, IGenericManager
    {
        private readonly ILogger<GenericManager> _logger;

        public GenericManager(ILogger<GenericManager> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task<long> GetNextTableId(IDbConnection dbConnection, IDbTransaction transaction, string tableName)
        {
            long nextId = 0;
            try
            {
                var inputParam = new Dictionary<string, object>
                {
                    { "@TableName", tableName }
                };

                var outputParam = new Dictionary<string, object>
                {
                    { "@OutNextId", string.Empty }
                };

                var resp = await _repository.ExecuteAsync<object>(dbConnection, transaction, DatabaseScripts.GetNextRowId, inputParam, outputParam, CommandType.StoredProcedure);
                nextId = Convert.ToInt64(resp["@OutNextId"]);

                if (nextId <= 0)
                {
                    transaction.Rollback();
                    return nextId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("", nameof(GetNextTableId), ex);
            }

            return nextId;
        }
    }
}
