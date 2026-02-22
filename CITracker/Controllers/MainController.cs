using Datalayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared;
using Shared.DTO;
using Shared.Models;
using Shared.Request;
using Shared.ViewModels;
using System.Net;
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
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationSoftSaving = GetOrganizationSoftSavingCategory(),
                BusinessObjectiveAlignment = _opsManager.GetAllOrganizationBOA(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList()
            };

            return View(coep);
        }

        [HttpGet("GetOrganizationLocation")]
        public IActionResult GetOrganizationLocation()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = new OrganizationLocationVM
            {
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
            };

            return Ok(coep);
        }

        [HttpGet("SearchSupportingValues")]
        public async Task<IActionResult> SearchSupportingValues(string search)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(search) || search.Length < 3)
                return Ok(Enumerable.Empty<SupportingValueSearchResultDTO>());

            var results = await _opsManager.GetMiniOESIProjects(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), search);

            return Ok(results.Result.Select(x => new
            {
                id = $"{x.Source}|{x.Id}",
                text = x.Title,
                group = x.Source == "OE" ? "Operational Excellence" : "Strategic Initiative"
            }));
        }

        [HttpGet("GetSupportingValueById")]
        public async Task<IActionResult> GetSupportingValueById(string id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(id) || id.Length < 3)
                return Ok(new { });

            var parts = id.Split('|');
            var type = parts[0];
            var realId = long.Parse(parts[1]);

            var results = await _opsManager.GetMiniOESIProject(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), type, realId);

            if (results.SingleResult == null)
            {
                return Ok(new { });
            }

            return Ok(new
            {
                id = $"{results.SingleResult.Source}|{results.SingleResult.Id}",
                text = results.SingleResult.Title,
                group = results.SingleResult.Source == "OE" ? "Operational Excellence" : "Strategic Initiative"
            });
        }

        [HttpPost("NewCIProject")]
        [ValidateAntiForgeryToken]
        public IActionResult NewCIProject(CIRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var newCIProject = new ContinuousImprovement
                {
                    Id = model.Id,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    Title = model.Title,
                    StartDate = Convert.ToDateTime(model.StartDate).Date,
                    EndDate = Convert.ToDateTime(model.EndDate).Date,
                    Priority = model.Priority,
                    BusinessObjectiveAlignment = String.Join('|', model.BusinessObjectiveAlignment),
                    ProblemStatement = model.ProblemStatement,
                    Methodology = model.Methodology,
                    Certification = model.Certification,
                    TotalExpectedRevenue = model.TotalExpectedRevenue == null ? 0 : (decimal)model.TotalExpectedRevenue,
                    Currency = model.Currency,
                    Status = model.Status,
                    Phase = model.Phase,
                    CountryId = Convert.ToInt32(model.CountryId),
                    FacilityId = Convert.ToInt32(model.FacilityId),
                    DepartmentId = Convert.ToInt32(model.DepartmentId),
                    SupportingValueStream = model.SupportingValueStream,
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.CreateNewCIProject(newCIProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res.SingleResult);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res.SingleResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("NewCIProjectTeam")]
        [ValidateAntiForgeryToken]
        public IActionResult NewCIProjectTeam(CITeamRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var newCIProjectTeam = new List<CIProjectTeamMember>();

                foreach(var i in model.Team)
                {
                    newCIProjectTeam.Add(new CIProjectTeamMember
                    {
                        ProjectId = model.ProjectId,
                        Role = i.Role,
                        SendNotification = i.SendNotification,
                        UserId = i.UserId,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var res = _opsManager.CreateNewCIProjectTeam(newCIProjectTeam, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectTeam)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("NewCIProjectComment")]
        [ValidateAntiForgeryToken]
        public IActionResult NewCIProjectComment(CICommentRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                if (!model.Comment.Any())
                    return Ok();

                var newCIProjectComm = new List<CIProjectComment>();

                foreach (var i in model.Comment)
                {
                    newCIProjectComm.Add(new CIProjectComment
                    {
                        ProjectId = model.ProjectId,
                        Comment = i.Comment,
                        Date = i.Date,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var res = _opsManager.CreateNewCIProjectComment(newCIProjectComm, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectComment)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("NewCIProjectTool")]
        [ValidateAntiForgeryToken]
        public IActionResult NewCIProjectTool(CIToolRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var tools = model.Tool.Split("||").ToList();

                var newCIProjectTool = new List<CIProjectToolDTO>();

                foreach (var i in tools)
                {
                    newCIProjectTool.Add(new CIProjectToolDTO
                    {
                        ProjectId = model.ProjectId,
                        Methodology = model.Methodology,
                        Phase = i.Split('-')[0],
                        ToolId = Convert.ToInt32(i.Split('-')[1]),
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var res = _opsManager.CreateNewCIProjectTool(newCIProjectTool, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectTool)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("NewCIProjectFinancial")]
        [ValidateAntiForgeryToken]
        public IActionResult NewCIProjectFinancial(CIFinancialRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            ///TECHNICAL DEBT
            //if (!ModelState.IsValid)
            //{
            //    var errors = ModelState
            //        .Where(x => x.Value.Errors.Any())
            //        .ToDictionary(
            //            x => x.Key,
            //            x => x.Value.Errors.Select(e => e.ErrorMessage)
            //        );

            //    return BadRequest(new { errors });
            //}

            try
            {
                var CIProjectSaving = new List<CIProjectSaving>();

                foreach (var i in model.Hard)
                {
                    CIProjectSaving.Add(new CIProjectSaving
                    {
                        ProjectId = model.ProjectId,
                        Date = i.Date,
                        SavingClassification = "Hard",
                        SavingType = i.SavingType,
                        SavingValue = i.SavingValue,
                        IsCurrency = true,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                foreach (var i in model.Soft)
                {
                    CIProjectSaving.Add(new CIProjectSaving
                    {
                        ProjectId = model.ProjectId,
                        Category = i.Category,
                        SavingClassification = "Soft",
                        SavingUnit = i.SavingUnit,
                        SavingValue = i.SavingValue,
                        IsCurrency = false,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var ci = new ContinuousImprovementDTO
                {
                    Id = model.ProjectId,
                    IsOneTimeSavings = model.OneTimeSaving,
                    IsCarryOverSavings = model.CarryOverSaving,
                    FinancialVerificationDate = model.FinancialVerificationDate
                };

                var res = _opsManager.CreateNewCIProjectSaving(CIProjectSaving, ci, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectFinancial)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("CIProjectDetail")]
        public IActionResult CIProjectDetail(long id)
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
                OrganizationUser = _opsManager.GetAllOrganizationUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Project = _opsManager.GetCIProject(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), id)?.Result?.SingleResult,
                ProjectTeam = _opsManager.GetCIProjectTeam(id)?.Result?.SingleResult,
                ProjectTool = _opsManager.GetCIProjectTool(id)?.Result,
                ProjectComment = _opsManager.GetCIProjectComment(id).Result?.SingleResult,
                ProjectFinancial = _opsManager.GetCIProjectFinancial(id).Result?.SingleResult,
                OrganizationSoftSaving = GetOrganizationSoftSavingCategory()
            };

            return View(coep);
        }

        [HttpGet("AllCIProjects")]
        public IActionResult AllCIProjects(int page = 1, InitiativeFilter filt = null)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            return LoadPaginatedCI(page, filt);
        }

        private IActionResult LoadPaginatedCI(int page, InitiativeFilter filt)
        {
            var coep = new ContinuousImprovementVM
            {
                Message = TempData["Message"]?.ToString() ?? "",
                OrganizationCountry = _opsManager.GetAllOrganizationCountries(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationFacility = _opsManager.GetAllOrganizationFacilities(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationDepartment = _opsManager.GetAllOrganizationDepartments(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                OrganizationUser = _opsManager.GetAllOperationalExcellenceUsers(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList(),
                Projects = _opsManager.GetPaginatedCIProjects(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), page, _config.Value.PageSize, filt).Result
            };

            return View("AllCIProjects", coep);
        }

        [HttpPost("FilteredCIProjects")]
        [ValidateAntiForgeryToken]
        public IActionResult FilteredCIProjects(int page = 1)
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

                return LoadPaginatedCI(page, filt);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(FilteredCIProjects)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return LoadPaginatedCI(page, null);
            }
        }

        [HttpPost("UpdateCIProject")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCIProject(CIRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var updateCIProject = new ContinuousImprovement
                {
                    Id = model.Id,
                    OrganizationId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")),
                    Title = model.Title,
                    StartDate = Convert.ToDateTime(model.StartDate).Date,
                    EndDate = Convert.ToDateTime(model.EndDate).Date,
                    Priority = model.Priority,
                    BusinessObjectiveAlignment = String.Join('|', model.BusinessObjectiveAlignment),
                    ProblemStatement = model.ProblemStatement,
                    Methodology = model.Methodology,
                    Certification = model.Certification,
                    TotalExpectedRevenue = model.TotalExpectedRevenue == null ? 0 : (decimal)model.TotalExpectedRevenue,
                    Currency = model.Currency,
                    Status = model.Status,
                    Phase = model.Phase,
                    CountryId = Convert.ToInt32(model.CountryId),
                    FacilityId = Convert.ToInt32(model.FacilityId),
                    DepartmentId = Convert.ToInt32(model.DepartmentId),
                    SupportingValueStream = model.SupportingValueStream,
                    CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                    DateCreated = DateTime.UtcNow
                };

                var res = _opsManager.CreateNewCIProject(updateCIProject, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res.SingleResult);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res.SingleResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(UpdateCIProject)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while modifying the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateCIProjectTeam")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCIProjectTeam(CITeamRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var newCIProjectTeam = new List<CIProjectTeamMember>();

                foreach (var i in model.Team)
                {
                    newCIProjectTeam.Add(new CIProjectTeamMember
                    {
                        Id = i.Id == null ? 0 : (long)i.Id,
                        ProjectId = model.ProjectId,
                        Role = i.Role,
                        SendNotification = i.SendNotification,
                        UserId = i.UserId,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var res = _opsManager.CreateNewCIProjectTeam(newCIProjectTeam, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectTeam)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateCIProjectTool")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCIProjectTool(CIToolRequest model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var tools = model.Tool.Split("||").ToList();

                var updCIProjectTool = new List<CIProjectToolDTO>();

                foreach (var i in tools)
                {
                    updCIProjectTool.Add(new CIProjectToolDTO
                    {
                        Id = Convert.ToInt64(i.Split('-')[2]),
                        ProjectId = model.ProjectId,
                        Methodology = model.Methodology,
                        Phase = i.Split('-')[0],
                        ToolId = Convert.ToInt32(i.Split('-')[1]),
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var res = _opsManager.UpdateCIProjectTool(updCIProjectTool, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;
                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(UpdateCIProjectTool)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while updating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateCIProjectComment")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCIProjectComment(CICommentDTO model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                if (model.Comment == null)
                    return Ok();

                if (!model.Comment.Any())
                    return Ok();

                var updCIProjectComm = new List<CIProjectComment>();

                foreach (var i in model.Comment)
                {
                    updCIProjectComm.Add(new CIProjectComment
                    {
                        Id = i.Id,
                        ProjectId = model.ProjectId,
                        Comment = i.Comment,
                        Date = i.Date,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var res = _opsManager.UpdateCIProjectComment(updCIProjectComm, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectComment)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("UpdateCIProjectFinancial")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCIProjectFinancial(CIFinancialDTO model)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            ///TECHNICAL DEBT
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value.Errors.Select(e => e.ErrorMessage)
                    );

                return BadRequest(new { errors });
            }

            try
            {
                var CIProjectSaving = new List<CIProjectSaving>();

                foreach (var i in model.Hard)
                {
                    CIProjectSaving.Add(new CIProjectSaving
                    {
                        Id = (long)i.Id,
                        ProjectId = model.ProjectId,
                        Date = i.Date,
                        SavingClassification = "Hard",
                        SavingType = i.SavingType,
                        SavingValue = i.SavingValue,
                        IsCurrency = true,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                foreach (var i in model.Soft)
                {
                    CIProjectSaving.Add(new CIProjectSaving
                    {
                        Id = (long)i.Id,
                        ProjectId = model.ProjectId,
                        Category = i.Category,
                        SavingClassification = "Soft",
                        SavingUnit = i.SavingUnit,
                        SavingValue = i.SavingValue,
                        IsCurrency = false,
                        CreatedBy = Convert.ToInt64(HttpContext.Session.GetString("UserId")),
                        DateCreated = DateTime.UtcNow
                    });
                }

                var ci = new ContinuousImprovementDTO
                {
                    Id = model.ProjectId,
                    IsOneTimeSavings = model.OneTimeSaving,
                    IsCarryOverSavings = model.CarryOverSaving,
                    FinancialVerificationDate = (DateTime) model.FinancialVerificationDate,
                    FinancialReportComment = model.FinancialReportComment,
                    IsAudited = model.IsAudited,
                    AuditedBy = (long) model.Auditor,
                    AuditedDate = model.AuditedDate
                };

                var res = _opsManager.UpdateCIProjectSaving(CIProjectSaving, ci, HttpContext.Session.GetString("UserEmail")).Result;

                TempData["Message"] = res.Message;

                if (res.StatusCode == (int)HttpStatusCode.OK)
                    return Ok(res);
                else
                    return StatusCode(StatusCodes.Status417ExpectationFailed, res);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error Occurred at {nameof(NewCIProjectFinancial)} - {JsonConvert.SerializeObject(ex)}");
                TempData["Message"] = $"An error occurred while creating the CI Project. {ex.Message}";
                return StatusCode(500, ex.Message);
            }
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
        public IActionResult GetMethodologyTools(string val, long pid)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var coep = _opsManager.GetAllOrganizationTools(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")), val, pid)?.Result?.Result?.ToList();

            var gcoep = coep.GroupBy(t => t.Phase).ToList();

            var result = gcoep.ToDictionary(
                g => g.Key,
                g => g.Select(x => new
                {
                    orgToolId = x.Id,
                    projectToolId = x.ProjectToolId,
                    id = x.ToolId,
                    name = x.Tool,
                    url = x.Url,
                    isChecked = x.IsChecked
                }).ToList()
            );

            return Json(result);
        }

        [HttpGet("GetSelectedTools")]
        public IActionResult GetSelectedTools(long val)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if(val <= 0)
            {
                return NotFound("Incorrect Parameter");
            }

            var coep = _opsManager.GetAllProjectSelectedTools(val)?.Result?.Result?.ToList();

            if (coep.Any())
            {
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

            return StatusCode(StatusCodes.Status417ExpectationFailed);
        }

        [HttpPost("UploadToolFile")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadToolFile(IFormFile file, int toolId, string toolName)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            // Whitelist allowed extensions
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type.");

            // Generate safe file name (never trust client filename)
            var safeFileName = $"{toolName.Replace(" ", "").ToLower()}{extension}";

            // Target directory (wwwroot/uploads/tools)
            var uploadRoot = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{HttpContext.Session.GetString("OrganizationId")}",
                "tools"
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
            //var fileName = $"/uploads/Org-{HttpContext.Session.GetString("OrganizationId")}/tools/{safeFileName}";
            var fileName = $"/tools/{safeFileName}";
            var res = _opsManager.UpdateToolId(toolId, fileName);

            return Ok(new
            {
                toolId,
                fileName
            });
        }

        [HttpGet("DownloadProjectToolDoc")]
        public IActionResult DownloadProjectToolDoc(long toolId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var orgId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"));

            var fileName = _opsManager.GetProjectToolFileName(toolId).Result?.SingleResult?.Url;

            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            fileName = fileName.TrimStart('/', '\\');

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{orgId}",
                fileName
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpGet("DownloadReportToolDoc")]
        public IActionResult DownloadReportToolDoc(long projectId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var orgId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"));

            var fileName = _opsManager.GetCIProjectMini(orgId, projectId).Result?.SingleResult?.FinalReportUrl;

            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            fileName = fileName.TrimStart('/', '\\');

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{orgId}",
                fileName
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpGet("DownloadFinReportDoc")]
        public IActionResult DownloadFinReportDoc(long projectId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var orgId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"));

            var fileName = _opsManager.GetCIProjectMini(orgId, projectId).Result?.SingleResult?.FinancialReportUrl;

            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            fileName = fileName.TrimStart('/', '\\');

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{orgId}",
                fileName
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpGet("DownloadToolDoc")]
        public IActionResult DownloadToolDoc(long toolId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var orgId = Convert.ToInt32(HttpContext.Session.GetString("OrganizationId"));

            var fileName = _opsManager.GetToolFileName(toolId, orgId).Result?.SingleResult?.Url;

            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            fileName = fileName.TrimStart('/', '\\');

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{orgId}",
                fileName
            );

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var contentType = "application/octet-stream";

            return PhysicalFile(filePath, contentType, fileName);
        }

        [HttpPost("UploadFinalReportFile")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFinalReportFile(IFormFile file, int projectId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            // Whitelist allowed extensions
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".jpg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type.");

            // Generate safe file name (never trust client filename)
            var safeFileName = $"{DateTime.Now.ToString("yyMMddhhmmss")}{extension}";

            // Target directory
            var uploadRoot = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{HttpContext.Session.GetString("OrganizationId")}",
                "report"
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
            //var fileName = $"/uploads/Org-{HttpContext.Session.GetString("OrganizationId")}/report/{safeFileName}";
            var fileName = $"/report/{safeFileName}";
            var res = _opsManager.UpdateReportFile(projectId, fileName);

            return Ok(new
            {
                projectId
            });
        }

        [HttpPost("UploadFinancialReportFile")]
        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFinancialReportFile(IFormFile file, int projectId)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            // Whitelist allowed extensions
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".png", ".ppt", ".jpg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type.");

            // Generate safe file name (never trust client filename)
            var safeFileName = $"{DateTime.Now.ToString("yyMMddhhmmss")}{extension}";

            // Target directory
            var uploadRoot = Path.Combine(
                Directory.GetCurrentDirectory(),
                "SecureUploads",
                "uploads",
                $"Org-{HttpContext.Session.GetString("OrganizationId")}",
                "financialreport"
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
            //var fileName = $"/uploads/Org-{HttpContext.Session.GetString("OrganizationId")}/financialreport/{safeFileName}";
            var fileName = $"/financialreport/{safeFileName}";
            var res = _opsManager.UpdateFinancialReportFile(projectId, fileName);

            return Ok(new
            {
                projectId,
                fileName
            });
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

        [HttpGet("ProjectCountByMethodology")]
        public IActionResult MethodologyCount()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { name = "DMAIC", value = 1048 },
                new { name = "Gemba Kaizen", value = 735 },
                new { name = "Project", value = 580 },
                new { name = "JDI", value = 484 },
                new { name = "Others", value = 300 }
            };

            return Ok(data);
        }

        [HttpGet("ProjectStatusCount")]
        public IActionResult ProjectStatusCount()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { name = "PROPOSED", value = 1048 },
                new { name = "INITIATED", value = 735 },
                new { name = "COMPLETED", value = 580 },
                new { name = "CLOSED", value = 484 },
                new { name = "CANCELLED", value = 300 }
            };

            return Ok(data);
        }

        [HttpGet("MethodologiesbyMonth")]
        public IActionResult MethodologiesbyMonth()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { month = "January", completed = 12, ongoing = 7, onHold = 3 },
                new { month = "February", completed = 9, ongoing = 10, onHold = 2 },
                new { month = "March", completed = 15, ongoing = 5, onHold = 4 },
                new { month = "April", completed = 11, ongoing = 8, onHold = 6 },
                new { month = "May", completed = 18, ongoing = 6, onHold = 1 },
                new { month = "June", completed = 14, ongoing = 9, onHold = 2 }
            };

            return Ok(data);
        }

        [HttpGet("ProjectCertificationCount")]
        public IActionResult ProjectCertificationCount()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { label = "Black Belt", value = 11 },
                new { label = "Green Belt", value = 16 },
                new { label = "PMP", value = 7 },
                new { label = "Not Applicable", value = 3 }
            };

            return Ok(data);
        }

        [HttpGet("SavingsByCategory")]
        public IActionResult SavingsByCategory()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { name = "Revenue", value = 1048000 },
                new { name = "Cost Avoidance", value = 735000 },
                new { name = "Cost Reduction", value = 580000 },
                new { name = "Cost Contianment", value = 484000 }
            };

            return Ok(data);
        }

        [HttpGet("MonthlySavings")]
        public IActionResult MonthlySavings()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { month = "January", CostAvoidance = 12, Revenue = 7, CostReduction = 3 },
                new { month = "February", CostAvoidance = 9, Revenue = 10, CostReduction = 2 },
                new { month = "March", CostAvoidance = 15, Revenue = 5, CostReduction = 4 },
                new { month = "April", CostAvoidance = 11, Revenue = 8, CostReduction = 6 },
                new { month = "May", CostAvoidance = 18, Revenue = 6, CostReduction = 1 },
                new { month = "June", CostAvoidance = 14, Revenue = 9, CostReduction = 2 }
            };

            return Ok(data);
        }

        [HttpGet("CompletedProjectsByUser")]
        public IActionResult CompletedProjectsByUser()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { name = "Alice", completed = 12 },
                new { name = "John", completed = 18 },
                new { name = "Michael", completed = 9 },
                new { name = "Sarah", completed = 15 }
            };

            return Ok(data);
        }

        [HttpGet("MonthlyProjectsByMethodologies")]
        public IActionResult MonthlyProjectsByMethodologies()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            if (!UserHasValidRole())
            {
                return RedirectToAction("Index", "Home");
            }

            var data = new[]
            {
                new { month = "January", dmaic = 12, gemba = 7, project = 3, jdi = 6, others = 2 },
                new { month = "February", dmaic = 9, gemba = 10, project = 2, jdi = 6, others = 2 },
                new { month = "March", dmaic = 15, gemba = 5, project = 4, jdi = 6, others = 2 },
                new { month = "April", dmaic = 11, gemba = 8, project = 6, jdi = 6, others = 2 },
                new { month = "May", dmaic = 18, gemba = 6, project = 1, jdi = 6, others = 2 },
                new { month = "June", dmaic = 14, gemba = 9, project = 2, jdi = 6, others = 2 }
            };

            return Ok(data);
        }

        [HttpGet("MonthlyProjectsByDepartment")]
        public IActionResult MonthlyProjectsByDepartment()
        {
            var data = new
            {
                labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                datasets = new[]
                {
                    new {
                        department = "IT",
                        data = new[] { 5, 7, 3, 8, 6, 9 }
                    },
                    new {
                        department = "HR",
                        data = new[] { 2, 4, 6, 5, 3, 4 }
                    },
                    new {
                        department = "Finance",
                        data = new[] { 3, 5, 4, 7, 6, 8 }
                    }
                }
            };

            return Json(data);
        }

        [HttpGet("MonthlyProjectsByPhase")]
        public IActionResult MonthlyProjectsByPhase()
        {
            var data = new
            {
                labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                datasets = new[]
                {
                    new {
                        phase = "Define",
                        data = new[] { 3, 5, 4, 6, 7, 8 }
                    },
                    new {
                        phase = "Measure",
                        data = new[] { 2, 3, 5, 4, 6, 7 }
                    },
                    new {
                        phase = "Analyze",
                        data = new[] { 1, 2, 3, 2, 4, 5 }
                    },
                    new {
                        phase = "Improve",
                        data = new[] { 4, 6, 5, 7, 8, 9 }
                    },
                    new {
                        phase = "Control",
                        data = new[] { 2, 1, 2, 3, 4, 3 }
                    }
                }
            };

            return Json(data);
        }



        private Dictionary<string, string> GetOrganizationSoftSavingCategory()
        {
            var defaultMap = new Dictionary<string, string>
                {
                    { "Space", "sqft" },
                    { "Time", "hr" },
                    { "Quality", "#" },
                    { "Safety", "TRIR" },
                    { "Defect Rate", "#" }
                };

            var org = _opsManager.GetOrganizationSoftSaving(Convert.ToInt32(HttpContext.Session.GetString("OrganizationId")))?.Result?.Result?.ToList().ToDictionary(x => x.Category, x => x.Unit);

            return org ?? defaultMap;
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
