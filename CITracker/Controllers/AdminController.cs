using Datalayer.Interfaces;
using Infastructure.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.ExternalModels;
using Shared.Models;
using Shared.Request;
using Shared.Utilities;
using Shared.ViewModels;
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
            //check if tenant has an existing sharepoint site
            var hasSiteId = _opsManager.CheckIfTenantHasSiteId(HttpContext.Session.GetString("TenantId")).Result;

            if(!hasSiteId)
            {
                //discover sharepoint sites
                sites = _micOps.DiscoverSharePointSites(HttpContext.Session.GetString("TenantId"), _config.Value.ClientId, _config.Value.ClientSecret).Result;
            }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadToolDocument(ToolUploadRequest tool)
        {
            if (tool.File == null || tool.File.Length == 0)
                return BadRequest("No file uploaded");

            if (!Utils.IsValidWordDocument(tool.File))
                return BadRequest("Invalid or corrupted Word document");

            //// Upload to SharePoint
            //var fileUrl = await _sharePointService.UploadAsync(
            //    file,
            //    toolName,
            //    User.GetOrganizationId()
            //);

            //// Persist URL against tool + org
            //await _repository.SaveToolUploadAsync(
            //    toolId,
            //    fileUrl,
            //    User.GetOrganizationId()
            //);

            //return Ok(new { url = fileUrl });
            return Json("");
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
