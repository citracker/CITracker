using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Interfaces;
using Shared.Models;
using Shared.Utilities;
using System.Data;
using System.Net;

namespace Datalayer.Implementations
{
    public class UserManager : BaseManager, IUserManager
    {
        private readonly ILogger<UserManager> _logger;
        private readonly IGenericManager _genManager;

        public UserManager(ILogger<UserManager> logger, IRepository repository, IAppSettingsManager AppSettingsManager, IGenericManager genManager) : base (AppSettingsManager)
        {
            _logger = logger;
            _repository = repository;
            _connection = AppSettingsManager;
            _genManager = genManager;
        }

        public async Task<ResponseHandler<CIUserDTO>> GetUserByEmail(string email)
        {
            using var dbConnection = await OpenConnectionAsync();
                
            try
            {
                
                using var dbTransaction = dbConnection.BeginTransaction();
                var resi = await _repository.GetAsync<CIUserDTO>(dbConnection,
                    "SELECT a.Id, a.OrganizationId, a.Name, a.EmailAddress, a.Role, a.IsActive, a.HasCIAccess, a.HasOEAccess, a.HasSIAccess, b.TenantId as OrganizationTenantId, b.Domain as OrganizationDomain, b.IsSubscribed as IsOrganizationSubscribed, b.SubscriptionId from CIUser a left join Organization b on a.OrganizationId = b.id where a.EmailAddress = @em", new
                    {
                        em = email
                    }, CommandType.Text, dbTransaction);


                if (resi != null)
                {
                    return await Task.FromResult(new ResponseHandler<CIUserDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    //insert audit log here
                    var log = ModelBuilder.BuildAuditLog("Unregistered User SSO", "An unregistered user signed on to CITracker", email);
                    log.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var resp = await _repository.InsertAsync(dbConnection, log, dbTransaction);
                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler<CIUserDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetUserByEmail)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<CIUserDTO>
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

        public async Task<ResponseHandler<Organization>> GetOrganizationByTenant(string tenantId)
        {
            using var dbConnection = await OpenConnectionAsync();

            try
            {
                var resi = await _repository.GetAsync<Organization>(dbConnection,
                    "SELECT * from Organization where TenantId = @tid", new
                    {
                        tid = tenantId
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
                _logger.LogError($"Exception at {nameof(GetOrganizationByTenant)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<Organization>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

    }
}
