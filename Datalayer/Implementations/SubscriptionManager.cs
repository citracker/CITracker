using Datalayer.Interfaces;
using DataRepository;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
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
using System.Data.Common;
using System.Net;

namespace Datalayer.Implementations
{
    public class SubscriptionManager : BaseManager, ISubscriptionManager
    {
        private readonly ILogger<SubscriptionManager> _logger;
        private readonly IAppSettingsManager _connection;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCacheManager _memoryCacheManager;
        private readonly IGenericManager _genManager;

        public SubscriptionManager(ILogger<SubscriptionManager> logger, IRepository repository, IAppSettingsManager AppSettingsManager, IMemoryCache memoryCache, IMemoryCacheManager memoryCacheManager, IGenericManager genManager)
        {
            _logger = logger;
            _repository = repository;
            _connection = AppSettingsManager;
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
                    var resi = await _repository.GetListAsync<SubscriptionPlan>(dbConnection,
                        "Select * from SubscriptionPlan", CommandType.Text);

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
                var resi = await _repository.GetAsync<Organization>(dbConnection,
                    "Select * from Organization where tenantId = @tId", new
                    {
                        tId = tenantId
                    }, CommandType.Text);

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

        private async Task<ResponseHandler<Organization>> GetOrganizationById(int Id)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<Organization>(dbConnection,
                    "Select * from Organization where Id = @id", new
                    {
                        id = Id
                    }, CommandType.Text);

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
                _logger.LogError($"Exception at {nameof(GetOrganizationById)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<Organization>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OrganizationSubscription>> GetOrganizationSubscription(string tenantId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<OrganizationSubscription>(dbConnection,
                    "SELECT o.Id AS OrganizationId, s.SubscriptionPlanId, s.Status as SubscriptionStatus, CAST(s.StartDate AS DATETIME) AS StartDate, CAST(s.EndDate AS DATETIME) AS EndDate, sp.NumberOfLicences, COUNT(u.Id) AS NumberOfUsedLicences FROM Organization o INNER JOIN Subscription s ON o.SubscriptionId = s.Id INNER JOIN SubscriptionPlan sp ON s.SubscriptionPlanId = sp.Id LEFT JOIN CIUser u ON u.OrganizationId = o.Id AND u.IsActive = 1 WHERE o.TenantId = @tid AND o.IsSubscribed = 1 GROUP BY o.Id, s.SubscriptionPlanId, s.Status, s.StartDate, s.EndDate, sp.NumberOfLicences", new
                    {
                        tid = tenantId
                    }, CommandType.Text);

                if (resi != null)
                {

                    return await Task.FromResult(new ResponseHandler<OrganizationSubscription>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationSubscription>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetOrganizationSubscription)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<OrganizationSubscription>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task UpdateOrganizationSubscription(long orgId, string stripeCustomerId, string subStatus, long adminUser)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<Subscription>(dbConnection,
                    "SELECT * from Subscription where OrganizationId = @oid", new
                    {
                        oid = orgId
                    }, CommandType.Text);

                if (resi != null)
                {
                    resi.Status = subStatus;
                    resi.PaymentCustomerId = stripeCustomerId;
                    resi.LastUpdatedBy = adminUser;
                    resi.LastUpdatedDate = DateTime.UtcNow;

                    var updRes = await _repository.UpdateAsync(dbConnection, resi);
                    _logger.LogInformation($"Subscription update for orgId {orgId} was successful. Result: {updRes}");
                }
                else
                {
                    _logger.LogInformation($"Couldn't fetch Subscription information for orgId {orgId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateOrganizationSubscription)} - {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task UpdateOrganizationSubscriptionFromEvent(int clientReferenceId, string stripeCustomerId, string subscriptionId, string subscriptionStatus)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<Subscription>(dbConnection,
                    "SELECT * from Subscription where OrganizationId = @oid and PaymentCustomerId = @pid", new
                    {
                        oid = clientReferenceId,
                        pid = stripeCustomerId
                    }, CommandType.Text);

                if (resi != null)
                {
                    if(!resi.Status.ToLower().Equals("active"))
                        resi.Status = subscriptionStatus;
                    resi.PaymentCustomerId = stripeCustomerId;
                    resi.PaymentSubscriptionId = subscriptionId;
                    resi.LastUpdatedDate = DateTime.UtcNow;

                    var updRes = await _repository.UpdateAsync(dbConnection, resi);
                    _logger.LogInformation($"Subscription update for orgId {clientReferenceId} is now {subscriptionStatus}. Result: {updRes}");
                }
                else
                {
                    _logger.LogInformation($"Couldn't fetch Subscription information for orgId {clientReferenceId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateOrganizationSubscriptionFromEvent)} - {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task UpdateOrganizationSubscriptionFromUpdatedEvent(string subscriptionId, string stripeCustomerId, DateTime? startDate, DateTime? endDate, string priceId, string subscriptionStatus)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<Subscription>(dbConnection,
                    "SELECT * from Subscription where PaymentCustomerId = @pid", new
                    {
                        pid = stripeCustomerId
                    }, CommandType.Text);

                if (resi != null)
                {
                    resi.PaymentSubscriptionId = subscriptionId;
                    resi.StartDate = (DateTime) startDate;
                    resi.EndDate = (DateTime)startDate;
                    resi.Status = subscriptionStatus;
                    resi.LastUpdatedDate = DateTime.UtcNow;

                    //fetch subscriptionPlan for this subscription.
                    var subPlan = await _repository.GetAsync<SubscriptionPlan>(dbConnection,
                        "SELECT * from SubscriptionPlan where Id = @subId", new
                        {
                            subId = resi.SubscriptionPlanId
                        }, CommandType.Text);

                    if (subPlan != null)
                    {
                        if(subPlan.PriceId != priceId)
                        {
                            _logger.LogInformation($"Subscription plan priceId {priceId} does not match with the one on record for subscriptionId {subscriptionId}. Fetching subscription plan with priceId {priceId}.");
                            var subPlanWithPrice = await _repository.GetAsync<SubscriptionPlan>(dbConnection,
                                "SELECT * from SubscriptionPlan where PriceId = @pId", new
                                {
                                    pId = priceId
                                }, CommandType.Text);
                            if(subPlanWithPrice != null)
                            {
                                resi.SubscriptionPlanId = subPlanWithPrice.Id;
                                _logger.LogInformation($"Subscription plan with priceId {priceId} has Id {subPlanWithPrice.Id}. Updating subscription plan for subscriptionId {subscriptionId} to {subPlanWithPrice.Id}.");
                            }
                            else
                            {
                                _logger.LogInformation($"Couldn't fetch subscription plan with priceId {priceId}. Subscription plan update for subscriptionId {subscriptionId} will be skipped.");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"Couldn't fetch subscription plan with Id {resi.SubscriptionPlanId} for Organization Id {resi.OrganizationId}.");
                    }

                    var updRes = await _repository.UpdateAsync(dbConnection, resi);
                    _logger.LogInformation($"Subscription update for SubscriptionId {subscriptionId} is now {subscriptionStatus}. Result: {updRes}");
                }
                else
                {
                    _logger.LogInformation($"Couldn't fetch Subscription information for SubscriptionId {subscriptionId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateOrganizationSubscriptionFromUpdatedEvent)} - {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task UpdateOrganizationSubscriptionFromDeletedEvent(string subscriptionId, string subscriptionStatus)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<Subscription>(dbConnection,
                    "SELECT * from Subscription where PaymentSubscriptionId = @psid", new {psid = subscriptionId}, CommandType.Text);

                if (resi != null)
                {
                    resi.Status = subscriptionStatus;
                    resi.Status = subscriptionStatus;
                    resi.LastUpdatedDate = DateTime.UtcNow;

                    var updRes = await _repository.UpdateAsync(dbConnection, resi);
                    _logger.LogInformation($"Subscription update for SubscriptionId {subscriptionId} is now {subscriptionStatus}. Result: {updRes}");
                }
                else
                {
                    _logger.LogInformation($"Couldn't fetch Subscription information for SubscriptionId {subscriptionId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateOrganizationSubscriptionFromDeletedEvent)} - {JsonConvert.SerializeObject(ex)}");
            }
        }

        public async Task<ResponseHandler<SubscriptionPlan>> GetSubscriptionPlanById(int id)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<SubscriptionPlan>(dbConnection,
                    "Select * from SubscriptionPlan where Id = @subId", new
                    {
                        subId = id
                    }, CommandType.Text);

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

        public async Task<ResponseHandler> RegisterOrganizationSubscription(Organization org, CIUser usr, Subscription sub)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();

            //TODO. Check for duplicate
            try
            {
                //create organisation
                org.Id = (int) _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationTable).Result;
                org.CreatedBy = org.Id;
                var orgRes = await _repository.InsertAsync(dbConnection, org, dbTransaction);

                //create organisation user
                usr.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIUserTable);
                usr.OrganizationId = org.Id;
                usr.CreatedBy = org.Id;
                usr.IsActive = true;
                var usrRes =  await _repository.InsertAsync(dbConnection, usr, dbTransaction);

                //create subscription record
                sub.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.SubscriptionTable);
                sub.OrganizationId = org.Id;
                sub.CreatedBy = org.Id;
                var subRes = await _repository.InsertAsync(dbConnection, sub, dbTransaction);

                org.IsSubscribed = false;
                org.SubscriptionId = sub.Id;
                var ordUpdRes = await _repository.UpdateAsync(dbConnection, org, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Organization Created", $"{org.Name} was registered.", org.AdminEmailAddress);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                var audit1 = ModelBuilder.BuildAuditLog("Admin User Created", $"{org.Name} admin user was registered.", org.AdminEmailAddress);
                audit1.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var audit1Res = await _repository.InsertAsync(dbConnection, audit1, dbTransaction);

                dbTransaction.Commit();

                return new ResponseHandler
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Message = "Organization registration was successful"
                };
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Duplicate admin email insert detected
                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.ExpectationFailed,
                    Message = "Admin Email Exists"
                });
            }
            catch (Exception ex)
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

        public async Task<ResponseHandler<Organization>> UpdateOrganizationSubscriptionFromPaymentSuceededEvent(string subscriptionId, string stripeCustomerId, DateTime? startDate, DateTime? endDate, string subscriptionStatus, decimal amount, string provider, string invoiceId, string paymentIntentId)
        {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                dbConnection.Open();
                using var dbTransaction = dbConnection.BeginTransaction();
            try
            {

                var resi = await _repository.GetAsync<Subscription>(dbConnection,
                    "SELECT * from Subscription where PaymentSubscriptionId = @psid and PaymentCustomerId = @pid", new
                    {
                        psid = subscriptionId,
                        pid = stripeCustomerId
                    }, CommandType.Text, dbTransaction);

                if (resi != null)
                {
                    resi.StartDate = (DateTime)startDate;
                    resi.EndDate = (DateTime)endDate;
                    resi.Status = subscriptionStatus;
                    resi.LastUpdatedDate = DateTime.UtcNow;
                                        
                    var updRes = await _repository.UpdateAsync(dbConnection, resi, dbTransaction);
                    _logger.LogInformation($"Subscription update for SubscriptionId {subscriptionId} is now {subscriptionStatus}. Result: {updRes}");

                    //create payment object for this subscription
                    var pay = new Payment
                    {
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.PaymentTable),
                        Amount = amount,
                        Provider = provider,
                        Reference = $"{invoiceId}||{paymentIntentId}" ,
                        SubscriptionId = resi.Id,
                        DateCreated = DateTime.UtcNow,
                        OrganizationId = resi.OrganizationId,
                        CreatedBy = resi.OrganizationId
                    };

                    var payRes = await _repository.InsertAsync(dbConnection, pay, dbTransaction);

                    var org = await GetOrganizationById(Convert.ToInt32(resi.OrganizationId));

                    //update subscription status as true                    
                    org.SingleResult.IsSubscribed = true;

                    _logger.LogInformation($"Organization's ({org.SingleResult.Name}) update Result is. Result: {await _repository.UpdateAsync(dbConnection, org.SingleResult, dbTransaction)}");

                    var audit2 = ModelBuilder.BuildAuditLog("Payment Made", $"{org.SingleResult.Name} made a subscription payment.", org.SingleResult.AdminEmailAddress);
                    audit2.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var audit2Res = await _repository.InsertAsync(dbConnection, audit2, dbTransaction);

                    _logger.LogInformation($"Organization's ({org.SingleResult.Name}) Subscription update for SubscriptionId {subscriptionId} is now {subscriptionStatus}. Result: {payRes}");

                    dbTransaction.Commit();

                    return org;
                }
                else
                {
                    _logger.LogInformation($"Couldn't fetch Subscription information for SubscriptionId {subscriptionId}.");
                    return await Task.FromResult(new ResponseHandler<Organization>
                    {
                        StatusCode = (int)HttpStatusCode.ExpectationFailed,
                        Message = "Couldn't fetch Subscription information for SubscriptionId {subscriptionId}."
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateOrganizationSubscriptionFromPaymentSuceededEvent)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<Organization>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
            finally
            {
                dbConnection.Close();
            }
        }
    }
}
