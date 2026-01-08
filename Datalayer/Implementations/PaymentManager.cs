using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Interfaces;
using Shared.Models;
using System.Data;
using System.Net;

namespace Datalayer.Implementations
{
    public class PaymentManager : BaseManager, IPaymentManager
    {
        private readonly ILogger<PaymentManager> _logger;
        private readonly IConnectionStringsManager _connection;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCacheManager _memoryCacheManager;

        public PaymentManager(ILogger<PaymentManager> logger, IRepository repository, IConnectionStringsManager connectionStringsManager, IMemoryCache memoryCache, IMemoryCacheManager memoryCacheManager)
        {
            _logger = logger;
            _repository = repository;
            _connection = connectionStringsManager;
            _memoryCache = memoryCache;
            _memoryCacheManager = memoryCacheManager;
        }

        public async Task<ResponseHandler<PaymentProvider>> FetchPaymentOptions()
        {
            try
            {
                if (!_memoryCache.TryGetValue("PaymentProviders", out ResponseHandler<PaymentProvider> repsMan))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = _repository.GetListAsync<PaymentProvider>(dbConnection,
                        "Select * from PaymentProvider where IsActive = 1", CommandType.Text).Result;

                    if (resi.Any())
                    {

                        repsMan = await Task.FromResult(new ResponseHandler<PaymentProvider>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });

                        await _memoryCacheManager.SetCache("PaymentProviders", repsMan);
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<PaymentProvider>
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Message = "Record not found"
                        });
                    }
                }

                return await Task.FromResult(repsMan);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(FetchPaymentOptions)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<PaymentProvider>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

    }
}
