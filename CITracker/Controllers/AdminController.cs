using CITracker.Validator;
using Datalayer.Interfaces;
using DocumentFormat.OpenXml.Bibliography;
using FluentValidation;
using Infastructure.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.ExternalModels;
using Shared.Models;
using Shared.Request;
using Shared.Utilities;
using Shared.ViewModels;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DriveInfo = Shared.ExternalModels.DriveInfo;

namespace CITracker.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IOptions<ADKeyValues> _config;
        private readonly IOperationManager _opsManager;
        private readonly IMicrosoftOperations _micOps;

        public AdminController(ILogger<AdminController> logger, IOptions<ADKeyValues> config, IOperationManager opsManager, IMicrosoftOperations micOps)
        {
            _logger = logger;
            _config = config;
            _opsManager = opsManager;
            _micOps = micOps;
        }


        [HttpGet("ManageOperationalLocation")]
        public IActionResult ManageOperationalLocation()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Dashboard", "Main");
            }

            var orgCountries = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

            orgCountries.Message = TempData["Message"]?.ToString() ?? orgCountries.Message;

            return View(orgCountries);
        }

        [HttpGet("ManageFacilities")]
        public IActionResult ManageFacilities()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Dashboard", "Main");
            }

            var orgCountries = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;
            var orgFacilities = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

            var faci = new OperationLocationVM
            {
                Message = TempData["Message"]?.ToString() ?? orgCountries.Message,
                Country = orgCountries.Result?.ToList(),
                Facility = orgFacilities.Result?.ToList()
            };

            return View(faci);
        }

        [HttpGet("ManageDepartments")]
        public IActionResult ManageDepartments()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Dashboard", "Main");
            }

            var orgCountries = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;
            var orgFacilities = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;
            var orgDepartments = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

            var depart = new OperationLocationVM
            {
                Message = TempData["Message"]?.ToString() ?? orgCountries.Message,
                Country = orgCountries.Result?.ToList(),
                Facility = orgFacilities.Result?.ToList(),
                Department = orgDepartments.Result?.ToList()
            };

            return View(depart);
        }

        [HttpGet("ManageUsers")]
        public IActionResult ManageUsers()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Dashboard", "Main");
            }


            var orgUsers = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

            orgUsers.Message = TempData["Message"]?.ToString() ?? orgUsers.Message;

            return View(orgUsers);
        }

        [HttpGet("ManageTools")]
        public IActionResult ManageTools()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Dashboard", "Main");
            }

            var sites = new List<DriveInfo>();
            ////check if tenant has an existing sharepoint site
            //var hasSiteId = _opsManager.CheckIfTenantHasSiteId(HttpContext.Session.GetString("TenantId")).Result;

            //if(!hasSiteId)
            //{
            //    //discover sharepoint sites
            //    sites = _micOps.DiscoverSharePointSites(HttpContext.Session.GetString("TenantId"), _config.Value.ClientId, _config.Value.ClientSecret).Result;
            //}

            var res = new ManageToolsVM
            {
                Phases = _opsManager.GetMethodologyPhases().Result.Result.ToList(),
                Sites = sites
            };

            return View(res);
        }

        [HttpGet("ManageSavingCategory")]
        public IActionResult ManageSavingCategory()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Dashboard", "Main");
            }


            var orgsc = _opsManager.GetAllOrganizationSavingCategory(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

            orgsc.Message = TempData["Message"]?.ToString() ?? orgsc.Message;

            return View(orgsc);
        }

        [HttpGet("BulkUploads")]
        public IActionResult BulkUploads()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost("BulkUpload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpload([FromBody] BulkUploadRequest request)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!IsUserAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (request.Rows == null || !request.Rows.Any())
                return BadRequest("No data submitted");

            var resp = new ResponseHandler();

            // Route logic by upload type
            switch (request.UploadType)
            {
                case 1:
                    resp = await HandleUserUpload(request.Rows);
                    break;

                case 2:
                    resp = await HandleLocationUpload(request.Rows);
                    break;

                case 3:
                    resp = await HandleOperationalExcellenceUpload(request.Rows);
                    break;

                case 4:
                    resp = await HandleStrategicInitiativeUpload(request.Rows);
                    break;

                case 5:
                    resp = await HandleContinuousImprovementUpload(request.Rows);
                    break;

                default:
                    return BadRequest(new { success = false, message = "Invalid upload type" });
            }

            if(resp.StatusCode != (int)HttpStatusCode.OK)
                return StatusCode((int)HttpStatusCode.ExpectationFailed, new { message = resp.Message });
            else
                return Ok(resp.Message);
        }

        [HttpPost("AddCountry")]
        [ValidateAntiForgeryToken]
        public IActionResult AddCountry()
        {
            try
            {
                var ordCty = new OrganizationCountry
                {
                    Country = Request.Form["country"],
                    DateCreated = DateTime.UtcNow,
                    IsActive = true,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId"))
                };

                var res = _opsManager.AddOrganizationCountry(ordCty, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageOperationalLocation", "Admin");
            }
            catch(Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(AddCountry)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageOperationalLocation", "Admin");
            }
        }

        [HttpPost("RenameCountry")]
        [ValidateAntiForgeryToken]
        public IActionResult RenameCountry()
        {
            try
            {
                var res = _opsManager.RenameOrganizationCountry(Convert.ToInt64(Request.Form["country"]), Request.Form["input"], HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageOperationalLocation", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(RenameCountry)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageOperationalLocation", "Admin");
            }
        }

        [HttpPost("DeleteCountry")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCountry()
        {
            try
            {
                var res = _opsManager.DeleteOrganizationCountry(Convert.ToInt64(Request.Form["country"]), HttpContext.Session.GetString("UserEmail"), Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageOperationalLocation", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(DeleteCountry)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageOperationalLocation", "Admin");
            }
        }

        [HttpPost("AddFacility")]
        [ValidateAntiForgeryToken]
        public IActionResult AddFacility()
        {
            try
            {
                var ordFac = new OrganizationFacility
                {
                    Facility = Request.Form["facility"],
                    DateCreated = DateTime.UtcNow,
                    IsActive = true,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    OrganizationCountryId = Convert.ToInt64(Request.Form["country"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId"))
                };

                var res = _opsManager.AddOrganizationFacility(ordFac, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageFacilities", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(AddFacility)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageFacilities", "Admin");
            }
        }

        [HttpPost("RenameFacility")]
        [ValidateAntiForgeryToken]
        public IActionResult RenameFacility()
        {
            try
            {
                var res = _opsManager.RenameOrganizationFacility(Convert.ToInt64(Request.Form["facilityR"]), Request.Form["facilityN"], HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageFacilities", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(RenameFacility)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageFacilities", "Admin");
            }
        }

        [HttpPost("DeleteFacility")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFacility()
        {
            try
            {
                var res = _opsManager.DeleteOrganizationFacility(Convert.ToInt64(Request.Form["facilityD"]), HttpContext.Session.GetString("UserEmail"), Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageFacilities", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(DeleteFacility)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageFacilities", "Admin");
            }
        }

        [HttpPost("AddDepartment")]
        [ValidateAntiForgeryToken]
        public IActionResult AddDepartment()
        {
            try
            {
                var ordDep = new OrganizationDepartment
                {
                    Department = Request.Form["department"],
                    DateCreated = DateTime.UtcNow,
                    IsActive = true,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    OrganizationCountryId = Convert.ToInt64(Request.Form["country"]),
                    OrganizationFacilityId = Convert.ToInt64(Request.Form["facilityA"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId"))
                };

                var res = _opsManager.AddOrganizationDepartment(ordDep, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageDepartments", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(AddDepartment)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageDepartments", "Admin");
            }
        }

        [HttpPost("RenameDepartment")]
        [ValidateAntiForgeryToken]
        public IActionResult RenameDepartment()
        {
            try
            {
                var res = _opsManager.RenameOrganizationDepartment(Convert.ToInt64(Request.Form["departmentR"]), Request.Form["departmentN"], HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageDepartments", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(RenameDepartment)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageDepartments", "Admin");
            }
        }

        [HttpPost("UploadToolDocument")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadToolDocument(IFormFile file, int toolId, string toolName)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            // 1️⃣ Whitelist allowed extensions
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type.");

            // 2️⃣ Generate safe file name (never trust client filename)
            var safeFileName = $"{Guid.NewGuid()}{extension}";

            // 3️⃣ Target directory (wwwroot/uploads/tools)
            var uploadRoot = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                $"Org-{HttpContext.Session.GetString("OrganizationId")}",
                "toolTemplates"
            );

            if (!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            var fullPath = Path.Combine(uploadRoot, safeFileName);

            // 4️⃣ Save file safely
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5️⃣ Return relative URL (never physical path)
            var fileUrl = $"/uploads/Org-{HttpContext.Session.GetString("OrganizationId")}/toolTemplates/{safeFileName}";

            var orgTool = new OrganizationTool
            {
                Url = fileUrl,
                MethodologyTool = toolId,
                DateCreated = DateTime.UtcNow,
                OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId"))
            };

            var res = _opsManager.UpdateOrganizationTool(orgTool, HttpContext.Session.GetString("UserEmail"));

            return Ok(new
            {
                toolId,
                fileUrl
            });
        }

        [HttpPost("DeleteDepartment")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDepartment()
        {
            try
            {
                var res = _opsManager.DeleteOrganizationDepartment(Convert.ToInt64(Request.Form["departmentD"]), HttpContext.Session.GetString("UserEmail"), Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageDepartments", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(DeleteDepartment)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageDepartments", "Admin");
            }
        }

        [HttpPost("AddUser")]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser()
        {
            try
            {
                var orgUsr = new CIUser
                {
                    EmailAddress = Request.Form["em"],
                    DateCreated = DateTime.UtcNow,
                    IsActive = true,
                    Name = $"{Request.Form["fn"]} {Request.Form["ln"]}",
                    Role = Shared.Enumerations.Role.User.ToString(),
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId"))
                };

                var res = _opsManager.AddOrganizationUser(orgUsr, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageUsers", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(AddUser)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageUsers", "Admin");
            }
        }

        [HttpPost("RenameUser")]
        [ValidateAntiForgeryToken]
        public IActionResult RenameUser()
        {
            try
            {
                var orgUsr = new CIUser
                {
                    EmailAddress = Request.Form["emN"],
                    Name = $"{Request.Form["fnN"]} {Request.Form["lnN"]}"
                };
                var res = _opsManager.RenameOrganizationUser(Convert.ToInt64(Request.Form["user"]), orgUsr, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageUsers", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(RenameUser)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageUsers", "Admin");
            }
        }

        [HttpPost("DeleteUser")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser()
        {
            try
            {
                var res = _opsManager.DeleteOrganizationUser(Convert.ToInt64(Request.Form["userD"]), HttpContext.Session.GetString("UserEmail"), Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageUsers", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(DeleteUser)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageUsers", "Admin");
            }
        }

        [HttpPost("AddSoftSavingCategory")]
        [ValidateAntiForgeryToken]
        public IActionResult AddSoftSavingCategory()
        {
            try
            {
                var ordss = new OrganizationSoftSaving
                {
                    DateCreated = DateTime.UtcNow,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    Category = Request.Form["ssc"],
                    Unit = Request.Form["ssu"],
                    IsActive = true
                };

                var res = _opsManager.AddOrganizationSoftSaving(ordss, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageSavingCategory", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(AddSoftSavingCategory)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageSavingCategory", "Admin");
            }
        }

        [HttpPost("ModifySoftSavingCategory")]
        [ValidateAntiForgeryToken]
        public IActionResult ModifySoftSavingCategory()
        {
            try
            {
                var orgUsr = new OrganizationSoftSaving
                {
                    Category = Request.Form["sscM"],
                    Unit = Request.Form["ssuM"]                    
                };
                var res = _opsManager.RenameOrganizationSoftSaving(Convert.ToInt64(Request.Form["sss"]), orgUsr, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageSavingCategory", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(ModifySoftSavingCategory)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageSavingCategory", "Admin");
            }
        }

        [HttpPost("DeleteSoftSavingCategory")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSoftSavingCategory()
        {
            try
            {
                var res = _opsManager.DeleteOrganizationSoftSaving(Convert.ToInt64(Request.Form["sssD"]), HttpContext.Session.GetString("UserEmail"), Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result;

                TempData["Message"] = res.Message;

                return RedirectToAction("ManageSavingCategory", "Admin");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(DeleteSoftSavingCategory)} - {JsonConvert.SerializeObject(e)}");
                return RedirectToAction("ManageSavingCategory", "Admin");
            }
        }

        private async Task<ResponseHandler> HandleUserUpload(List<Dictionary<string, string>> rows)
        {
            try
            {
                // 1. Map rows → strongly typed DTO
                var usrs = rows.Select(r => new BulkUser
                {                    
                    IsActive = true,
                    Role = Shared.Enumerations.Role.User.ToString(),
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    EmailAddress = r.GetValueOrDefault("EmailAddress")?.Trim(),
                    Name = r.GetValueOrDefault("Name")?.Trim(),
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId"))
                }).ToList();

                // 2. Validate
                var validator = new BulkUserValidator();
                var failures = usrs
                    .Select((u, i) => new { Index = i, Result = validator.Validate(u) })
                    .Where(x => !x.Result.IsValid)
                    .ToList();

                if (failures.Any())
                {
                    var errors = failures.SelectMany(x => x.Result.Errors).Select(x => x.ErrorMessage).ToList();

                    return new ResponseHandler
                    {
                        Message = string.Join(", ", errors),
                        StatusCode = (int)HttpStatusCode.ExpectationFailed
                    };
                }

                var users = usrs.Select(u => new CIUser
                {
                    CreatedBy = (long) u.CreatedBy,
                    DateCreated = (DateTime) u.DateCreated,
                    EmailAddress = u.EmailAddress,
                    IsActive = (bool) u.IsActive,
                    Name = u.Name,
                    OrganizationId = (int) u.OrganizationId,
                    Role = u.Role
                }).ToList();

                // 3. De-duplicate by Email (Excel + DB safety)
                users = users
                    .GroupBy(u => u.EmailAddress.ToLower())
                    .Select(g => g.First())
                    .ToList();

                return await _opsManager.AddOrganizationUsers(users, HttpContext.Session.GetString("UserEmail"));
            }
            catch(Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(HandleUserUpload)} - {JsonConvert.SerializeObject(e)}");
                return new ResponseHandler
                {
                    Message = e.Message,
                    Error = e,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private async Task<ResponseHandler> HandleLocationUpload(List<Dictionary<string, string>> rows)
        {
            try
            {
                // 1. Map rows → strongly typed DTO
                var locs = rows.Select(r => new BulkLocation
                {
                    Country = r.GetValueOrDefault("Country")?.Trim(),
                    Facility = r.GetValueOrDefault("Facility")?.Trim(),
                    Department = r.GetValueOrDefault("Department")?.Trim()
                }).ToList();

                // 2. Validate
                var validator = new BulkLocationValidator();
                var failures = locs
                    .Select((u, i) => new { Index = i, Result = validator.Validate(u) })
                    .Where(x => !x.Result.IsValid)
                    .ToList();

                if (failures.Any())
                {
                    var errors = failures.SelectMany(x => x.Result.Errors).Select(x => x.ErrorMessage).ToList();

                    return new ResponseHandler
                    {
                        Message = string.Join(", ", errors),
                        StatusCode = (int)HttpStatusCode.ExpectationFailed
                    };
                }

                // 3. De-duplicate by Email (Excel + DB safety)
                locs = locs
                    .GroupBy(x => new
                    {
                        Country = x.Country.ToLower(),
                        Facility = x.Facility.ToLower(),
                        Department = x.Department.ToLower()
                    })
                    .Select(g => g.First())
                    .ToList();

                return await _opsManager.AddOrganizationLocations(locs, Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), Convert.ToInt64(HttpContext.Session.GetString("UserId")), HttpContext.Session.GetString("UserEmail"));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(HandleLocationUpload)} - {JsonConvert.SerializeObject(e)}");
                return new ResponseHandler
                {
                    Message = e.Message,
                    Error = e,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private async Task<ResponseHandler> HandleOperationalExcellenceUpload(List<Dictionary<string, string>> rows)
        {
            try
            {
                // 1. Map rows → strongly typed DTO
                var oe = rows.Select(r => new BulkOE
                {
                    Title = r.GetValueOrDefault("Title")?.Trim(),
                    Country = r.GetValueOrDefault("Country")?.Trim(),
                    Facility = r.GetValueOrDefault("Facility")?.Trim(),
                    Department = r.GetValueOrDefault("Department")?.Trim(),
                    Currency = r.GetValueOrDefault("Currency")?.Trim(),
                    Description = r.GetValueOrDefault("Description")?.Trim(),
                    EndDate = r.GetValueOrDefault("EndDate")?.Trim(),
                    ExecutiveSponsorEmailAddress = r.GetValueOrDefault("ExecutiveSponsorEmailAddress")?.Trim(),
                    FacilitatorEmailAddress = r.GetValueOrDefault("FacilitatorEmailAddress")?.Trim(),
                    IsCarryOverProject = r.GetValueOrDefault("IsCarryOverProject?")?.Trim(),
                    SavingsClassification = r.GetValueOrDefault("SavingsClassification")?.Trim(),
                    Priority = r.GetValueOrDefault("Priority")?.Trim(),
                    SponsorEmailAddress = r.GetValueOrDefault("SponsorEmailAddress")?.Trim(),
                    StartDate = r.GetValueOrDefault("StartDate")?.Trim(),
                    Status = r.GetValueOrDefault("Status")?.Trim().ToUpper(),
                    TargetSavings = Convert.ToDecimal(r.GetValueOrDefault("TargetSavings")?.Trim())
                }).ToList();

                // 2. Validate
                var validator = new BulkOEValidator();
                var failures = oe
                    .Select((u, i) => new { Index = i, Result = validator.Validate(u) })
                    .Where(x => !x.Result.IsValid)
                    .ToList();

                if (failures.Any())
                {
                    var errors = failures.SelectMany(x => x.Result.Errors).Select(x => x.ErrorMessage).ToList();

                    return new ResponseHandler
                    {
                        Message = string.Join(", ", errors),
                        StatusCode = (int)HttpStatusCode.ExpectationFailed
                    };
                }

                // 3. De-duplicate by Email (Excel + DB safety)
                oe = oe
                    .GroupBy(x => new
                    {
                        Title = x.Title.ToLower()
                    })
                    .Select(g => g.First())
                    .ToList();

                return await _opsManager.AddBulkOEProjects(oe, Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), Convert.ToInt64(HttpContext.Session.GetString("UserId")), HttpContext.Session.GetString("UserEmail"));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(HandleOperationalExcellenceUpload)} - {JsonConvert.SerializeObject(e)}");
                return new ResponseHandler
                {
                    Message = e.Message,
                    Error = e,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private async Task<ResponseHandler> HandleStrategicInitiativeUpload(List<Dictionary<string, string>> rows)
        {
            try
            {
                // 1. Map rows → strongly typed DTO
                var si = rows.Select(r => new BulkSI
                {
                    Title = r.GetValueOrDefault("Title")?.Trim(),
                    Country = r.GetValueOrDefault("Country")?.Trim(),
                    Facility = r.GetValueOrDefault("Facility")?.Trim(),
                    Department = r.GetValueOrDefault("Department")?.Trim(),
                    Description = r.GetValueOrDefault("Description")?.Trim(),
                    EndDate = r.GetValueOrDefault("EndDate")?.Trim(),
                    ExecutiveSponsorEmailAddress = r.GetValueOrDefault("ExecutiveSponsorEmailAddress")?.Trim(),
                    OwnerEmailAddress = r.GetValueOrDefault("OwnerEmailAddress")?.Trim(),
                    Priority = r.GetValueOrDefault("Priority")?.Trim(),
                    StartDate = r.GetValueOrDefault("StartDate")?.Trim(),
                    Status = r.GetValueOrDefault("Status")?.Trim().ToUpper()
                }).ToList();

                // 2. Validate
                var validator = new BulkSIValidator();
                var failures = si
                    .Select((u, i) => new { Index = i, Result = validator.Validate(u) })
                    .Where(x => !x.Result.IsValid)
                    .ToList();

                if (failures.Any())
                {
                    var errors = failures.SelectMany(x => x.Result.Errors).Select(x => x.ErrorMessage).ToList();

                    return new ResponseHandler
                    {
                        Message = string.Join(", ", errors),
                        StatusCode = (int)HttpStatusCode.ExpectationFailed
                    };
                }

                // 3. De-duplicate by Email (Excel + DB safety)
                si = si
                    .GroupBy(x => new
                    {
                        Title = x.Title.ToLower()
                    })
                    .Select(g => g.First())
                    .ToList();

                return await _opsManager.AddBulkSIProjects(si, Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), Convert.ToInt64(HttpContext.Session.GetString("UserId")), HttpContext.Session.GetString("UserEmail"));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(HandleStrategicInitiativeUpload)} - {JsonConvert.SerializeObject(e)}");
                return new ResponseHandler
                {
                    Message = e.Message,
                    Error = e,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private async Task<ResponseHandler> HandleContinuousImprovementUpload(List<Dictionary<string, string>> rows)
        {
            try
            {
                // 1. Map rows → strongly typed DTO
                var ci = rows.Select(r => new BulkCI
                {
                    Title = r.GetValueOrDefault("Title")?.Trim(),
                    Country = r.GetValueOrDefault("Country")?.Trim(),
                    Facility = r.GetValueOrDefault("Facility")?.Trim(),
                    Department = r.GetValueOrDefault("Department")?.Trim(),
                    ProblemStatement = r.GetValueOrDefault("ProblemStatement")?.Trim(),
                    EndDate = r.GetValueOrDefault("EndDate")?.Trim(),
                    Priority = r.GetValueOrDefault("Priority")?.Trim(),
                    StartDate = r.GetValueOrDefault("StartDate")?.Trim(),
                    Status = r.GetValueOrDefault("Status")?.Trim().ToUpper(),
                    BusinessObjectiveAlignment = r.GetValueOrDefault("BusinessObjectiveAlignment")?.Trim(),
                    Methodology = r.GetValueOrDefault("Methodology")?.Trim(),
                    Certification = r.GetValueOrDefault("Certification")?.Trim(),
                    Currency = r.GetValueOrDefault("Currency")?.Trim(),
                    IsCarryOverSavings = r.GetValueOrDefault("IsCarryOverSavings?")?.Trim().ToLower() == "yes" ? true : false,
                    IsOneTimeSavings = r.GetValueOrDefault("IsOneTimeSavings?")?.Trim().ToLower() == "yes" ? true : false,
                    Phase = Convert.ToInt32(r.GetValueOrDefault("Phase")?.Trim()),
                    SupportingValueStream = r.GetValueOrDefault("SupportingValueStream")?.Trim(),
                    TotalExpectedRevenue = Convert.ToDecimal(r.GetValueOrDefault("TotalExpectedRevenue")?.Trim())
                }).ToList();

                // 2. Validate
                var validator = new BulkCIValidator();
                var failures = ci
                    .Select((u, i) => new { Index = i, Result = validator.Validate(u) })
                    .Where(x => !x.Result.IsValid)
                    .ToList();

                if (failures.Any())
                {
                    var errors = failures.SelectMany(x => x.Result.Errors).Select(x => x.ErrorMessage).ToList();

                    return new ResponseHandler
                    {
                        Message = string.Join(", ", errors),
                        StatusCode = (int)HttpStatusCode.ExpectationFailed
                    };
                }

                // 3. De-duplicate by Email (Excel + DB safety)
                ci = ci
                    .GroupBy(x => new
                    {
                        Title = x.Title.ToLower()
                    })
                    .Select(g => g.First())
                    .ToList();

                return await _opsManager.AddBulkCIProjects(ci, Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), Convert.ToInt64(HttpContext.Session.GetString("UserId")), HttpContext.Session.GetString("UserEmail"));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Occurred at {nameof(HandleContinuousImprovementUpload)} - {JsonConvert.SerializeObject(e)}");
                return new ResponseHandler
                {
                    Message = e.Message,
                    Error = e,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }

        private bool IsAuthenticated()
        {
            if (User.Identity.IsAuthenticated)
            {
                return true;
            }
            return false;
        }

        private bool IsUserAdmin()
        {
            if (HttpContext.Session.GetString("UserRole").Equals("Admin"))
            {
                return true;
            }
            return false;
        }
    }
}
