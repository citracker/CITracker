using Dapper;
using Dapper.Contrib.Extensions;
using Datalayer.Interfaces;
using DataRepository;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Filters;
using Shared.DTO;
using Shared.Enumerations;
using Shared.Interfaces;
using Shared.Models;
using Shared.Request;
using Shared.Utilities;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Datalayer.Implementations
{
    public class OperationManager : BaseManager, IOperationManager
    {
        private readonly ILogger<OperationManager> _logger;
        private readonly IConnectionStringsManager _connection;
        private readonly IMemoryCache _memoryCache;
        private readonly IMemoryCacheManager _memoryCacheManager;
        private readonly IGenericManager _genManager;

        public OperationManager(ILogger<OperationManager> logger, IRepository repository, IConnectionStringsManager connectionStringsManager, IMemoryCache memoryCache, IMemoryCacheManager memoryCacheManager, IGenericManager genManager)
        {
            _logger = logger;
            _repository = repository;
            _connection = connectionStringsManager;
            _memoryCache = memoryCache;
            _memoryCacheManager = memoryCacheManager;
            _genManager = genManager;
        }

        public async Task<ResponseHandler<Country>> FetchOperationalCountry()
        {
            try
            {
                if (!_memoryCache.TryGetValue("Country", out ResponseHandler<Country> country))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = await _repository.GetListAsync<Country>(dbConnection,"Select * from Country", CommandType.Text);

                    if (resi.Any())
                    {

                        country = await Task.FromResult(new ResponseHandler<Country>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });

                        await _memoryCacheManager.SetCache("Country", country);
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<Country>
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Message = "Record not found"
                        });
                    }
                }

                return await Task.FromResult(country);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(FetchOperationalCountry)} - {JsonConvert.SerializeObject(ex)}");

                return await Task.FromResult(new ResponseHandler<Country>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OrganizationCountry>> GetAllOrganizationCountries(int orgId)
        {
            try
            {
                //if (!_memoryCache.TryGetValue($"OrganizationCountry-{orgId}", out ResponseHandler<OrganizationCountry> country))
                //{
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<OrganizationCountry>(dbConnection,
                    "Select * from OrganizationCountry where OrganizationId = @oid and IsActive = 1 order by Country", new { oid = orgId }, CommandType.Text);

                if (resi.Any())
                {

                    return await Task.FromResult(new ResponseHandler<OrganizationCountry>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });

                    //await _memoryCacheManager.SetCache($"OrganizationCountry-{orgId}", country);
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationCountry>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
                //}

                //return await Task.FromResult(country);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOrganizationCountries)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OrganizationCountry>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OrganizationFacility>> GetAllOrganizationFacilities(int orgId)
        {
            try
            {
                //if (!_memoryCache.TryGetValue($"OrganizationFacility-{orgId}", out ResponseHandler<OrganizationFacility> facility))
                //{
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<OrganizationFacility>(dbConnection,
                    "Select * from OrganizationFacility where OrganizationId = @oid and IsActive = 1 order by Facility", new { oid = orgId }, CommandType.Text);

                if (resi.Any())
                {

                    return await Task.FromResult(new ResponseHandler<OrganizationFacility>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });

                    //await _memoryCacheManager.SetCache($"OrganizationFacility-{orgId}", facility);
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationFacility>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
                //}

                //return await Task.FromResult(facility);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOrganizationFacilities)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OrganizationFacility>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OrganizationDepartment>> GetAllOrganizationDepartments(int orgId)
        {
            try
            {
                //if (!_memoryCache.TryGetValue($"OrganizationDepartment-{orgId}", out ResponseHandler<OrganizationDepartment> depart))
                //{
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<OrganizationDepartment>(dbConnection,
                    "Select * from OrganizationDepartment where OrganizationId = @oid and IsActive = 1 order by Department", new { oid = orgId }, CommandType.Text);

                if (resi.Any())
                {

                    return await Task.FromResult(new ResponseHandler<OrganizationDepartment>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });

                    //await _memoryCacheManager.SetCache($"OrganizationDepartment-{orgId}", depart);
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationDepartment>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
                //}

                //return await Task.FromResult(depart);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOrganizationDepartments)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OrganizationDepartment>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<CIUser>> GetAllOrganizationUsers(int orgId)
        {
            try
            {
                //if (!_memoryCache.TryGetValue($"CIUser-{orgId}", out ResponseHandler<CIUser> users))
                //{
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<CIUser>(dbConnection,
                "Select * from CIUser where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text);

                if (resi.Any())
                {

                    return await Task.FromResult(new ResponseHandler<CIUser>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });

                    //await _memoryCacheManager.SetCache($"CIUser-{orgId}", users);
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CIUser>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
                //}

                //return await Task.FromResult(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOrganizationUsers)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<CIUser>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> AddOrganizationCountry(OrganizationCountry orgCountry, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                orgCountry.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationCountryTable);
                var resp = await _repository.InsertAsync(dbConnection, orgCountry, dbTransaction);
                
                var audit = ModelBuilder.BuildAuditLog("Country Added", $"Company Admin added new Organization Country of operation.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                //UpdateCountryListInMemory(dbConnection, dbTransaction, orgCountry.OrganizationId);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationCountry)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> RenameOrganizationCountry(long countryId, string countryName, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.GetAsync<OrganizationCountry>(dbConnection,
                    "Select * from OrganizationCountry where Id = @cid", new { cid = countryId }, CommandType.Text, dbTransaction);

                if (resi != null)
                {
                    resi.Country = countryName;
                    var res = await _repository.UpdateAsync(dbConnection, resi, dbTransaction);


                    var audit = ModelBuilder.BuildAuditLog("Country Renamed", $"Company Admin renamed organization depart Id '{resi.Id}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateCountryListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record updated Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch(Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(RenameOrganizationCountry)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> DeleteOrganizationCountry(long countryId, string adminEmail, int orgId)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.ExecuteAsync(dbConnection,
                    "Update OrganizationCountry set IsActive = 0 where Id = @cid", new { cid = countryId }, CommandType.Text, dbTransaction);

                if (resi > 0)
                {
                    var audit = ModelBuilder.BuildAuditLog("Country Deleted", $"Company Admin deleted organization depart Id '{countryId}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateCountryListInMemory(dbConnection, dbTransaction, orgId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record deleted Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(DeleteOrganizationCountry)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddOrganizationFacility(OrganizationFacility orgFacility, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                orgFacility.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationFacilityTable);
                var resp = await _repository.InsertAsync(dbConnection, orgFacility, dbTransaction);
                
                var audit = ModelBuilder.BuildAuditLog("Facility Added", $"Company Admin added new Organization Facility of operation.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                //UpdateFacilityListInMemory(dbConnection, dbTransaction, orgFacility.OrganizationId);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationFacility)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> RenameOrganizationFacility(long facilityId, string facilityName, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.GetAsync<OrganizationFacility>(dbConnection,
                    "Select * from OrganizationFacility where Id = @cid", new { cid = facilityId }, CommandType.Text, dbTransaction);

                if (resi != null)
                {
                    resi.Facility = facilityName;
                    var res = await _repository.UpdateAsync(dbConnection, resi, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Facility Renamed", $"Company Admin renamed organization facility Id '{resi.Id}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateFacilityListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record updated Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(RenameOrganizationFacility)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> DeleteOrganizationFacility(long facilityId, string adminEmail, int orgId)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.ExecuteAsync(dbConnection,
                    "Update OrganizationFacility set IsActive = 0 where Id = @cid", new { cid = facilityId }, CommandType.Text, dbTransaction);

                if (resi > 0)
                {
                    var audit = ModelBuilder.BuildAuditLog("Facility Deleted", $"Company Admin deleted organization facility Id '{facilityId}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateFacilityListInMemory(dbConnection, dbTransaction, orgId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record deleted Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(DeleteOrganizationFacility)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddOrganizationDepartment(OrganizationDepartment orgDepartment, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                orgDepartment.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationDepartmentTable);
                var resp = await _repository.InsertAsync(dbConnection, orgDepartment, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Department Added", $"Company Admin added new Organization Department of operation.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                //UpdateDepartmentListInMemory(dbConnection, dbTransaction, orgDepartment.OrganizationId);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationDepartment)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> RenameOrganizationDepartment(long departmentId, string departmentName, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.GetAsync<OrganizationDepartment>(dbConnection,
                    "Select * from OrganizationDepartment where Id = @cid", new { cid = departmentId }, CommandType.Text, dbTransaction);

                if (resi != null)
                {
                    resi.Department = departmentName;
                    var res = await _repository.UpdateAsync(dbConnection, resi, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Department Renamed", $"Company Admin renamed organization department Id '{resi.Id}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateDepartmentListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record updated Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(RenameOrganizationDepartment)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> DeleteOrganizationDepartment(long departmentId, string adminEmail, int orgId)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.ExecuteAsync(dbConnection,
                    "Update OrganizationDepartment set IsActive = 0 where Id = @cid", new { cid = departmentId }, CommandType.Text, dbTransaction);

                if (resi > 0)
                {
                    var audit = ModelBuilder.BuildAuditLog("Department Deleted", $"Company Admin deleted organization department Id '{departmentId}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateDepartmentListInMemory(dbConnection, dbTransaction, orgId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record deleted Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(DeleteOrganizationDepartment)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddOrganizationUser(CIUser orgUsr, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                orgUsr.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIUserTable);
                var resp = await _repository.InsertAsync(dbConnection, orgUsr, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("User Added", $"Company Admin added new Organization User.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                //UpdateUserListInMemory(dbConnection, dbTransaction, orgUsr.OrganizationId);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationUser)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> RenameOrganizationUser(long usrId, CIUser usr, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.GetAsync<CIUser>(dbConnection,
                    "Select * from CIUser where Id = @cid", new { cid = usrId }, CommandType.Text, dbTransaction);

                if (resi != null)
                {
                    resi.EmailAddress = usr.EmailAddress;
                    resi.Name = usr.Name;
                    var res = await _repository.UpdateAsync(dbConnection, resi, dbTransaction);


                    var audit = ModelBuilder.BuildAuditLog("User Renamed", $"Company Admin renamed organization User '{resi.Id}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateUserListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record updated Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(RenameOrganizationUser)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> DeleteOrganizationUser(long usrId, string adminEmail, int orgId)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.ExecuteAsync(dbConnection,
                    "Update CIUser set IsActive = 0 where Id = @cid", new { cid = usrId }, CommandType.Text, dbTransaction);

                if (resi > 0)
                {
                    var audit = ModelBuilder.BuildAuditLog("User Deleted", $"Company Admin deleted organization User '{usrId}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    //UpdateUserListInMemory(dbConnection, dbTransaction, orgId);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record deleted Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(DeleteOrganizationUser)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        private async void UpdateCountryListInMemory(IDbConnection dbConnection, IDbTransaction dbTransaction, int orgId)
        {

            var re = await _repository.GetListAsync<OrganizationCountry>(dbConnection,
                "Select * from OrganizationCountry where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text, dbTransaction);

            if (re.ToList().Any())
            {
                var ctry = await Task.FromResult(new ResponseHandler<OrganizationCountry>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful",
                    Result = re
                });

                await _memoryCacheManager.SetCache($"OrganizationCountry-{orgId}", ctry);
            }
        }

        private async void UpdateFacilityListInMemory(IDbConnection dbConnection, IDbTransaction dbTransaction, int orgId)
        {

            var re = await _repository.GetListAsync<OrganizationFacility>(dbConnection,
                "Select * from OrganizationFacility where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text, dbTransaction);

            if (re.ToList().Any())
            {
                var ctry = await Task.FromResult(new ResponseHandler<OrganizationFacility>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful",
                    Result = re
                });

                await _memoryCacheManager.SetCache($"OrganizationFacility-{orgId}", ctry);
            }
        }

        private async void UpdateDepartmentListInMemory(IDbConnection dbConnection, IDbTransaction dbTransaction, int orgId)
        {

            var re = await _repository.GetListAsync<OrganizationDepartment>(dbConnection,
                "Select * from OrganizationDepartment where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text, dbTransaction);

            if (re.ToList().Any())
            {
                var ctry = await Task.FromResult(new ResponseHandler<OrganizationDepartment>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful",
                    Result = re
                });

                await _memoryCacheManager.SetCache($"OrganizationDepartment-{orgId}", ctry);
            }
        }

        private async void UpdateUserListInMemory(IDbConnection dbConnection, IDbTransaction dbTransaction, int orgId)
        {
            var re = await _repository.GetListAsync<CIUser>(dbConnection,
                "Select * from CIUser where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text, dbTransaction);

            if (re.ToList().Any())
            {
                var ctry = await Task.FromResult(new ResponseHandler<CIUser>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful",
                    Result = re
                });

                await _memoryCacheManager.SetCache($"CIUser-{orgId}", ctry);
            }
        }

        public async Task<ResponseHandler> CreateNewOEProject(OperationalExcellence opExel, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                opExel.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OperationalExcellenceTable);
                var resp = await _repository.InsertAsync(dbConnection, opExel, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Operational Excellence Initiative Added", $"Company Admin added new Operational Excellence Initiative.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewOEProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<OperationalExcellenceDTO>> GetPaginatedOEProjects(int orgId, int pageNumber, int pageSize, InitiativeFilter filt)
        {
            try
            {
                IEnumerable<OperationalExcellenceDTO> resi = null; int count = 0;
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var query = "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Description, a.FacilitatorId, b.Name as Facilitator, a.SponsorId, b1.Name as Sponsor, a.ExecutiveSponsorId, b2.Name as ExecutiveSponsor, a.CarryOverProject, a.SavingsClassification, a.TargetSavings, a.Currency, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, a.Status, e.Department as OrganizationDepartment, a.CreatedBy, b3.Name as CreatedByStaff, (select SUM(Savings) from OperationalExcellenceMonthlySaving where ProjectId = a.Id) as ActualSavings FROM OperationalExcellence a left join CIUser b on a.FacilitatorId = b.Id left join CIUser b1 on a.SponsorId = b1.Id left join CIUser b2 on a.ExecutiveSponsorId = b2.Id left join CIUser b3 on a.CreatedBy = b3.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id where a.OrganizationId = @oid and a.Status NOT IN ('CLOSED', 'CANCELLED') @where ORDER BY a.DateCreated DESC OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                var countquery = "SELECT count(id) from OperationalExcellence where OrganizationId = @oid and Status NOT IN ('CLOSED', 'CANCELLED') @where";

                if (filt == null || (filt.StartDate == new DateTime() && filt.EndDate == new DateTime() && String.IsNullOrEmpty(filt.Title) && filt.CountryId == 0 && filt.DepartmentId == 0 && String.IsNullOrEmpty(filt.Priority) && filt.UserId == 0))
                {
                    resi = await _repository.GetListAsync<OperationalExcellenceDTO>(dbConnection, query.Replace("@where", ""), new { oid = orgId, pageNumber, pageSize }, CommandType.Text);

                    count = await _repository.GetSumOrCountAsync<int>(dbConnection, countquery.Replace("@where", ""), new { oid = orgId }, CommandType.Text);
                }
                else
                {
                    var where = new StringBuilder();
                    var where1 = new StringBuilder();
                    var parameters = new DynamicParameters();

                    if (!string.IsNullOrWhiteSpace(filt.Title))
                    {
                        where.Append(" AND a.Title LIKE @Title");
                        where1.Append(" AND Title LIKE @Title");
                        parameters.Add("@Title", $"%{filt.Title.Trim()}%");
                    }

                    if (!string.IsNullOrWhiteSpace(filt.Status))
                    {
                        where.Append(" AND a.Status = @Stat");
                        where1.Append(" AND Status LIKE @Stat");
                        parameters.Add("@Stat", $"%{filt.Status.Trim()}%");
                    }

                    if (!string.IsNullOrWhiteSpace(filt.Priority))
                    {
                        where.Append(" AND a.Priority = @Priority");
                        where1.Append(" AND Priority = @Priority");
                        parameters.Add("@Priority", filt.Priority.Trim());
                    }

                    if (filt.UserId > 0)
                    {
                        where.Append(" AND (a.FacilitatorId = @UserId OR a.SponsorId = @UserId OR a.ExecutiveSponsorId = @UserId)");
                        where1.Append(" AND (FacilitatorId = @UserId OR SponsorId = @UserId OR ExecutiveSponsorId = @UserId)");
                        parameters.Add("@UserId", filt.UserId);
                    }

                    if (filt.CountryId > 0)
                    {
                        where.Append(" AND a.OrganizationCountryId = @CountryId");
                        where1.Append(" AND OrganizationCountryId = @CountryId");
                        parameters.Add("@CountryId", filt.CountryId);
                    }

                    if (filt.DepartmentId > 0)
                    {
                        where.Append(" AND a.OrganizationDepartmentId = @DepartmentId");
                        where1.Append(" AND OrganizationDepartmentId = @DepartmentId");
                        parameters.Add("@DepartmentId", filt.DepartmentId);
                    }

                    if (filt.StartDate != DateTime.MinValue)
                    {
                        where.Append(" AND a.StartDate >= @StartDate");
                        where1.Append(" AND StartDate >= @StartDate");
                        parameters.Add("@StartDate", filt.StartDate.Date);
                    }

                    if (filt.EndDate != DateTime.MinValue)
                    {
                        where.Append(" AND a.EndDate <= @EndDate");
                        where1.Append(" AND EndDate <= @EndDate");
                        parameters.Add("@EndDate", filt.EndDate.Date);
                    }

                    var finalQuery = query.Replace("@where", where.ToString());


                    parameters.Add("@oid", orgId);

                    var finalcountquery = countquery.Replace("@where", where1.ToString());

                    count = await _repository.GetSumOrCountAsync<int>(dbConnection, finalcountquery, parameters, CommandType.Text);

                    parameters.Add("@pageNumber", pageNumber);
                    parameters.Add("@pageSize", pageSize);

                    resi = await _repository.GetListAsync<OperationalExcellenceDTO>(dbConnection, finalQuery, parameters, CommandType.Text);
                }

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi,
                        TotalRecords = count,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetPaginatedOEProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OperationalExcellenceDTO>> GetOEProject(int orgId, long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                
                var resi = await _repository.GetAsync<OperationalExcellenceDTO>(dbConnection,
                "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Description, a.FacilitatorId, b.Name as Facilitator, a.SponsorId, b1.Name as Sponsor, a.ExecutiveSponsorId, b2.Name as ExecutiveSponsor, a.CarryOverProject, a.SavingsClassification, a.TargetSavings, a.Currency, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, a.Status, e.Department as OrganizationDepartment, a.CreatedBy, b3.Name as CreatedByStaff, (select SUM(Savings) from OperationalExcellenceMonthlySaving where ProjectId = a.Id) as ActualSavings FROM OperationalExcellence a left join CIUser b on a.FacilitatorId = b.Id left join CIUser b1 on a.SponsorId = b1.Id left join CIUser b2 on a.ExecutiveSponsorId = b2.Id left join CIUser b3 on a.CreatedBy = b3.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id where a.OrganizationId = @oid and a.Id = @pid", new { oid = orgId, pid = projectId }, CommandType.Text);

                if (resi != null)
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetOEProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<CIUser>> GetAllOperationalExcellenceUsers(int orgId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<CIUser>(dbConnection,
                "SELECT u.Id, u.Name FROM CIUser u WHERE u.Id IN (SELECT DISTINCT UserId FROM (SELECT SponsorId AS UserId FROM OperationalExcellence where OrganizationId = @orgId UNION ALL SELECT ExecutiveSponsorId FROM OperationalExcellence where OrganizationId = @orgId UNION ALL SELECT FacilitatorId FROM OperationalExcellence where OrganizationId = @orgId ) x )", new { orgId }, CommandType.Text);


                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<CIUser>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CIUser>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOperationalExcellenceUsers)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<CIUser>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<CIUser>> GetAllStrategicInitiativeUsers(int orgId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<CIUser>(dbConnection,
                "SELECT u.Id, u.Name FROM CIUser u WHERE u.Id IN (SELECT DISTINCT UserId FROM (SELECT ExecutiveSponsorId As UserId FROM StrategicInitiative where OrganizationId = @orgId UNION ALL SELECT OwnerId FROM StrategicInitiative where OrganizationId = @orgId ) x )", new { orgId }, CommandType.Text);


                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<CIUser>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CIUser>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllStrategicInitiativeUsers)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<CIUser>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> UpdateExistingOEProject(OperationalExcellence opExel, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var re = await _repository.GetAsync<OperationalExcellenceDTO>(dbConnection,
                "SELECT * FROM OperationalExcellence where OrganizationId = @oid and Id = @pid", new { oid = opExel.OrganizationId, pid = opExel.Id }, CommandType.Text, dbTransaction);

                if (re != null)
                {
                    opExel.DateCreated = re.DateCreated;
                    opExel.CreatedBy = re.CreatedBy;
                }

                var resp = await _repository.UpdateAsync(dbConnection, opExel, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Operational Excellence Initiative Added", $"Company Admin updated Operational Excellence Initiative with Id {opExel.Id}.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateExistingOEProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> CreateNewOEProjectMonthlySavings(OperationalExcellenceMonthlySaving opExel, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                opExel.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OperationalExcellenceMonthlySavingTable);
                var resp = await _repository.InsertAsync(dbConnection, opExel, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Operational Excellence Monthly Saving", $"User with Id {opExel.CreatedBy} added new Operational Excellence Monthly Saving.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewOEProjectMonthlySavings)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<OperationalExcellenceMonthlySavingDTO>> GetOEProjectMonthlySavings(long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<OperationalExcellenceMonthlySavingDTO>(dbConnection,
                "select a.Id, a.ProjectId, a.OrganizationId, a.MonthYear, a.Savings, a.Currency, a.DateCreated, a.CreatedBy, b.Name as CreatedByUser from OperationalExcellenceMonthlySaving a left join CIUser b on a.CreatedBy = b.Id where ProjectId = @pid order by a.DateCreated desc", new { pid = projectId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceMonthlySavingDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi,
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceMonthlySavingDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetOEProjectMonthlySavings)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OperationalExcellenceMonthlySavingDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OperationalExcellenceMonthlySavingDTO>> GetOEProjectMonthlySaving(long monthlySavingId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<OperationalExcellenceMonthlySavingDTO>(dbConnection,
                "select * from OperationalExcellenceMonthlySaving where Id = @msid", new { msid = monthlySavingId }, CommandType.Text);

                if (resi != null)
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceMonthlySavingDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi,
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceMonthlySavingDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(OperationalExcellenceMonthlySavingDTO)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OperationalExcellenceMonthlySavingDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> UpdateOEProjectMonthlySavings(OperationalExcellenceMonthlySaving opExel, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var re = await _repository.GetAsync<OperationalExcellenceMonthlySaving>(dbConnection,
                "select * from OperationalExcellenceMonthlySaving where Id = @msid", new { msid = opExel.Id }, CommandType.Text, dbTransaction);

                if(re != null)
                {
                    opExel.DateCreated = re.DateCreated;
                    opExel.CreatedBy = re.CreatedBy;
                }

                var resp = await _repository.UpdateAsync(dbConnection, opExel, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Operational Excellence Monthly Saving", $"User with Id {opExel.CreatedBy} updated j Operational Excellence Monthly Saving.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateOEProjectMonthlySavings)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> CreateNewSIProject(StrategicInitiative si, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                si.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.StrategicInitiativeTable);
                var resp = await _repository.InsertAsync(dbConnection, si, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Strategic Initiative Added", $"Company Rep added new Strategic Initiative.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewSIProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> CreateNewSISubProject(SISubProject si, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                si.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.SISubProjectTable);
                var resp = await _repository.InsertAsync(dbConnection, si, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("SISubProject Added", $"Company Rep added new SI Sub Project.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewSISubProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<StrategicInitiativeDTO>> GetAllInProgressOrganizationSI(int orgId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<StrategicInitiativeDTO>(dbConnection, "SELECT a.Id, a.Title, COALESCE(AVG(b.Percentage), 0) AS CumulativePercent FROM StrategicInitiative a LEFT JOIN SISubProject b ON b.SIId = a.Id WHERE a.OrganizationId = @oid GROUP BY a.Id, a.Title HAVING COALESCE(AVG(b.Percentage), 0) < 100", new {oid = orgId}, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllInProgressOrganizationSI)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<SISubProjectDTO>> GetSISubProjects(long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<SISubProjectDTO>(dbConnection,
                "select a.Id, a.Initiative, a.StartDate, a.EndDate, a.Description, a.FacilitatorId, b.Name as Facilitator, a.Percentage, a.Savings, a.Currency, a.DateCreated, a.CreatedBy, b1.Name as CreatedByUser from SISubProject a left join CIUser b on a.FacilitatorId = b.Id left join CIUser b1 on a.CreatedBy = b1.Id where a.SIId = @pid order by a.DateCreated desc", new { pid = projectId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<SISubProjectDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi,
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<SISubProjectDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetSISubProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<SISubProjectDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<StrategicInitiativeDTO>> GetPaginatedSIProjects(int orgId, int pageNumber, int pageSize, InitiativeFilter filt)
        {
            try
            {
                IEnumerable<StrategicInitiativeDTO> resi = null; int count = 0;
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var query = "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Description, a.OwnerId, b.Name as Owner, a.ExecutiveSponsorId, b1.Name as ExecutiveSponsor, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, e.Department as OrganizationDepartment, a.CreatedBy, b2.Name as CreatedByStaff, COALESCE(sp.CummulativeROI, 0) AS CummulativeROI, COALESCE(sp.PercentageProgress, 0) AS PercentageProgress, COALESCE(sp.Teams, '') AS Teams FROM StrategicInitiative a left join CIUser b on a.OwnerId = b.Id left join CIUser b1 on a.ExecutiveSponsorId = b1.Id left join CIUser b2 on a.CreatedBy = b2.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id LEFT JOIN (SELECT sp.SIId, SUM(sp.Savings) AS CummulativeROI, AVG(sp.Percentage) AS PercentageProgress, STRING_AGG(u.Name, ', ') AS Teams FROM SISubProject sp LEFT JOIN CIUser u ON sp.FacilitatorId = u.Id GROUP BY sp.SIId) sp ON a.Id = sp.SIId where a.OrganizationId = @oid and a.Status NOT IN ('CLOSED', 'CANCELLED') @where ORDER BY a.DateCreated DESC OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                var countquery = "SELECT count(id) from StrategicInitiative where OrganizationId = @oid and Status NOT IN ('CLOSED', 'CANCELLED') @where";

                if (filt == null || (filt.StartDate == new DateTime() && filt.EndDate == new DateTime() && String.IsNullOrEmpty(filt.Title) && filt.CountryId == 0 && filt.DepartmentId == 0 && String.IsNullOrEmpty(filt.Priority) && filt.UserId == 0))
                {
                    resi = await _repository.GetListAsync<StrategicInitiativeDTO>(dbConnection, query.Replace("@where", ""), new { oid = orgId, pageNumber, pageSize }, CommandType.Text);

                    count = await _repository.GetSumOrCountAsync<int>(dbConnection, countquery.Replace("@where", ""), new { oid = orgId }, CommandType.Text);
                }
                else
                {
                    var where = new StringBuilder();
                    var where1 = new StringBuilder();
                    var parameters = new DynamicParameters();

                    if (!string.IsNullOrWhiteSpace(filt.Title))
                    {
                        where.Append(" AND a.Title LIKE @Title");
                        where1.Append(" AND Title LIKE @Title");
                        parameters.Add("@Title", $"%{filt.Title.Trim()}%");
                    }

                    if (!string.IsNullOrWhiteSpace(filt.Priority))
                    {
                        where.Append(" AND a.Priority = @Priority");
                        where1.Append(" AND Priority = @Priority");
                        parameters.Add("@Priority", filt.Priority.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(filt.Status))
                    {
                        where.Append(" AND a.Status = @Stat");
                        where1.Append(" AND Status LIKE @Stat");
                        parameters.Add("@Stat", $"%{filt.Status.Trim()}%");
                    }

                    if (filt.UserId > 0)
                    {
                        where.Append(" AND (a.OwnerId = @UserId OR a.ExecutiveSponsorId = @UserId)");
                        where1.Append(" AND (OwnerId = @UserId OR ExecutiveSponsorId = @UserId)");
                        parameters.Add("@UserId", filt.UserId);
                    }

                    if (filt.CountryId > 0)
                    {
                        where.Append(" AND a.OrganizationCountryId = @CountryId");
                        where1.Append(" AND OrganizationCountryId = @CountryId");
                        parameters.Add("@CountryId", filt.CountryId);
                    }

                    if (filt.DepartmentId > 0)
                    {
                        where.Append(" AND a.OrganizationDepartmentId = @DepartmentId");
                        where1.Append(" AND OrganizationDepartmentId = @DepartmentId");
                        parameters.Add("@DepartmentId", filt.DepartmentId);
                    }

                    if (filt.StartDate != DateTime.MinValue)
                    {
                        where.Append(" AND a.StartDate >= @StartDate");
                        where1.Append(" AND StartDate >= @StartDate");
                        parameters.Add("@StartDate", filt.StartDate.Date);
                    }

                    if (filt.EndDate != DateTime.MinValue)
                    {
                        where.Append(" AND a.EndDate <= @EndDate");
                        where1.Append(" AND EndDate <= @EndDate");
                        parameters.Add("@EndDate", filt.EndDate.Date);
                    }

                    var finalQuery = query.Replace("@where", where.ToString());


                    parameters.Add("@oid", orgId);

                    var finalcountquery = countquery.Replace("@where", where1.ToString());

                    count = await _repository.GetSumOrCountAsync<int>(dbConnection, finalcountquery, parameters, CommandType.Text);

                    parameters.Add("@pageNumber", pageNumber);
                    parameters.Add("@pageSize", pageSize);

                    resi = await _repository.GetListAsync<StrategicInitiativeDTO>(dbConnection, finalQuery, parameters, CommandType.Text);
                }

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi,
                        TotalRecords = count,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetPaginatedSIProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<StrategicInitiativeDTO>> GetSIProject(int orgId, long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetAsync<StrategicInitiativeDTO>(dbConnection,
                "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Status, a.Description, a.OwnerId, b.Name as Owner, a.ExecutiveSponsorId, b1.Name as ExecutiveSponsor, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, e.Department as OrganizationDepartment, a.CreatedBy, b2.Name as CreatedByStaff, COALESCE(sp.CummulativeROI, 0) AS CummulativeROI, COALESCE(sp.PercentageProgress, 0) AS PercentageProgress, COALESCE(sp.Teams, '') AS Teams FROM StrategicInitiative a left join CIUser b on a.OwnerId = b.Id left join CIUser b1 on a.ExecutiveSponsorId = b1.Id left join CIUser b2 on a.CreatedBy = b2.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id LEFT JOIN (SELECT sp.SIId, SUM(sp.Savings) AS CummulativeROI, AVG(sp.Percentage) AS PercentageProgress, STRING_AGG(u.Name, ', ') AS Teams FROM SISubProject sp LEFT JOIN CIUser u ON sp.FacilitatorId = u.Id GROUP BY sp.SIId) sp ON a.Id = sp.SIId where a.OrganizationId = @oid and a.Id = @pid", new { oid = orgId, pid = projectId }, CommandType.Text);

                if (resi != null)
                {
                    return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetSIProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> UpdateExistingSIProject(StrategicInitiative si, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var re = await _repository.GetAsync<StrategicInitiative>(dbConnection,
                "SELECT * FROM StrategicInitiative where OrganizationId = @oid and Id = @pid", new { oid = si.OrganizationId, pid = si.Id }, CommandType.Text, dbTransaction);

                if (re != null)
                {
                    si.DateCreated = re.DateCreated;
                    si.CreatedBy = re.CreatedBy;
                }

                var resp = await _repository.UpdateAsync(dbConnection, si, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Strategic Initiative Updated", $"Company Admin updated Strategic Initiative with Id {si.Id}.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateExistingSIProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<SISubProjectDTO>> GetSISubProject(long Id)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<SISubProjectDTO>(dbConnection,
                "select a.Id, a.Initiative, a.StartDate, a.EndDate, a.Description, a.FacilitatorId, b.Name as Facilitator, a.Percentage, a.Savings, a.Currency, a.DateCreated, a.CreatedBy, b1.Name as CreatedByUser from SISubProject a left join CIUser b on a.FacilitatorId = b.Id left join CIUser b1 on a.CreatedBy = b1.Id where a.Id = @pid order by a.DateCreated desc", new { pid = Id }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<SISubProjectDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi,
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<SISubProjectDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetSISubProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<SISubProjectDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> UpdateExistingSISubProject(SISubProject si, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var re = await _repository.GetAsync<SISubProject>(dbConnection,
                "SELECT * FROM SISubProject where Id = @pid", new { pid = si.Id }, CommandType.Text, dbTransaction);

                if (re != null)
                {
                    si.DateCreated = re.DateCreated;
                    si.CreatedBy = re.CreatedBy;
                }

                var resp = await _repository.UpdateAsync(dbConnection, si, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("SISubProject Updated", $"Company Admin updated SISubProject with Id {si.Id}.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateExistingSISubProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<SupportingValueSearchResultDTO>> GetMiniOESIProject(int orgId, string type, long pid )
        {
            try
            {
                SupportingValueSearchResultDTO item = null;
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                if (type == "OE")
                {
                    item = await _repository.GetAsync<SupportingValueSearchResultDTO>(dbConnection, "SELECT Id, Title FROM OperationalExcellence WHERE Id = @id",
                        new { id = pid }, CommandType.Text);
                }

                if (type == "SI")
                {
                    item = await _repository.GetAsync<SupportingValueSearchResultDTO>(dbConnection, "SELECT Id, Title FROM StrategicInitiative WHERE Id = @id",
                        new { id = pid }, CommandType.Text);
                }

                if (item != null)
                {
                    return await Task.FromResult(new ResponseHandler<SupportingValueSearchResultDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = item
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<SupportingValueSearchResultDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetMiniOESIProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<SupportingValueSearchResultDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<SupportingValueSearchResultDTO>> GetMiniOESIProjects(int orgId, string search)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<SupportingValueSearchResultDTO>(dbConnection,
                "SELECT Id, Title, 'OE' AS Source FROM OperationalExcellence WHERE OrganizationId = @orgId AND Status NOT IN ('CLOSED', 'CANCELLED') AND Title LIKE '%'+@search+'%' UNION ALL SELECT Id, Title, 'SI' AS Source FROM StrategicInitiative WHERE OrganizationId = @orgId AND Status NOT IN ('CLOSED', 'CANCELLED') AND Title LIKE '%'+@search+'%' ORDER BY Title", new { orgId, search }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<SupportingValueSearchResultDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<SupportingValueSearchResultDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetMiniOESIProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<SupportingValueSearchResultDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OrganizationToolDTO>> GetAllOrganizationTools(int orgId, string method, long pid)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                method = !method.ToLower().Equals("project") ? "General" : "Project";

                var resi = await _repository.GetListAsync<OrganizationToolDTO>(dbConnection,
                "SELECT d.Id, a.Url, b.Id as ToolId, b.Tool, c.Phase, CASE WHEN d.ToolId IS NOT NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS IsChecked FROM MethodologyTool b INNER JOIN MethodologyPhase c ON c.Id = b.Phase LEFT JOIN OrganizationTool a ON a.MethodologyTool = b.Id AND a.OrganizationId = @oid LEFT JOIN CIProjectTool d ON d.ToolId = b.Id AND d.ProjectId = @pid WHERE c.Methodology = @mth", new { oid = orgId, mth = method, pid = pid }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationToolDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationToolDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOrganizationTools)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler<OrganizationToolDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler<MethodologyPhase>> GetMethodologyPhases(string method = null)
        {
            try
            {
                IEnumerable<MethodologyPhase> resi = null;
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                if (!String.IsNullOrEmpty(method))
                {
                    resi = await _repository.GetListAsync<MethodologyPhase>(dbConnection, "SELECT Id, Methodology, Phase FROM MethodologyPhase WHERE Methodology = @mth", new { mth = method }, CommandType.Text);
                }
                else
                {
                    resi = await _repository.GetListAsync<MethodologyPhase>(dbConnection, "SELECT Id, Methodology, Phase FROM MethodologyPhase", CommandType.Text);
                }


                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<MethodologyPhase>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<MethodologyPhase>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetMethodologyPhases)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler<MethodologyPhase>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler<OrganizationToolDTO>> GetAllMethodologyTools(string method)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<OrganizationToolDTO>(dbConnection,
                "SELECT a.Id, a.Url, b.Tool, c.Phase FROM MethodologyTool b INNER JOIN MethodologyPhase c ON c.Id = b.Phase LEFT JOIN OrganizationTool a ON a.MethodologyTool = b.Id WHERE c.Methodology = @mth", new { mth = method }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationToolDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationToolDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllMethodologyTools)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler<OrganizationToolDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler<OrganizationSoftSaving>> GetAllOrganizationSavingCategory(int orgId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetListAsync<OrganizationSoftSaving>(dbConnection,
                "Select * from OrganizationSoftSaving where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text);

                if (resi.Any())
                {

                    return await Task.FromResult(new ResponseHandler<OrganizationSoftSaving>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });

                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<OrganizationSoftSaving>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllOrganizationSavingCategory)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OrganizationSoftSaving>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler> AddOrganizationSoftSaving(OrganizationSoftSaving orgSs, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                orgSs.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationSoftSavingTable);
                var resp = await _repository.InsertAsync(dbConnection, orgSs, dbTransaction);

                var audit = ModelBuilder.BuildAuditLog("Soft Saving Added", $"Company Admin added new Organization Soft Saving.", adminEmail);
                audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationSoftSaving)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> RenameOrganizationSoftSaving(long ssId, OrganizationSoftSaving oss, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.GetAsync<OrganizationSoftSaving>(dbConnection,
                    "Select * from OrganizationSoftSaving where Id = @cid", new { cid = ssId }, CommandType.Text, dbTransaction);

                if (resi != null)
                {
                    resi.Category = oss.Category;
                    resi.Unit = oss.Unit;
                    var res = await _repository.UpdateAsync(dbConnection, resi, dbTransaction);


                    var audit = ModelBuilder.BuildAuditLog("Soft Saving Renamed", $"Company Admin renamed organization Soft Saving '{resi.Id}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record updated Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(RenameOrganizationSoftSaving)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> DeleteOrganizationSoftSaving(long ssId, string adminEmail, int orgId)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                var resi = await _repository.ExecuteAsync(dbConnection,
                    "Update OrganizationSoftSaving set IsActive = 0 where Id = @cid", new { cid = ssId }, CommandType.Text, dbTransaction);

                if (resi > 0)
                {
                    var audit = ModelBuilder.BuildAuditLog("Soft Saving Deleted", $"Company Admin deleted organization Soft Saving '{ssId}'.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    dbTransaction.Commit();

                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Record deleted Sucessfully"
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(DeleteOrganizationSoftSaving)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<ContinuousImprovement>> CreateNewCIProject(ContinuousImprovement ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                if(ci.Id == 0)
                {
                    ci.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.ContinuousImprovementTable);
                    var resp = await _repository.InsertAsync(dbConnection, ci, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Continuous Improvement Added", $"Company Rep added new Continuous Improvement Project.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }
                else
                {
                    var res = await _repository.UpdateAsync(dbConnection, ci, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Continuous Improvement Updated", $"Company Rep updated new Continuous Improvement Project.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler<ContinuousImprovement>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful",
                    SingleResult = ci
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewCIProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<ContinuousImprovement>
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

        public async Task<bool> CheckIfTenantHasSiteId(string tenantId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                var resi = await _repository.GetAsync<Organization>(dbConnection,
                "Select * from Organization where TenantId = @tid", new { tid = tenantId }, CommandType.Text);

                if (resi != null)
                {
                    return !String.IsNullOrEmpty(resi.SiteId);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(CheckIfTenantHasSiteId)} - {JsonConvert.SerializeObject(ex)}");
                return false;
            }
        }

        public async Task<ResponseHandler> CreateNewCIProjectTeam(List<CIProjectTeamMember> ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //get count of all team members on this project

                var count = await _repository.GetListAsync<CIProjectTeamMember>(dbConnection, "select * from CIProjectTeamMember WHERE ProjectId = @ProjectId", new { ci[0].ProjectId }, CommandType.Text, dbTransaction);

                count = count.ToList();

                if (count.Any())
                {
                    //if(count.Count() > ci.Count)
                    //{
                        //an item was deleted on the UI
                        // 2. Extract IDs
                        var incomingIds = ci.Select(x => x.Id).ToHashSet();

                        // 3. Find deleted records
                        var itemsToDelete = count.Where(dbItem => !incomingIds.Contains(dbItem.Id)).ToList();

                        foreach(var i in itemsToDelete)
                        {
                            if(i.Role != "Facilitator")
                                await _repository.ExecuteAsync(dbConnection, "Delete from CIProjectTeamMember where Id = @id", new { id = i.Id }, CommandType.Text, dbTransaction);
                        }
                    //}
                }


                foreach (var teamMember in ci)
                {
                    try
                    {
                        teamMember.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectTeamMemberTable);

                        await _repository.InsertAsync(dbConnection, teamMember, dbTransaction);

                        // Audit: only after successful insert
                        var audit = ModelBuilder.BuildAuditLog("Team Member Added", $"Added {teamMember.UserId} as {teamMember.Role} to project {teamMember.ProjectId}.", adminEmail);

                        audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                        await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        // Duplicate (ProjectId, UserId, Role)
                        // Decide business behavior: ignore or update

                        var existing = await dbConnection.QuerySingleAsync<CIProjectTeamMember>(
                            @"SELECT * FROM CIProjectTeamMember WHERE ProjectId = @ProjectId AND UserId = @UserId AND Role = @Role", new { teamMember.ProjectId, teamMember.UserId, teamMember.Role }, dbTransaction);

                        if (existing.SendNotification != teamMember.SendNotification || existing.UserId != teamMember.UserId || existing.Role != teamMember.Role)
                        {
                            existing.UserId = teamMember.UserId;
                            existing.Role = teamMember.Role;
                            existing.SendNotification = teamMember.SendNotification;

                            await _repository.UpdateAsync(dbConnection, existing, dbTransaction);

                            var audit = ModelBuilder.BuildAuditLog(
                                "Team Member Updated",
                                $"Updated {teamMember.UserId}'s notification preference.",
                                adminEmail
                            );

                            audit.Id = await _genManager.GetNextTableId(
                                dbConnection,
                                dbTransaction,
                                DatabaseScripts.AuditLogTable
                            );

                            await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                        }
                    }
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewCIProjectTeam)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> CreateNewCIProjectTool(List<CIProjectToolDTO> ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //get all phases
                var phases = await _repository.GetListAsync<MethodologyPhase>(dbConnection, "SELECT Id, Methodology, Phase FROM MethodologyPhase", CommandType.Text, dbTransaction);

                foreach (var i in ci)
                {
                    var ce = new CIProjectTool
                    {
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectToolTable),
                        ProjectId = i.ProjectId,
                        Methodology = i.Methodology,
                        PhaseId = phases.ToList().Where(t => t.Phase == i.Phase).FirstOrDefault().Id,
                        ToolId = i.ToolId,
                        CreatedBy = i.CreatedBy,
                        DateCreated = i.DateCreated
                    };

                    var resp = await _repository.InsertAsync(dbConnection, ce, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("CI Project Tool Added", $"Company Rep added new CI Project tool.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewCIProjectTool)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<ContinuousImprovementDTO>> GetPaginatedCIProjects(int orgId, int pageNumber, int pageSize, InitiativeFilter filt)
        {
            try
            {
                IEnumerable<ContinuousImprovementDTO> resi = null; int count = 0;
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var query = "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.ProblemStatement, a.Methodology, a.Certification, a.TotalExpectedRevenue, a.Currency, a.Status, a.CountryId, c.Country AS Country, a.FacilityId, d.Facility AS Facility, a.DepartmentId, e.Department AS Department, a.Phase AS PhaseId, f.Phase, tm.UserId AS FacilitatorId, u.Name AS FacilitatorName, a.CreatedBy, b1.Name AS CreatedByStaff FROM Continuousimprovement a LEFT JOIN CIProjectTeamMember tm ON tm.ProjectId = a.Id AND tm.Role = 'Facilitator' LEFT JOIN CIUser u ON u.Id = tm.UserId LEFT JOIN CIUser b1 ON a.CreatedBy = b1.Id LEFT JOIN OrganizationCountry c ON a.CountryId = c.Id LEFT JOIN OrganizationFacility d ON a.FacilityId = d.Id LEFT JOIN OrganizationDepartment e ON a.DepartmentId = e.Id LEFT JOIN MethodologyPhase f ON a.Phase = f.Id WHERE a.OrganizationId = @oid AND a.Status NOT IN ('CLOSED', 'CANCELLED') @where ORDER BY a.DateCreated DESC OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                var countquery = "SELECT count(id) from ContinuousImprovement where OrganizationId = @oid and Status NOT IN ('CLOSED', 'CANCELLED') @where";

                if (filt == null || (filt.StartDate == new DateTime() && filt.EndDate == new DateTime() && String.IsNullOrEmpty(filt.Title) && filt.CountryId == 0 && filt.DepartmentId == 0 && String.IsNullOrEmpty(filt.Priority) && filt.UserId == 0))
                {
                    resi = await _repository.GetListAsync<ContinuousImprovementDTO>(dbConnection, query.Replace("@where", ""), new { oid = orgId, pageNumber, pageSize }, CommandType.Text);

                    count = await _repository.GetSumOrCountAsync<int>(dbConnection, countquery.Replace("@where", ""), new { oid = orgId }, CommandType.Text);
                }
                else
                {
                    var where = new StringBuilder();
                    var where1 = new StringBuilder();
                    var parameters = new DynamicParameters();

                    if (!string.IsNullOrWhiteSpace(filt.Title))
                    {
                        where.Append(" AND a.Title LIKE @Title");
                        where1.Append(" AND Title LIKE @Title");
                        parameters.Add("@Title", $"%{filt.Title.Trim()}%");
                    }

                    if (!string.IsNullOrWhiteSpace(filt.Status))
                    {
                        where.Append(" AND a.Status = @Stat");
                        where1.Append(" AND Status LIKE @Stat");
                        parameters.Add("@Stat", $"%{filt.Status.Trim()}%");
                    }

                    if (!string.IsNullOrWhiteSpace(filt.Priority))
                    {
                        where.Append(" AND a.Priority = @Priority");
                        where1.Append(" AND Priority = @Priority");
                        parameters.Add("@Priority", filt.Priority.Trim());
                    }

                    //if (filt.UserId > 0)
                    //{
                    //    where.Append(" AND (a.FacilitatorId = @UserId OR a.SponsorId = @UserId OR a.ExecutiveSponsorId = @UserId)");
                    //    where1.Append(" AND (FacilitatorId = @UserId OR SponsorId = @UserId OR ExecutiveSponsorId = @UserId)");
                    //    parameters.Add("@UserId", filt.UserId);
                    //}

                    if (filt.CountryId > 0)
                    {
                        where.Append(" AND a.OrganizationCountryId = @CountryId");
                        where1.Append(" AND OrganizationCountryId = @CountryId");
                        parameters.Add("@CountryId", filt.CountryId);
                    }

                    if (filt.DepartmentId > 0)
                    {
                        where.Append(" AND a.OrganizationDepartmentId = @DepartmentId");
                        where1.Append(" AND OrganizationDepartmentId = @DepartmentId");
                        parameters.Add("@DepartmentId", filt.DepartmentId);
                    }

                    if (filt.StartDate != DateTime.MinValue)
                    {
                        where.Append(" AND a.StartDate >= @StartDate");
                        where1.Append(" AND StartDate >= @StartDate");
                        parameters.Add("@StartDate", filt.StartDate.Date);
                    }

                    if (filt.EndDate != DateTime.MinValue)
                    {
                        where.Append(" AND a.EndDate <= @EndDate");
                        where1.Append(" AND EndDate <= @EndDate");
                        parameters.Add("@EndDate", filt.EndDate.Date);
                    }

                    var finalQuery = query.Replace("@where", where.ToString());


                    parameters.Add("@oid", orgId);

                    var finalcountquery = countquery.Replace("@where", where1.ToString());

                    count = await _repository.GetSumOrCountAsync<int>(dbConnection, finalcountquery, parameters, CommandType.Text);

                    parameters.Add("@pageNumber", pageNumber);
                    parameters.Add("@pageSize", pageSize);

                    resi = await _repository.GetListAsync<ContinuousImprovementDTO>(dbConnection, finalQuery, parameters, CommandType.Text);
                }

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<ContinuousImprovementDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi,
                        TotalRecords = count,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalPages = (int)Math.Ceiling(count / (double)pageSize)
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<ContinuousImprovementDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetPaginatedCIProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<ContinuousImprovementDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<CIProjectToolDTO>> GetAllProjectSelectedTools(long pid)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<CIProjectToolDTO>(dbConnection,
                "select a.Id, a.ProjectId, a.Methodology, a.PhaseId, b.Phase, a.ToolId, c.Tool, a.Url from CIProjectTool a left join MethodologyPhase b on b.Id = a.PhaseId left join MethodologyTool c on c.Id = a.ToolId where a.ProjectId = @pid", new { pid = pid }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<CIProjectToolDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CIProjectToolDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetAllProjectSelectedTools)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler<CIProjectToolDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler> UpdateToolId(int toolId, string fileUrl)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetAsync<CIProjectTool>(dbConnection,
                "select * from CIProjectTool where Id = @pid", new { pid = toolId }, CommandType.Text);

                if (resi != null)
                {
                    resi.Url = fileUrl;

                    var res = await _repository.UpdateAsync(dbConnection, resi);

                    if (res)
                    {
                        return await Task.FromResult(new ResponseHandler<CIProjectToolDTO>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful"
                        });
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<CIProjectToolDTO>
                        {
                            StatusCode = (int)HttpStatusCode.ExpectationFailed,
                            Message = "Tool Url update was unsucessful"
                        });
                    }  
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateToolId)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler<CIProjectToolDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler> CreateNewCIProjectComment(List<CIProjectComment> ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (var comm in ci)
                {
                    comm.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectCommentTable);

                    await _repository.InsertAsync(dbConnection, comm, dbTransaction);

                    // Audit: only after successful insert
                    var audit = ModelBuilder.BuildAuditLog("Comment Added", $"Added a comment to project {comm.ProjectId}.", adminEmail);

                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                    await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewCIProjectComment)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> CreateNewCIProjectSaving(List<CIProjectSaving> si, ContinuousImprovementDTO ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (var comm in si)
                {
                    comm.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectSavingTable);

                    await _repository.InsertAsync(dbConnection, comm, dbTransaction);

                    // Audit: only after successful insert
                    var audit = ModelBuilder.BuildAuditLog("Saving Added", $"Added a saving to project {comm.ProjectId}.", adminEmail);

                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                    await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                //fetch project
                var proj = await _repository.GetAsync<ContinuousImprovement>(dbConnection, "select * from ContinuousImprovement where Id = @pid", new { pid = ci.Id }, CommandType.Text, dbTransaction);

                if(proj != null)
                {
                    proj.IsOneTimeSavings = ci.IsOneTimeSavings;
                    proj.IsCarryOverSavings = ci.IsCarryOverSavings;
                    proj.FinancialVerificationDate = ci.FinancialVerificationDate;

                    await _repository.UpdateAsync(dbConnection, proj, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("CI Financial Info Updated", $"Updated the financial info of a CI Project {ci.Id}.", adminEmail);

                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                    await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(CreateNewCIProjectSaving)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler<ContinuousImprovementDTO>> GetCIProject(int orgId, long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                
                var resi = await _repository.GetAsync<ContinuousImprovementDTO>(dbConnection,
                "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.BusinessObjectiveAlignment, a.ProblemStatement, a.Methodology, a.Certification, a.TotalExpectedRevenue, a.Currency, a.Status, a.CountryId, c.Country AS Country, a.FacilityId, d.Facility AS Facility, a.DepartmentId, e.Department AS Department, a.Phase AS PhaseId, f.Phase, tm.UserId AS FacilitatorId, u.Name AS FacilitatorName, a.CreatedBy, b1.Name AS CreatedByStaff, a.SupportingValueStream, a.IsOneTimeSavings, a.IsCarryOverSavings, a.FinancialVerificationDate, a.FinancialReportUrl, a.FinancialReportComment, a.IsAudited, a.AuditedBy, b2.Name as AuditedByStaff, a.AuditedDate FROM Continuousimprovement a LEFT JOIN CIProjectTeamMember tm ON tm.ProjectId = a.Id AND tm.Role = 'Facilitator' LEFT JOIN CIUser u ON u.Id = tm.UserId LEFT JOIN CIUser b1 ON a.CreatedBy = b1.Id LEFT JOIN CIUser b2 ON a.AuditedBy = b2.Id LEFT JOIN OrganizationCountry c ON a.CountryId = c.Id LEFT JOIN OrganizationFacility d ON a.FacilityId = d.Id LEFT JOIN OrganizationDepartment e ON a.DepartmentId = e.Id LEFT JOIN MethodologyPhase f ON a.Phase = f.Id WHERE a.OrganizationId = @oid and a.Id = @pid", new { oid = orgId, pid = projectId }, CommandType.Text);

                if (resi != null)
                {
                    return await Task.FromResult(new ResponseHandler<ContinuousImprovementDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = resi
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<ContinuousImprovementDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetCIProject)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<ContinuousImprovementDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<CITeamDTO>> GetCIProjectTeam(long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<TeamMembersDTO>(dbConnection,
                "select a.Id, a.ProjectId, a.UserId, b.Name as [User], a.Role, a.SendNotification from CIProjectTeamMember a left join CIUser b on b.Id = a.UserId where a.ProjectId = @pid", new { pid = projectId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<CITeamDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = new CITeamDTO
                        {
                            ProjectId = projectId,
                            Team = resi.ToList()
                        }
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CITeamDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found",
                        SingleResult = new CITeamDTO
                        {
                            ProjectId = projectId
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetCIProjectTeam)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<CITeamDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured",
                    SingleResult = new CITeamDTO
                    {
                        ProjectId = projectId
                    }
                });
            }
        }

        public async Task<List<CIProjectToolDTO>> GetCIProjectTool(long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<CIProjectToolDTO>(dbConnection,
                "select a.Id, a.ProjectId, a.Methodology, a.PhaseId, b.Phase, a.ToolId, c.Tool, a.Url from CIProjectTool a left join MethodologyPhase b on b.Id = a.PhaseId left join MethodologyTool c on c.Id = a.ToolId where ProjectId = @pid", new { pid = projectId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(resi.ToList());
                }
                else
                {
                    return await Task.FromResult(new List<CIProjectToolDTO>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetCIProjectTool)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new List<CIProjectToolDTO>());
            }
        }

        public async Task<ResponseHandler<CICommentDTO>> GetCIProjectComment(long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<CommentzDTO>(dbConnection,
                "select Id, Comment, Date from CIProjectComment where ProjectId = @pid", new { pid = projectId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<CICommentDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = new CICommentDTO
                        {
                            ProjectId = projectId,
                            Comment = resi.ToList()
                        }
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CICommentDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found",
                        SingleResult = new CICommentDTO
                        {
                            ProjectId = projectId,
                            Comment = new List<CommentzDTO>()
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetCIProjectComment)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<CICommentDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    SingleResult = new CICommentDTO
                    {
                        ProjectId = projectId,
                        Comment = new List<CommentzDTO>()
                    }
                });
            }
        }

        public async Task<ResponseHandler<CIFinancialDTO>> GetCIProjectFinancial(long projectId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<SavingsDTO>(dbConnection,
                "select Id, Category, SavingClassification, SavingType, SavingValue, SavingUnit, IsCurrency, [Date] from CIProjectSaving where ProjectId = @pid", new { pid = projectId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<CIFinancialDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        SingleResult = new CIFinancialDTO
                        {
                            ProjectId = projectId,
                            Hard = resi.Where(e => e.SavingClassification == "Hard").Select(h => new HardSavingsDTO
                            {
                                Id = h.Id,
                                SavingType = h.SavingType,
                                SavingValue = h.SavingValue,
                                Date = h.Date                                
                            }).ToList(),
                            Soft = resi.Where(e => e.SavingClassification == "Soft").Select(s => new SoftSavingsDTO
                            {
                                Id = s.Id,
                                SavingValue = s.SavingValue,
                                Category = s.Category,
                                SavingUnit = s.SavingUnit
                            }).ToList()
                        }
                    });
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler<CIFinancialDTO>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(GetCIProjectFinancial)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<CIFinancialDTO>());
            }
        }

        public async Task<ResponseHandler> UpdateCIProjectTool(List<CIProjectToolDTO> ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //get all project tools
                var count = await _repository.GetListAsync<CIProjectTool>(dbConnection, "select * from CIProjectTool WHERE ProjectId = @ProjectId", new { ci[0].ProjectId }, CommandType.Text, dbTransaction);

                count = count.ToList();

                if (count.Any())
                {
                    //an item was deleted on the UI
                    // 2. Extract IDs
                    var incomingIds = ci.Select(x => x.Id).ToHashSet();

                    // 3. Find deleted records
                    var itemsToDelete = count.Where(dbItem => !incomingIds.Contains(dbItem.Id)).ToList();

                    foreach (var i in itemsToDelete)
                    {
                        await _repository.ExecuteAsync(dbConnection, "Delete from CIProjectTool where Id = @id and Url Is NULL", new { id = i.Id }, CommandType.Text, dbTransaction);
                    }
                }

                //get all phases
                var phases = await _repository.GetListAsync<MethodologyPhase>(dbConnection, "SELECT Id, Methodology, Phase FROM MethodologyPhase", CommandType.Text, dbTransaction);

                foreach (var i in ci)
                {
                    try
                    {
                        i.PhaseId = phases.ToList().Where(t => t.Phase == i.Phase).FirstOrDefault().Id;

                        var ce = new CIProjectTool
                        {
                            Id = i.Id == 0 ? await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectToolTable) : i.Id,
                            ProjectId = i.ProjectId,
                            Methodology = i.Methodology,
                            PhaseId = i.PhaseId,
                            ToolId = i.ToolId,
                            CreatedBy = i.CreatedBy,
                            DateCreated = i.DateCreated
                        };

                        var resp = await _repository.InsertAsync(dbConnection, ce, dbTransaction);

                        var audit = ModelBuilder.BuildAuditLog("CI Project Tool Added", $"Company Rep added new CI Project tool.", adminEmail);
                        audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                        await _repository.InsertAsync(dbConnection, audit, dbTransaction);

                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        // Duplicate (Methodology, PhaseId, ToolId)
                        // Decide business behavior: ignore or update

                        var j = await _repository.GetAsync<CIProjectTool>(dbConnection, @"SELECT * FROM CIProjectTool WHERE Id = @id", new { id = i.Id }, CommandType.Text, dbTransaction);


                        if (j.Methodology != i.Methodology || j.PhaseId != i.PhaseId || j.ToolId != i.ToolId)
                        {
                            j.Methodology = i.Methodology;
                            j.PhaseId = i.PhaseId;
                            j.ToolId = i.ToolId;

                            await _repository.UpdateAsync(dbConnection, j, dbTransaction);

                            var audit = ModelBuilder.BuildAuditLog(
                                "Project Tool Updated",
                                $"Updated {i.Id}'s details.",
                                adminEmail
                            );

                            audit.Id = await _genManager.GetNextTableId(
                                dbConnection,
                                dbTransaction,
                                DatabaseScripts.AuditLogTable
                            );

                            await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                        }
                    }
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateCIProjectTool)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> UpdateCIProjectComment(List<CIProjectComment> ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //get all project comments
                var count = await _repository.GetListAsync<CIProjectTool>(dbConnection, "select * from CIProjectComment WHERE ProjectId = @ProjectId", new { ci[0].ProjectId }, CommandType.Text, dbTransaction);

                count = count.ToList();

                if (count.Any())
                {
                    //an item was deleted on the UI
                    // 2. Extract IDs
                    var incomingIds = ci.Select(x => x.Id).ToHashSet();

                    // 3. Find deleted records
                    var itemsToDelete = count.Where(dbItem => !incomingIds.Contains(dbItem.Id)).ToList();

                    foreach (var i in itemsToDelete)
                    {
                        await _repository.ExecuteAsync(dbConnection, "Delete from CIProjectComment where Id = @id", new { id = i.Id }, CommandType.Text, dbTransaction);
                    }
                }


                foreach (var comm in ci)
                {
                    try
                    {
                        comm.Id = comm.Id == 0 ? await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectCommentTable) : comm.Id;

                        await _repository.InsertAsync(dbConnection, comm, dbTransaction);

                        // Audit: only after successful insert
                        var audit = ModelBuilder.BuildAuditLog("Comment Added", $"Added a comment to project {comm.ProjectId}.", adminEmail);

                        audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                        await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        var j = await _repository.GetAsync<CIProjectComment>(dbConnection, @"SELECT * FROM CIProjectComment WHERE Id = @id", new { id = comm.Id }, CommandType.Text, dbTransaction);


                        if (j.Comment != comm.Comment || j.Date != comm.Date)
                        {
                            j.Comment = comm.Comment;
                            j.Date = comm.Date;

                            await _repository.UpdateAsync(dbConnection, j, dbTransaction);

                            var audit = ModelBuilder.BuildAuditLog(
                                "Project Comment Updated",
                                $"Updated {comm.Id}'s details.",
                                adminEmail
                            );

                            audit.Id = await _genManager.GetNextTableId(
                                dbConnection,
                                dbTransaction,
                                DatabaseScripts.AuditLogTable
                            );

                            await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                        }
                    }
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateCIProjectComment)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> UpdateReportFile(int projectId, string fileUrl)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetAsync<ContinuousImprovement>(dbConnection,
                "select * from ContinuousImprovement where Id = @pid", new { pid = projectId }, CommandType.Text);

                if (resi != null)
                {
                    resi.FinalReportUrl = fileUrl;

                    var res = await _repository.UpdateAsync(dbConnection, resi);

                    if (res)
                    {
                        return await Task.FromResult(new ResponseHandler
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful"
                        });
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler
                        {
                            StatusCode = (int)HttpStatusCode.ExpectationFailed,
                            Message = "Tool Url update was unsucessful"
                        });
                    }
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateReportFile)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler> UpdateFinancialReportFile(int projectId, string fileUrl)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetAsync<ContinuousImprovement>(dbConnection,
                "select * from ContinuousImprovement where Id = @pid", new { pid = projectId }, CommandType.Text);

                if (resi != null)
                {
                    resi.FinancialReportUrl = fileUrl;

                    var res = await _repository.UpdateAsync(dbConnection, resi);

                    if (res)
                    {
                        return await Task.FromResult(new ResponseHandler
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful"
                        });
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler
                        {
                            StatusCode = (int)HttpStatusCode.ExpectationFailed,
                            Message = "Tool Url update was unsucessful"
                        });
                    }
                }
                else
                {
                    return await Task.FromResult(new ResponseHandler
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = "Record not found"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception at {nameof(UpdateFinancialReportFile)} - {JsonConvert.SerializeObject(ex)}");
                return new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                };
            }
        }

        public async Task<ResponseHandler> UpdateCIProjectSaving(List<CIProjectSaving> si, ContinuousImprovementDTO ci, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //get all project savings
                var count = await _repository.GetListAsync<CIProjectSaving>(dbConnection, "select * from CIProjectSaving WHERE ProjectId = @ProjectId", new { ci.Id }, CommandType.Text, dbTransaction);

                count = count.ToList();

                if (count.Any())
                {
                    //an item was deleted on the UI
                    // 2. Extract IDs
                    var incomingIds = si.Select(x => x.Id).ToHashSet();

                    // 3. Find deleted records
                    var itemsToDelete = count.Where(dbItem => !incomingIds.Contains(dbItem.Id)).ToList();

                    foreach (var i in itemsToDelete)
                    {
                        await _repository.ExecuteAsync(dbConnection, "Delete from CIProjectSaving where Id = @id", new { id = i.Id }, CommandType.Text, dbTransaction);
                    }
                }

                foreach (var comm in si)
                {
                    try{
                        comm.Id = comm.Id == 0 ? await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIProjectSavingTable) : comm.Id;

                        await _repository.InsertAsync(dbConnection, comm, dbTransaction);

                        // Audit: only after successful insert
                        var audit = ModelBuilder.BuildAuditLog("Saving Added", $"Added a saving to project {comm.ProjectId}.", adminEmail);

                        audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                        await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                    }
                    catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                    {
                        var j = await _repository.GetAsync<CIProjectSaving>(dbConnection, @"SELECT * FROM CIProjectSaving WHERE Id = @id", new { id = comm.Id }, CommandType.Text, dbTransaction);


                        if (j.SavingValue != comm.SavingValue || j.Category != comm.Category || j.Date != comm.Date || j.MonthofYear != comm.MonthofYear || j.SavingClassification != comm.SavingClassification || j.SavingType != comm.SavingType || j.SavingUnit != comm.SavingUnit)
                        {
                            j.SavingUnit = comm.SavingUnit;
                            j.Date = comm.Date;
                            j.Category = comm.Category;
                            j.SavingClassification = comm.SavingClassification;
                            j.MonthofYear = comm.MonthofYear;
                            j.SavingType = comm.SavingType;
                            j.SavingValue = comm.SavingValue;

                            await _repository.UpdateAsync(dbConnection, j, dbTransaction);

                            var audit = ModelBuilder.BuildAuditLog(
                                "Project Saving Updated",
                                $"Updated {comm.Id}'s details.",
                                adminEmail
                            );

                            audit.Id = await _genManager.GetNextTableId(
                                dbConnection,
                                dbTransaction,
                                DatabaseScripts.AuditLogTable
                            );

                            await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                        }
                    }
                }

                //fetch project
                var proj = await _repository.GetAsync<ContinuousImprovement>(dbConnection, "select * from ContinuousImprovement where Id = @pid", new { pid = ci.Id }, CommandType.Text, dbTransaction);

                if (proj != null)
                {
                    proj.IsOneTimeSavings = ci.IsOneTimeSavings;
                    proj.IsCarryOverSavings = ci.IsCarryOverSavings;
                    proj.FinancialVerificationDate = ci.FinancialVerificationDate;
                    proj.IsAudited = ci.IsAudited;
                    proj.AuditedBy = ci.AuditedBy;
                    proj.AuditedDate = ci.AuditedDate;
                    proj.FinancialReportComment = ci.FinancialReportComment;

                    await _repository.UpdateAsync(dbConnection, proj, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("CI Financial Info Updated", $"Updated the financial info of a CI Project {ci.Id}.", adminEmail);

                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);

                    await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateCIProjectSaving)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddOrganizationUsers(List<CIUser> orgUsr, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach(var i in orgUsr)
                {
                    var user = await _repository.GetAsync<CIUser>(dbConnection, "Select 1 from CIUser where EmailAddress = @em and OrganizationId = @orgId", new { em = i.EmailAddress, orgId = i.OrganizationId }, CommandType.Text, dbTransaction);

                    if (user != null)
                        continue;

                    i.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.CIUserTable);
                    var resp = await _repository.InsertAsync(dbConnection, i, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("User Added", $"Company Admin added new Organization User.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                //UpdateUserListInMemory(dbConnection, dbTransaction, orgUsr.OrganizationId);

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationUsers)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddOrganizationLocations(List<BulkLocation> orgLocs, int orgId, long uid, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (var i in orgLocs)
                {
                    var cty = new OrganizationCountry
                    {
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationCountryTable),
                        Country = i.Country,
                        CreatedBy = uid,
                        DateCreated = DateTime.UtcNow,
                        IsActive = true,
                        OrganizationId = orgId
                    };

                    var countryId = await GetOrCreateCountry(dbConnection, dbTransaction, cty, adminEmail);

                    // 5. Facility (tied to Country)
                    var fac = new OrganizationFacility
                    {
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationFacilityTable),
                        OrganizationCountryId = countryId,
                        CreatedBy = uid,
                        DateCreated = DateTime.UtcNow,
                        IsActive = true,
                        OrganizationId = orgId,
                        Facility = i.Facility
                    };
                    var facilityId = await GetOrCreateFacility(dbConnection, dbTransaction, fac, adminEmail);

                    // 6. Department (tied to Facility + Country)
                    var dept = new OrganizationDepartment
                    {
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationDepartmentTable),
                        OrganizationCountryId = countryId,
                        CreatedBy = uid,
                        DateCreated = DateTime.UtcNow,
                        IsActive = true,
                        OrganizationId = orgId,
                        Department = i.Department,
                        OrganizationFacilityId = facilityId
                    };
                    await GetOrCreateDepartment(dbConnection, dbTransaction, dept, adminEmail);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddOrganizationLocations)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        private async Task<long> GetOrCreateCountry(IDbConnection conn, IDbTransaction tran, OrganizationCountry cty, string email)
        {
            try
            {
                var ctry = await _repository.GetAsync<OrganizationCountry>(conn, "Select * from OrganizationCountry where OrganizationId = @oid and Country = @cty and IsActive = 1", new { oid = cty.OrganizationId, cty = cty.Country }, CommandType.Text, tran);

                if (ctry != null)
                    return ctry.Id;

                var resp = await _repository.InsertAsync(conn, cty, tran);

                var audit = ModelBuilder.BuildAuditLog("Country Added", $"Company Admin added new Organization Country of operation.", email);
                audit.Id = await _genManager.GetNextTableId(conn, tran, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(conn, audit, tran);

                return cty.Id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        private async Task<long> GetOrCreateFacility(IDbConnection conn, IDbTransaction tran, OrganizationFacility fac, string email)
        {
            try
            {
                var facil = await _repository.GetAsync<OrganizationFacility>(conn, "Select * from OrganizationFacility where OrganizationId = @oid and OrganizationCountryId = @ocid and Facility = @fac and IsActive = 1", new { oid = fac.OrganizationId, ocid = fac.OrganizationCountryId, fac = fac.Facility }, CommandType.Text, tran);

                if (facil != null)
                    return facil.Id;

                var resp = await _repository.InsertAsync(conn, fac, tran);

                var audit = ModelBuilder.BuildAuditLog("Facility Added", $"Company Admin added new Organization Facility of operation.", email);
                audit.Id = await _genManager.GetNextTableId(conn, tran, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(conn, audit, tran);

                return fac.Id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        private async Task<long> GetOrCreateDepartment(IDbConnection conn, IDbTransaction tran, OrganizationDepartment dept, string email)
        {
            try
            {
                var depart = await _repository.GetAsync<OrganizationDepartment>(conn, "Select * from OrganizationDepartment where OrganizationId = @oid and OrganizationCountryId = @ocid and OrganizationFacilityId = @orgfacId and Department = @dept and IsActive = 1", new { oid = dept.OrganizationId, ocid = dept.OrganizationCountryId, orgfacId = dept.OrganizationFacilityId, dept = dept.Department }, CommandType.Text, tran);

                if (depart != null)
                    return depart.Id;

                var resp = await _repository.InsertAsync(conn, dept, tran);

                var audit = ModelBuilder.BuildAuditLog("Department Added", $"Company Admin added new Organization Department of operation.", email);
                audit.Id = await _genManager.GetNextTableId(conn, tran, DatabaseScripts.AuditLogTable);
                var auditRes = await _repository.InsertAsync(conn, audit, tran);

                return dept.Id;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        private async Task<CIUser> GetUser(IDbConnection conn, IDbTransaction tran, long orgId, string email)
        {
            try
            {
                return await _repository.GetAsync<CIUser>(conn, "select * from CIUser where EmailAddress = @em and OrganizationId = @orgId", new { em = email, orgId }, CommandType.Text, tran);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ResponseHandler> AddBulkOEProjects(List<BulkOE> opExel, int orgId, long uId, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach(var i in opExel)
                {
                    //check if project exist
                    var opProj = await _repository.GetAsync<OperationalExcellence>(dbConnection, "select 1 from OperationalExcellence where Title = @tit and OrganizationId = @orgId and Priority = @pro and Status = @stat", new { tit = i.Title, orgId = orgId, pro = i.Priority, stat = i.Status }, CommandType.Text, dbTransaction);

                    if (opProj != null)
                        continue;

                    var esid = await GetUser(dbConnection, dbTransaction, orgId, i.ExecutiveSponsorEmailAddress);

                    var fid = await GetUser(dbConnection, dbTransaction, orgId, i.FacilitatorEmailAddress);

                    var sid = await GetUser(dbConnection, dbTransaction, orgId, i.SponsorEmailAddress);

                    var ctr = await _repository.GetAsync<OrganizationCountry>(dbConnection, "Select * from OrganizationCountry where OrganizationId = @oid and Country = @cty and IsActive = 1", new { oid = orgId, cty = i.Country }, CommandType.Text, dbTransaction);

                    var facil = await _repository.GetAsync<OrganizationFacility>(dbConnection, "Select * from OrganizationFacility where OrganizationId = @oid and OrganizationCountryId = @ocid and Facility = @fac and IsActive = 1", new { oid = orgId, ocid = ctr.Id, fac = i.Facility }, CommandType.Text, dbTransaction);

                    var depart = await _repository.GetAsync<OrganizationDepartment>(dbConnection, "Select * from OrganizationDepartment where OrganizationId = @oid and OrganizationCountryId = @ocid and OrganizationFacilityId = @orgfacId and Department = @dept and IsActive = 1", new { oid = orgId, ocid = ctr.Id, orgfacId = facil.Id, dept = i.Department }, CommandType.Text, dbTransaction);

                    var op = new OperationalExcellence
                    {
                        CarryOverProject = i.IsCarryOverProject,
                        CreatedBy = uId,
                        Currency = Utils.GetSymbol(i.Currency),
                        DateCreated = DateTime.UtcNow,
                        Description = i.Description,
                        EndDate = Convert.ToDateTime(i.EndDate),
                        ExecutiveSponsorId = esid != null ? esid.Id : uId,
                        FacilitatorId = fid != null ? fid.Id : uId,
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OperationalExcellenceTable),
                        OrganizationCountryId = ctr.Id,
                        OrganizationDepartmentId = depart.Id,
                        OrganizationFacilityId = facil.Id,
                        OrganizationId = orgId,
                        Priority = i.Priority,
                        SavingsClassification = i.SavingsClassification,
                        SponsorId = sid != null ? sid.Id : uId,
                        StartDate = Convert.ToDateTime(i.StartDate),
                        Status = i.Status,
                        TargetSavings = i.TargetSavings,
                        Title = i.Title
                    };
                    var resp = await _repository.InsertAsync(dbConnection, op, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Operational Excellence Initiative Added", $"Company Admin added new Operational Excellence Initiative.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddBulkOEProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddBulkSIProjects(List<BulkSI> sInit, int orgId, long uId, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (var i in sInit)
                {
                    //check if project exist
                    var opProj = await _repository.GetAsync<StrategicInitiative>(dbConnection, "select 1 from StrategicInitiative where Title = @tit and OrganizationId = @orgId and Priority = @pro and Status = @stat", new { tit = i.Title, orgId = orgId, pro = i.Priority, stat = i.Status }, CommandType.Text, dbTransaction);

                    if (opProj != null)
                        continue;

                    var esid = await GetUser(dbConnection, dbTransaction, orgId, i.ExecutiveSponsorEmailAddress);

                    var fid = await GetUser(dbConnection, dbTransaction, orgId, i.OwnerEmailAddress);

                    var ctr = await _repository.GetAsync<OrganizationCountry>(dbConnection, "Select * from OrganizationCountry where OrganizationId = @oid and Country = @cty and IsActive = 1", new { oid = orgId, cty = i.Country }, CommandType.Text, dbTransaction);

                    var facil = await _repository.GetAsync<OrganizationFacility>(dbConnection, "Select * from OrganizationFacility where OrganizationId = @oid and OrganizationCountryId = @ocid and Facility = @fac and IsActive = 1", new { oid = orgId, ocid = ctr.Id, fac = i.Facility }, CommandType.Text, dbTransaction);

                    var depart = await _repository.GetAsync<OrganizationDepartment>(dbConnection, "Select * from OrganizationDepartment where OrganizationId = @oid and OrganizationCountryId = @ocid and OrganizationFacilityId = @orgfacId and Department = @dept and IsActive = 1", new { oid = orgId, ocid = ctr.Id, orgfacId = facil.Id, dept = i.Department }, CommandType.Text, dbTransaction);

                    var si = new StrategicInitiative
                    {
                        CreatedBy = uId,
                        DateCreated = DateTime.UtcNow,
                        Description = i.Description,
                        EndDate = Convert.ToDateTime(i.EndDate),
                        ExecutiveSponsorId = esid != null ? esid.Id : uId,
                        OwnerId = fid != null ? fid.Id : uId,
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.StrategicInitiativeTable),
                        OrganizationCountryId = ctr.Id,
                        OrganizationDepartmentId = depart.Id,
                        OrganizationFacilityId = facil.Id,
                        OrganizationId = orgId,
                        Priority = i.Priority,
                        StartDate = Convert.ToDateTime(i.StartDate),
                        Status = i.Status,
                        Title = i.Title
                    };
                    var resp = await _repository.InsertAsync(dbConnection, si, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Strategic Initiative Added", $"Company Admin added new Strategic Initiative.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddBulkSIProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> UpdateOrganizationTool(OrganizationTool orgTool, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                //check if tool exist
                var OrgT = await _repository.GetAsync<OrganizationTool>(dbConnection, "select * from OrganizationTool where MethodologyTool = @meth and OrganizationId = @orgId", new { meth = orgTool.MethodologyTool, orgId = orgTool.OrganizationId }, CommandType.Text, dbTransaction);

                if (OrgT != null)
                {
                    OrgT.Url = orgTool.Url;
                    await _repository.UpdateAsync(dbConnection, OrgT, dbTransaction);
                    var audit = ModelBuilder.BuildAuditLog("Methodology Tool template Updated", $"Company Admin updated existing methodology tool template.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }
                else
                {
                    orgTool.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.OrganizationToolTable);
                    var resp = await _repository.InsertAsync(dbConnection, orgTool, dbTransaction);
                    var audit = ModelBuilder.BuildAuditLog("Methodology Tool template Added", $"Company Admin added new methodology tool template.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }
                    
                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(UpdateOrganizationTool)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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

        public async Task<ResponseHandler> AddBulkCIProjects(List<BulkCI> ci, int orgId, long uId, string adminEmail)
        {
            using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
            dbConnection.Open();
            using var dbTransaction = dbConnection.BeginTransaction();
            try
            {
                foreach (var i in ci)
                {
                    //check if project exist
                    var ciProj = await _repository.GetAsync<ContinuousImprovement>(dbConnection, "select 1 from ContinuousImprovement where Title = @tit and OrganizationId = @orgId and Priority = @pro and Status = @stat", new { tit = i.Title, orgId, pro = i.Priority, stat = i.Status }, CommandType.Text, dbTransaction);

                    if (ciProj != null)
                        continue;

                    var ctr = await _repository.GetAsync<OrganizationCountry>(dbConnection, "Select * from OrganizationCountry where OrganizationId = @oid and Country = @cty and IsActive = 1", new { oid = orgId, cty = i.Country }, CommandType.Text, dbTransaction);

                    var facil = await _repository.GetAsync<OrganizationFacility>(dbConnection, "Select * from OrganizationFacility where OrganizationId = @oid and OrganizationCountryId = @ocid and Facility = @fac and IsActive = 1", new { oid = orgId, ocid = ctr.Id, fac = i.Facility }, CommandType.Text, dbTransaction);

                    var depart = await _repository.GetAsync<OrganizationDepartment>(dbConnection, "Select * from OrganizationDepartment where OrganizationId = @oid and OrganizationCountryId = @ocid and OrganizationFacilityId = @orgfacId and Department = @dept and IsActive = 1", new { oid = orgId, ocid = ctr.Id, orgfacId = facil.Id, dept = i.Department }, CommandType.Text, dbTransaction);

                    var si = new ContinuousImprovement
                    {
                        CreatedBy = uId,
                        DateCreated = DateTime.UtcNow,
                        ProblemStatement = i.ProblemStatement,
                        EndDate = Convert.ToDateTime(i.EndDate),
                        Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.ContinuousImprovementTable),
                        CountryId = (int)ctr.Id,
                        DepartmentId = (int)depart.Id,
                        FacilityId = (int)facil.Id,
                        OrganizationId = orgId,
                        Priority = i.Priority,
                        StartDate = Convert.ToDateTime(i.StartDate),
                        Status = i.Status,
                        Title = i.Title,
                        BusinessObjectiveAlignment = i.BusinessObjectiveAlignment,
                        Certification = i.Certification,
                        Currency = i.Currency,
                        IsCarryOverSavings = i.IsCarryOverSavings,
                        IsOneTimeSavings = i.IsOneTimeSavings,
                        Methodology = i.Methodology,
                        Phase = i.Phase.ToString(),
                        TotalExpectedRevenue = (decimal)i.TotalExpectedRevenue
                    };
                    var resp = await _repository.InsertAsync(dbConnection, si, dbTransaction);

                    var audit = ModelBuilder.BuildAuditLog("Continuous Improvement Added", $"Company Admin added new Continuous Improvement.", adminEmail);
                    audit.Id = await _genManager.GetNextTableId(dbConnection, dbTransaction, DatabaseScripts.AuditLogTable);
                    var auditRes = await _repository.InsertAsync(dbConnection, audit, dbTransaction);
                }

                dbTransaction.Commit();

                return await Task.FromResult(new ResponseHandler
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Successful"
                });
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                _logger.LogError($"Exception at {nameof(AddBulkCIProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler
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
