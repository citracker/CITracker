using Dapper;
using Datalayer.Interfaces;
using DataRepository;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Math;
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
using Shared.Utilities;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

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
                if (!_memoryCache.TryGetValue($"OrganizationCountry-{orgId}", out ResponseHandler<OrganizationCountry> country))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = await _repository.GetListAsync<OrganizationCountry>(dbConnection,
                        "Select * from OrganizationCountry where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text);

                    if (resi.Any())
                    {

                        country = await Task.FromResult(new ResponseHandler<OrganizationCountry>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });

                        await _memoryCacheManager.SetCache($"OrganizationCountry-{orgId}", country);
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<OrganizationCountry>
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
                if (!_memoryCache.TryGetValue($"OrganizationFacility-{orgId}", out ResponseHandler<OrganizationFacility> facility))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = await _repository.GetListAsync<OrganizationFacility>(dbConnection,
                        "Select * from OrganizationFacility where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text);

                    if (resi.Any())
                    {

                        facility = await Task.FromResult(new ResponseHandler<OrganizationFacility>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });

                        await _memoryCacheManager.SetCache($"OrganizationFacility-{orgId}", facility);
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<OrganizationFacility>
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Message = "Record not found"
                        });
                    }
                }

                return await Task.FromResult(facility);
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
                if (!_memoryCache.TryGetValue($"OrganizationDepartment-{orgId}", out ResponseHandler<OrganizationDepartment> depart))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = await _repository.GetListAsync<OrganizationDepartment>(dbConnection,
                        "Select * from OrganizationDepartment where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text);

                    if (resi.Any())
                    {

                        depart = await Task.FromResult(new ResponseHandler<OrganizationDepartment>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });

                        await _memoryCacheManager.SetCache($"OrganizationDepartment-{orgId}", depart);
                    }
                    else
                    {
                        return await Task.FromResult(new ResponseHandler<OrganizationDepartment>
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Message = "Record not found"
                        });
                    }
                }

                return await Task.FromResult(depart);
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
                if (!_memoryCache.TryGetValue($"CIUser-{orgId}", out ResponseHandler<CIUser> users))
                {
                    using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());
                    var resi = await _repository.GetListAsync<CIUser>(dbConnection,
                    "Select * from CIUser where OrganizationId = @oid and IsActive = 1", new { oid = orgId }, CommandType.Text);

                    if (resi.Any())
                    {

                        users = await Task.FromResult(new ResponseHandler<CIUser>
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Message = "Successful",
                            Result = resi
                        });

                        await _memoryCacheManager.SetCache($"CIUser-{orgId}", users);
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

                return await Task.FromResult(users);
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

                UpdateCountryListInMemory(dbConnection, dbTransaction, orgCountry.OrganizationId);

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

                    UpdateCountryListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

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

                    UpdateCountryListInMemory(dbConnection, dbTransaction, orgId);

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

                UpdateFacilityListInMemory(dbConnection, dbTransaction, orgFacility.OrganizationId);

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

                    UpdateFacilityListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

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

                    UpdateFacilityListInMemory(dbConnection, dbTransaction, orgId);

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

                UpdateDepartmentListInMemory(dbConnection, dbTransaction, orgDepartment.OrganizationId);

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

                    UpdateDepartmentListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

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

                    UpdateDepartmentListInMemory(dbConnection, dbTransaction, orgId);

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

                UpdateUserListInMemory(dbConnection, dbTransaction, orgUsr.OrganizationId);

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

                    UpdateUserListInMemory(dbConnection, dbTransaction, resi.OrganizationId);

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

                    UpdateUserListInMemory(dbConnection, dbTransaction, orgId);

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

            if (re.Any())
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

            if (re.Any())
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

            if (re.Any())
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

            if (re.Any())
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

                var query = "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Description, a.FacilitatorId, b.Name as Facilitator, a.SponsorId, b1.Name as Sponsor, a.ExecutiveSponsorId, b2.Name as ExecutiveSponsor, a.CarryOverProject, a.SavingsClassification, a.TargetSavings, a.Currency, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, a.Status, e.Department as OrganizationDepartment, a.CreatedBy, b3.Name as CreatedByStaff, (select SUM(Savings) from OperationalExcellenceMonthlySaving where ProjectId = a.Id) as ActualSavings FROM OperationalExcellence a left join CIUser b on a.FacilitatorId = b.Id left join CIUser b1 on a.SponsorId = b1.Id left join CIUser b2 on a.ExecutiveSponsorId = b2.Id left join CIUser b3 on a.CreatedBy = b3.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id where a.OrganizationId = @oid and a.Status != 'CLOSED' @where ORDER BY a.DateCreated DESC OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                var countquery = "SELECT count(id) from OperationalExcellence where OrganizationId = @oid and Status != 'CLOSED' @where";

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
                ///TODO: Include sum of all sub monthly targets
                var resi = await _repository.GetAsync<OperationalExcellenceDTO>(dbConnection,
                "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Description, a.FacilitatorId, b.Name as Facilitator, a.SponsorId, b1.Name as Sponsor, a.ExecutiveSponsorId, b2.Name as ExecutiveSponsor, a.CarryOverProject, a.SavingsClassification, a.TargetSavings, a.Currency, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, a.Status, e.Department as OrganizationDepartment, a.CreatedBy, b3.Name as CreatedByStaff, (select SUM(Savings) from OperationalExcellenceMonthlySaving where ProjectId = a.Id) as ActualSavings FROM OperationalExcellence a left join CIUser b on a.FacilitatorId = b.Id left join CIUser b1 on a.SponsorId = b1.Id left join CIUser b2 on a.ExecutiveSponsorId = b2.Id left join CIUser b3 on a.CreatedBy = b3.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id where a.OrganizationId = @oid and a.Id = @pid and a.Status != 'CLOSED'", new { oid = orgId, pid = projectId }, CommandType.Text);

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

                var audit = ModelBuilder.BuildAuditLog("Operational Excellence Monthly Saving", $"User with Id {opExel.CreatedBy} updated existing Operational Excellence Monthly Saving.", adminEmail);
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

                var query = "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Description, a.OwnerId, b.Name as Owner, a.ExecutiveSponsorId, b1.Name as ExecutiveSponsor, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, e.Department as OrganizationDepartment, a.CreatedBy, b2.Name as CreatedByStaff, COALESCE(sp.CummulativeROI, 0) AS CummulativeROI, COALESCE(sp.PercentageProgress, 0) AS PercentageProgress, COALESCE(sp.Teams, '') AS Teams FROM StrategicInitiative a left join CIUser b on a.OwnerId = b.Id left join CIUser b1 on a.ExecutiveSponsorId = b1.Id left join CIUser b2 on a.CreatedBy = b2.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id LEFT JOIN (SELECT sp.SIId, SUM(sp.Savings) AS CummulativeROI, AVG(sp.Percentage) AS PercentageProgress, STRING_AGG(u.Name, ', ') AS Teams FROM SISubProject sp LEFT JOIN CIUser u ON sp.FacilitatorId = u.Id GROUP BY sp.SIId) sp ON a.Id = sp.SIId where a.OrganizationId = @oid and a.Status != 'CLOSED' @where ORDER BY a.DateCreated DESC OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                var countquery = "SELECT count(id) from StrategicInitiative where OrganizationId = @oid and Status != 'CLOSED' @where";

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
                "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.Status, a.Description, a.OwnerId, b.Name as Owner, a.ExecutiveSponsorId, b1.Name as ExecutiveSponsor, a.OrganizationCountryId, c.Country as OrganizationCountry, a.OrganizationFacilityId, d.Facility as OrganizationFacility, a.OrganizationDepartmentId, e.Department as OrganizationDepartment, a.CreatedBy, b2.Name as CreatedByStaff, COALESCE(sp.CummulativeROI, 0) AS CummulativeROI, COALESCE(sp.PercentageProgress, 0) AS PercentageProgress, COALESCE(sp.Teams, '') AS Teams FROM StrategicInitiative a left join CIUser b on a.OwnerId = b.Id left join CIUser b1 on a.ExecutiveSponsorId = b1.Id left join CIUser b2 on a.CreatedBy = b2.Id left join OrganizationCountry c on a.OrganizationCountryId = c.Id left join OrganizationFacility d on a.OrganizationFacilityId = d.Id left join OrganizationDepartment e on a.OrganizationDepartmentId = e.Id LEFT JOIN (SELECT sp.SIId, SUM(sp.Savings) AS CummulativeROI, AVG(sp.Percentage) AS PercentageProgress, STRING_AGG(u.Name, ', ') AS Teams FROM SISubProject sp LEFT JOIN CIUser u ON sp.FacilitatorId = u.Id GROUP BY sp.SIId) sp ON a.Id = sp.SIId where a.OrganizationId = @oid and a.Id = @pid and a.Status != 'CLOSED'", new { oid = orgId, pid = projectId }, CommandType.Text);

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

        public async Task<ResponseHandler<OperationalExcellenceDTO>> GetMiniOEProjects(int orgId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<OperationalExcellenceDTO>(dbConnection,
                "SELECT Id, Title FROM OperationalExcellence where OrganizationId = @oid and Status != 'CLOSED'", new { oid = orgId }, CommandType.Text);

                if (resi.Any())
                {
                    return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = "Successful",
                        Result = resi
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
                _logger.LogError($"Exception at {nameof(GetMiniOEProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<OperationalExcellenceDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<StrategicInitiativeDTO>> GetMiniSIProjects(int orgId)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                var resi = await _repository.GetListAsync<StrategicInitiativeDTO>(dbConnection,
                "SELECT Id, Title FROM StrategicInitiative where OrganizationId = @oid and Status != 'CLOSED'", new { oid = orgId }, CommandType.Text);

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
                _logger.LogError($"Exception at {nameof(GetMiniSIProjects)} - {JsonConvert.SerializeObject(ex)}");
                return await Task.FromResult(new ResponseHandler<StrategicInitiativeDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An error occured"
                });
            }
        }

        public async Task<ResponseHandler<OrganizationToolDTO>> GetAllOrganizationTools(int orgId, string method)
        {
            try
            {
                using var dbConnection = CreateConnection(DatabaseConnectionType.MicrosoftSQLServer, await _connection.SQLDBConnection());

                method = !method.ToLower().Equals("project") ? "General" : "Project";

                var resi = await _repository.GetListAsync<OrganizationToolDTO>(dbConnection,
                "SELECT a.Id, a.Url, b.Id as ToolId, b.Tool, c.Phase FROM MethodologyTool b INNER JOIN MethodologyPhase c ON c.Id = b.Phase LEFT JOIN OrganizationTool a ON a.MethodologyTool = b.Id AND a.OrganizationId = @oid WHERE c.Methodology = @mth", new { oid = orgId, mth = method }, CommandType.Text);

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

                        if (existing.SendNotification != teamMember.SendNotification)
                        {
                            existing.SendNotification = teamMember.SendNotification;
                            existing.DateCreated = teamMember.DateCreated;
                            existing.CreatedBy = teamMember.CreatedBy;

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

                var query = "SELECT a.Id, a.OrganizationId, a.Title, a.StartDate, a.EndDate, a.Priority, a.ProblemStatement, a.Methodology, a.Certification, a.TotalExpectedRevenue, a.Currency, a.Status, a.CountryId, c.Country AS Country, a.FacilityId, d.Facility AS Facility, a.DepartmentId, e.Department AS Department, a.Phase AS PhaseId, f.Phase, tm.UserId AS FacilitatorId, u.Name AS FacilitatorName, a.CreatedBy, b1.Name AS CreatedByStaff FROM Continuousimprovement a LEFT JOIN CIProjectTeamMember tm ON tm.ProjectId = a.Id AND tm.Role = 'Facilitator' LEFT JOIN CIUser u ON u.Id = tm.UserId LEFT JOIN CIUser b1 ON a.CreatedBy = b1.Id LEFT JOIN OrganizationCountry c ON a.CountryId = c.Id LEFT JOIN OrganizationFacility d ON a.FacilityId = d.Id LEFT JOIN OrganizationDepartment e ON a.DepartmentId = e.Id LEFT JOIN MethodologyPhase f ON a.Phase = f.Id WHERE a.OrganizationId = @oid AND a.Status != 'CLOSED' @where ORDER BY a.DateCreated DESC OFFSET (@pageNumber - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                var countquery = "SELECT count(id) from ContinuousImprovement where OrganizationId = @oid and Status != 'CLOSED' @where";

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
    }
}
