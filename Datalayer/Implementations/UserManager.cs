using Datalayer.Interfaces;
using DataRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Implementations;
using Shared.Interfaces;
using Shared.Models;
using Shared.Utilities;
using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Datalayer.Implementations
{
    public class UserManager : BaseManager, IUserManager
    {
        private readonly ILogger<UserManager> _logger;
        private readonly IConnectionStringsManager _connection;
        private readonly IGenericManager _genManager;

        public UserManager(ILogger<UserManager> logger, IRepository repository, IConnectionStringsManager connectionStringsManager, IGenericManager genManager)
        {
            _logger = logger;
            _repository = repository;
            _connection = connectionStringsManager;
            _genManager = genManager;
        }

        public async Task<ResponseHandler<CIUserDTO>> GetUserByEmail(string email)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                
            try
            {
                dbConnection.Open();
                using var dbTransaction = dbConnection.BeginTransaction();
                var resi = _repository.GetAsync<CIUserDTO>(dbConnection,
                    "SELECT a.Id, a.OrganizationId, a.Name, a.EmailAddress, a.Role, a.IsActive, b.TenantId as OrganizationTenantId, b.IsSubscribed as IsOrganizationSubscribed, b.SubscriptionId from CIUser a left join Organization b on a.OrganizationId = b.id where EmailAddress = @em", new
                    {
                        em = email
                    }, CommandType.Text, dbTransaction).Result;


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
                    log.Id = _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable).Result;
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

    }
}
