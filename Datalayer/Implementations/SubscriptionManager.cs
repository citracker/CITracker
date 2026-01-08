using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Implementations;
using Shared.Interfaces;
using Shared.Models;
using Shared.Utilities;
using System.Data;
using System.Net;
using System.Security.AccessControl;

namespace Datalayer.Implementations
{
    public class SubscriptionManager : BaseManager, ISubscriptionManager
    {
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly IConnectionStringsManager _connection;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCacheManager _memoryCacheManager;
        private readonly IGenericManager _genManager;

        public SubscriptionManager(ILogger<SubscriptionManager> logger, IRepository repository, IConnectionStringsManager connectionStringsManager, IMemoryCache memoryCache, IMemoryCacheManager memoryCacheManager, IGenericManager genManager)
        {
            _logger = logger;
            _repository = repository;
            _connection = connectionStringsManager;
            _memoryCache = memoryCache;
            _memoryCacheManager = memoryCacheManager;
            _genManager = genManager;
        }

        public async Task<ResponseHandler<SubscriptionPlan>> GetAllSubscriptionPlans()
        {
            try
            {
                if (!_memoryCache.TryGetValue("SubscriptionPlans", out ResponseHandler<SubscriptionPlan> subsPlan))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = _repository.GetListAsync<SubscriptionPlan>(dbConnection,
                        "Select * from SubscriptionPlan", CommandType.Text).Result;

                    if (resi.Any())
                    {

                        subsPlan = await Task.FromResult(new ResponseHandler<SubscriptionPlan>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });                    

                        await _memoryCacheManager.SetCache("SubscriptionPlans", subsPlan);
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<SubscriptionPlan>
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Message = "Record not found"
                        });
                    }
                }

                return await Task.FromResult(subsPlan);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllSubscriptionPlans)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<SubscriptionPlan>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<Organization>> GetOrganizationByTenantId(string tenantId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = _repository.GetAsync<Organization>(dbConnection,
                    "Select * from Organization where tenantId = @tId", new
                    {
                        tId = tenantId
                    }, CommandType.Text).Result;

                if (resi != null)
                {

                    return await Task.FromResult(new ResponseHandler<Organization>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<Organization>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetOrganizationByTenantId)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<Organization>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<SubscriptionPlan>> GetSubscriptionPlanById(int id)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = _repository.GetAsync<SubscriptionPlan>(dbConnection,
                    "Select * from SubscriptionPlan where Id = @subId", new
                    {
                        subId = id
                    }, CommandType.Text).Result;

                if (resi != null)
                {

                    return await Task.FromResult(new ResponseHandler<SubscriptionPlan>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<SubscriptionPlan>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetSubscriptionPlanById)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<SubscriptionPlan>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> RegisterOrganizationSubscription(Organization org, CIUser usr, Payment pay, Subscription sub)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //create organisation
                org.Id = (int) _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationTable).Result;
                org.CreatedBy = org.Id;
                var orgRes = await _repository.InsertAsync(dbConnection, org, dbTransaction);

                //create organisation user
                usr.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIUserTable).Result;
                usr.OrganizationId = org.Id;
                usr.CreatedBy = org.Id;
                usr.IsActive = true;
                var usrRes =  await _repository.InsertAsync(dbConnection, usr, dbTransaction);

                //create payment record
                pay.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.PaymentTable).Result;
                pay.OrganizationId = org.Id;
                pay.CreatedBy = org.Id;
                var payRes = await _repository.InsertAsync(dbConnection, pay, dbTransaction);

                //create subscription record
                sub.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.SubscriptionTable).Result;
                sub.OrganizationId = org.Id;
                sub.CreatedBy = org.Id;
                var subRes = await _repository.InsertAsync(dbConnection, sub, dbTransaction);

                org.IsSubscribed = true;
                org.SubscriptionId = sub.Id;
                var ordUpdRes = await _repository.UpdateAsync(dbConnection, org, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Organization Created", $"{org.Name} was registered.", org.AdminEmailAddress);
                audit.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable).Result;
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                var audit1 = ModelBuilder.BuildAuditLog("Admin User Created", $"{org.Name} admin user was registered.", org.AdminEmailAddress);
                audit1.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable).Result;
                var audit1Res = await _repository.InsertAsync(dbConnection, audit1, dbTransaction);

                var audit2 = ModelBuilder.BuildAuditLog("Payment Made", $"{org.Name} made a subscription payment.", org.AdminEmailAddress);
                audit2.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable).Result;
                var audit2Res = await _repository.InsertAsync(dbConnection, audit2, dbTransaction);

                dbTransaction.Commit();

                return new ResponseHandler
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Message = "Organization registration was successful"
                };
            }
            catch(Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(RegisterOrganizationSubscription)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
            finally
            {
                dbConnection.Close();
            }
        }
    }
}
