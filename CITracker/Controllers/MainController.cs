using Datalayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.Models;
using Shared.ViewModels;
using System.Text.RegularExpressions;

namespace CITracker.Controllers
{
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        private readonly IOperationManager _opsManager;
        private readonly IOptions<KeyValues> _config;

        public MainController(ILogger<MainController> logger, IOptions<KeyValues> config, IOperationManager opsManager)
        {
            _opsManager = opsManager;
            _logger = logger;
            _config = config;
        }


        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("Initiatives")]
        public IActionResult Initiatives()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("CreateCIProject")]
        public IActionResult CreateCIProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new ContinuousImprovementVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OperationalExcellence = _opsManager.GetMiniOEProjects(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                StrategicInitiative = _opsManager.GetMiniSIProjects(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList()
                //MethodologyTool = _opsManager.GetAllOrganizationTools(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList()
            };

            return View(coep);
        }

        [HttpGet("CIProjectDetail")]
        public IActionResult CIProjectDetail()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("AllCIProjects")]
        public IActionResult AllCIProjects()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet("CreateOEProject")]
        public IActionResult CreateOEProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new OperationalExcellenceVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList()
            };

            return View(coep);
        }

        [HttpPost("NewOEProject")]
        [ValidateAntiForgeryToken]
        public IActionResult NewOEProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var newOEProject = new OperationalExcellence
                {
                    Title = Request.Form["title"],
                    StartDate = Convert.ToDateTime(Request.Form["startdate"]).Date,
                    EndDate = Convert.ToDateTime(Request.Form["enddate"]).Date,
                    Priority = Request.Form["priority"],
                    Description = Request.Form["description"],
                    FacilitatorId = Convert.ToInt64(Request.Form["facilitator"]),
                    SponsorId = Convert.ToInt64(Request.Form["sponsor"]),
                    ExecutiveSponsorId = Convert.ToInt64(Request.Form["execsponsor"]),
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    CarryOverProject = Request.Form["carryover"],
                    SavingsClassification = Request.Form["savingsclassification"],
                    TargetSavings = Convert.ToDecimal(Request.Form["targetsavings"]),
                    Currency = Request.Form["currency"],
                    Status = Request.Form["status"],
                    OrganizationCountryId = Convert.ToInt64(Request.Form["country"]),
                    OrganizationFacilityId = Convert.ToInt64(Request.Form["facility"]),
                    OrganizationDepartmentId = Convert.ToInt64(Request.Form["department"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.CreateNewOEProject(newOEProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("CreateOEProject", "Main");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewOEProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the OE Project. {ex.Message}";
                return RedirectToAction("CreateOEProject", "Main");
            }
        }

        [HttpGet("AllOEProjects")]
        public IActionResult AllOEProjects(int page = 1, InitiativeFilter filt = null)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return LoadPaginatedOE(page, filt);
        }

        private IActionResult LoadPaginatedOE(int page, InitiativeFilter filt)
        {
            var coep = new OperationalExcellenceVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOperationalExcellenceUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Projects = _opsManager.GetPaginatedOEProjects(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), page, _config.Value.PageSize, filt).Result
            };

            return View("AllOEProjects", coep);
        }

        [HttpPost("FilteredOEProjects")]
        [ValidateAntiForgeryToken]
        public IActionResult FilteredOEProjects(int page = 1)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var filt = new InitiativeFilter
                {
                    Title = Request.Form["title"],
                    StartDate = String.IsNullOrEmpty(Request.Form["startdate"]) ? new DateTime() : Convert.ToDateTime(Request.Form["startdate"]),
                    EndDate = String.IsNullOrEmpty(Request.Form["enddate"]) ? new DateTime() : Convert.ToDateTime(Request.Form["enddate"]),
                    Priority = Request.Form["priority"],
                    Status = Request.Form["status"],
                    DepartmentId = Convert.ToInt64(Request.Form["department"]),
                    CountryId = Convert.ToInt32(Request.Form["country"]),
                    UserId = Convert.ToInt64(Request.Form["users"])
                };

                return LoadPaginatedOE(page, filt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(FilteredOEProjects)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the OE Project. {ex.Message}";
                return LoadPaginatedOE(page, null);
            }
        }

        [HttpPost("UpdateOEProject")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOEProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }
            var id = Convert.ToInt64(Request.Form["id"]);
            try
            {
                var existingOEProject = new OperationalExcellence
                {
                    Id = id,
                    Title = Request.Form["title"],
                    StartDate = Convert.ToDateTime(Request.Form["startdate"]).Date,
                    EndDate = Convert.ToDateTime(Request.Form["enddate"]).Date,
                    Priority = Request.Form["priority"],
                    Description = Request.Form["description"],
                    FacilitatorId = Convert.ToInt64(Request.Form["facilitator"]),
                    SponsorId = Convert.ToInt64(Request.Form["sponsor"]),
                    ExecutiveSponsorId = Convert.ToInt64(Request.Form["execsponsor"]),
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    CarryOverProject = Request.Form["carryover"],
                    SavingsClassification = Request.Form["savingsclassification"],
                    TargetSavings = Convert.ToDecimal(Request.Form["targetsavings"]),
                    Currency = Request.Form["currency"],
                    Status = Request.Form["status"],
                    OrganizationCountryId = Convert.ToInt64(Request.Form["country"]),
                    OrganizationFacilityId = Convert.ToInt64(Request.Form["facility"]),
                    OrganizationDepartmentId = Convert.ToInt64(Request.Form["department"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.UpdateExistingOEProject(existingOEProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("OEProjectDetail", "Main", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewOEProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the OE Project. {ex.Message}";
                return RedirectToAction("OEProjectDetail", "Main", new { id = id });
            }
        }

        [HttpGet("OEProjectDetail")]
        public IActionResult OEProjectDetail(int id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new OperationalExcellenceVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOperationalExcellenceUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Projects = _opsManager.GetOEProject(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), id).Result,
                MonthlySavings = _opsManager.GetOEProjectMonthlySavings(id)?.Result?.Result?.ToList()
            };

            return View(coep);
        }

        [HttpPost("AddOEProjectMonthlySavings")]
        [ValidateAntiForgeryToken]
        public IActionResult AddOEProjectMonthlySavings()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }
            var id = Convert.ToInt64(Request.Form["edid"]);
            try
            {
                var newOEProjectMS = new OperationalExcellenceMonthlySaving
                {
                    ProjectId = id,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    MonthYear = Request.Form["monthyear"],
                    Currency = Request.Form["currency"],
                    Savings = Convert.ToDecimal(Request.Form["savings"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.CreateNewOEProjectMonthlySavings(newOEProjectMS, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("OEProjectDetail", "Main", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(AddOEProjectMonthlySavings)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the OE Project. {ex.Message}";
                return RedirectToAction("OEProjectDetail", "Main", new { id = id });
            }
        }

        [HttpGet("OEMonthlySavingDetail")]
        public IActionResult OEMonthlySavingDetail(long Id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = _opsManager.GetOEProjectMonthlySaving(Id)?.Result?.SingleResult;

            return View(coep);
        }

        [HttpGet("GetMethodologyPhases")]
        public IActionResult GetMethodologyPhases(string val)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = _opsManager.GetMethodologyPhases(val)?.Result?.Result?.ToList();

            return Json(coep);
        }

        [HttpGet("GetMethodologyTools")]
        public IActionResult GetMethodologyTools(string val)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = _opsManager.GetAllOrganizationTools(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), val)?.Result?.Result?.ToList();

            var gcoep = coep.GroupBy(t => t.Phase).ToList();

            var result = gcoep.ToDictionary(
                g => g.Key,
                g => g.Select(x => new
                {
                    id = x.Id,
                    name = x.Tool,
                    url = x.Url
                }).ToList()
            );

            return Json(result);
        }

        [HttpGet("OEMonthlySaving")]
        public IActionResult OEMonthlySaving(long pId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = _opsManager.GetOEProjectMonthlySavings(pId)?.Result?.Result?.ToList();

            return Json(coep);
        }

        [HttpPost("UpdateOEProjectMonthlySavings")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOEProjectMonthlySavings()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }
            var id = Convert.ToInt64(Request.Form["msid"]);
            var pjid = Convert.ToInt64(Request.Form["pjid"]);
            try
            {
                var newOEProjectMS = new OperationalExcellenceMonthlySaving
                {
                    Id = id,
                    ProjectId = pjid,
                    OrganizationId = Convert.ToInt32(Request.Form["orgid"]),
                    MonthYear = Request.Form["monthyear"],
                    Currency = Request.Form["currency"],
                    Savings = Convert.ToDecimal(Request.Form["savings"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.UpdateOEProjectMonthlySavings(newOEProjectMS, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("OEProjectDetail", "Main", new { id = pjid });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(UpdateOEProjectMonthlySavings)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the OE Project. {ex.Message}";
                return RedirectToAction("OEProjectDetail", "Main", new { id = pjid });
            }
        }

        [HttpGet("CreateSIProject")]
        public IActionResult CreateSIProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new StrategicInitiativeVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList()
            };

            return View(coep);
        }

        [HttpPost("NewSIProject")]
        [ValidateAntiForgeryToken]
        public IActionResult NewSIProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var newSIProject = new StrategicInitiative
                {
                    Title = Request.Form["title"],
                    StartDate = Convert.ToDateTime(Request.Form["startdate"]).Date,
                    EndDate = Convert.ToDateTime(Request.Form["enddate"]).Date,
                    Priority = Request.Form["priority"],
                    Description = Request.Form["description"],
                    OwnerId = Convert.ToInt64(Request.Form["owner"]),
                    Status = Request.Form["status"],
                    ExecutiveSponsorId = Convert.ToInt64(Request.Form["execsponsor"]),
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    OrganizationCountryId = Convert.ToInt64(Request.Form["country"]),
                    OrganizationFacilityId = Convert.ToInt64(Request.Form["facility"]),
                    OrganizationDepartmentId = Convert.ToInt64(Request.Form["department"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.CreateNewSIProject(newSIProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("CreateSIProject", "Main");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewSIProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the SI Project. {ex.Message}";
                return RedirectToAction("CreateSIProject", "Main");
            }
        }

        [HttpGet("AllSIProjects")]
        public IActionResult AllSIProjects(int page = 1, InitiativeFilter filt = null)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return LoadPaginatedSI(page, filt);
        }

        private IActionResult LoadPaginatedSI(int page, InitiativeFilter filt)
        {
            var coep = new StrategicInitiativeVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllStrategicInitiativeUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Initiative = _opsManager.GetPaginatedSIProjects(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), page, _config.Value.PageSize, filt).Result
            };

            return View("AllSIProjects", coep);
        }

        [HttpPost("FilteredSIProjects")]
        [ValidateAntiForgeryToken]
        public IActionResult FilteredSIProjects(int page = 1)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var filt = new InitiativeFilter
                {
                    Title = Request.Form["title"],
                    StartDate = String.IsNullOrEmpty(Request.Form["startdate"]) ? new DateTime() : Convert.ToDateTime(Request.Form["startdate"]),
                    EndDate = String.IsNullOrEmpty(Request.Form["enddate"]) ? new DateTime() : Convert.ToDateTime(Request.Form["enddate"]),
                    Priority = Request.Form["priority"],
                    Status = Request.Form["status"],
                    DepartmentId = Convert.ToInt64(Request.Form["department"]),
                    CountryId = Convert.ToInt32(Request.Form["country"]),
                    UserId = Convert.ToInt64(Request.Form["users"])
                };

                return LoadPaginatedSI(page, filt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(FilteredSIProjects)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while fetching the SI Project. {ex.Message}";
                return LoadPaginatedSI(page, null);
            }
        }

        [HttpGet("CreateSISubProject")]
        public IActionResult CreateSISubProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new StrategicInitiativeVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Initiative = _opsManager.GetAllInProgressOrganizationSI(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result
            };

            return View(coep);
        }

        [HttpPost("NewSISubProject")]
        [ValidateAntiForgeryToken]
        public IActionResult NewSISubProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var newSISubProject = new SISubProject
                {
                    Initiative = Request.Form["title"],
                    SIId = Convert.ToInt64(Request.Form["siproject"]),
                    StartDate = Convert.ToDateTime(Request.Form["startdate"]).Date,
                    EndDate = Convert.ToDateTime(Request.Form["enddate"]).Date,
                    Description = Request.Form["description"],
                    FacilitatorId = Convert.ToInt64(Request.Form["facilitator"]),
                    Currency = Request.Form["currency"],
                    Percentage = Convert.ToDecimal(Request.Form["percent"]),
                    Savings = Convert.ToDecimal(Request.Form["targetsavings"]),
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.CreateNewSISubProject(newSISubProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("CreateSISubProject", "Main");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewSISubProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the SI Sub Project. {ex.Message}";
                return RedirectToAction("CreateSISubProject", "Main");
            }
        }

        [HttpGet("SISubProject")]
        public IActionResult SISubProject(long pId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = _opsManager.GetSISubProjects(pId)?.Result?.Result?.ToList();

            return Json(coep);
        }

        [HttpGet("SISubProjectDetail")]
        public IActionResult SISubProjectDetail(long Id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new StrategicInitiativeVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Initiative = _opsManager.GetAllInProgressOrganizationSI(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"))).Result,
                SISubProject = _opsManager.GetSISubProject(Id)?.Result?.Result?.FirstOrDefault()
            };

            return View(coep);
        }

        [HttpGet("SIProjectInitiativeDetail")]
        public IActionResult SIProjectInitiativeDetail(long Id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }


            var coep = new StrategicInitiativeVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Initiative = _opsManager.GetSIProject(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), Id).Result,
                SubProject = _opsManager.GetSISubProjects(Id)?.Result?.Result?.ToList()
            };

            return View(coep);
        }

        [HttpPost("UpdateSIProject")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSIProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }
            var id = Convert.ToInt64(Request.Form["id"]);
            try
            {
                var existingSIProject = new StrategicInitiative
                {
                    Id = id,
                    Title = Request.Form["title"],
                    StartDate = Convert.ToDateTime(Request.Form["startdate"]).Date,
                    EndDate = Convert.ToDateTime(Request.Form["enddate"]).Date,
                    Priority = Request.Form["priority"],
                    Status = Request.Form["status"],
                    Description = Request.Form["description"],
                    OwnerId = Convert.ToInt64(Request.Form["owner"]),
                    ExecutiveSponsorId = Convert.ToInt64(Request.Form["execsponsor"]),
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    OrganizationCountryId = Convert.ToInt64(Request.Form["country"]),
                    OrganizationFacilityId = Convert.ToInt64(Request.Form["facility"]),
                    OrganizationDepartmentId = Convert.ToInt64(Request.Form["department"])
                };

                var res = _opsManager.UpdateExistingSIProject(existingSIProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("SIProjectInitiativeDetail", "Main", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(UpdateSIProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while updating the SI Project. {ex.Message}";
                return RedirectToAction("SIProjectInitiativeDetail", "Main", new { id = id });
            }
        }

        [HttpPost("UpdateSISubProject")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSISubProject()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }
            var id = Convert.ToInt64(Request.Form["id"]);
            try
            {
                var existingSISubProject = new SISubProject
                {
                    Id = id,
                    Initiative = Request.Form["title"],
                    SIId = Convert.ToInt64(Request.Form["siproject"]),
                    StartDate = Convert.ToDateTime(Request.Form["startdate"]).Date,
                    EndDate = Convert.ToDateTime(Request.Form["enddate"]).Date,
                    Description = Request.Form["description"],
                    FacilitatorId = Convert.ToInt64(Request.Form["facilitator"]),
                    Percentage = Convert.ToDecimal(Request.Form["percent"]),
                    Savings = Convert.ToDecimal(Request.Form["targetsavings"]),
                    Currency = Request.Form["currency"]
                };

                var res = _opsManager.UpdateExistingSISubProject(existingSISubProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                return RedirectToAction("SISubProjectDetail", "Main", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(UpdateSISubProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while updating the SI Sub Project. {ex.Message}";
                return RedirectToAction("SISubProjectDetail", "Main", new { id = id });
            }
        }


        private bool IsAuthenticated()
        {
            if (User.Identity.IsAuthenticated && UserHasValidRole())
            {
                return true;
            }
            return false;
        }

        private bool UserHasValidRole()
        {
            if (String.IsNullOrEmpty(HttpContext.Session.GetString("UserRole"))){
                return false;
            }
            return true;
        }
    }
}
