using Shared.DTO;
using Shared.Models;

namespace Datalayer.Interfaces
{
    public interface IOperationManager
    {
        Task<ResponseHandler<Country>> FetchOperationalCountry();
        Task<ResponseHandler<OrganizationCountry>> GetAllOrganizationCountries(int orgId);
        Task<ResponseHandler<OrganizationFacility>> GetAllOrganizationFacilities(int orgId);
        Task<ResponseHandler<OrganizationDepartment>> GetAllOrganizationDepartments(int orgId);
        Task<ResponseHandler<CIUser>> GetAllOrganizationUsers(int orgId);
        Task<ResponseHandler<OrganizationSoftSaving>> GetAllOrganizationSavingCategory(int orgId);
        Task<ResponseHandler<CIUser>> GetAllOperationalExcellenceUsers(int orgId);
        Task<ResponseHandler<CIUser>> GetAllStrategicInitiativeUsers(int orgId);
        Task<ResponseHandler> AddOrganizationCountry(OrganizationCountry orgCountry, string adminEmail);
        Task<ResponseHandler> RenameOrganizationCountry(long countryId, string countryName, string adminEmail);
        Task<ResponseHandler> DeleteOrganizationCountry(long countryId, string adminEmail, int orgId);
        Task<ResponseHandler> AddOrganizationFacility(OrganizationFacility orgFacility, string adminEmail);
        Task<ResponseHandler> RenameOrganizationFacility(long facilityId, string facilityName, string adminEmail);
        Task<ResponseHandler> DeleteOrganizationFacility(long facilityId, string adminEmail, int orgId);
        Task<ResponseHandler> AddOrganizationDepartment(OrganizationDepartment orgDepartment, string adminEmail);
        Task<ResponseHandler> RenameOrganizationDepartment(long departmentId, string departmentName, string adminEmail);
        Task<ResponseHandler> DeleteOrganizationDepartment(long departmentId, string adminEmail, int orgId);
        Task<ResponseHandler> AddOrganizationUser(CIUser orgUsr, string adminEmail);
        Task<ResponseHandler> RenameOrganizationUser(long userId, CIUser usr, string adminEmail);
        Task<ResponseHandler> DeleteOrganizationUser(long userId, string adminEmail, int orgId);
        Task<ResponseHandler> CreateNewOEProject(OperationalExcellence opExel, string adminEmail);
        Task<ResponseHandler<OperationalExcellenceDTO>> GetPaginatedOEProjects(int orgId, int pageNumber, int pageSize, InitiativeFilter filt);
        Task<ResponseHandler<OperationalExcellenceDTO>> GetOEProject(int orgId, long projectId);
        Task<ResponseHandler> UpdateExistingOEProject(OperationalExcellence opExel, string adminEmail);
        Task<ResponseHandler> CreateNewOEProjectMonthlySavings(OperationalExcellenceMonthlySaving opExel, string adminEmail);
        Task<ResponseHandler<OperationalExcellenceMonthlySavingDTO>> GetOEProjectMonthlySavings(long projectId);
        Task<ResponseHandler<OperationalExcellenceMonthlySavingDTO>> GetOEProjectMonthlySaving(long monthlySavingId);
        Task<ResponseHandler> UpdateOEProjectMonthlySavings(OperationalExcellenceMonthlySaving opExel, string adminEmail);
        Task<ResponseHandler> CreateNewSIProject(StrategicInitiative si, string adminEmail);
        Task<ResponseHandler> CreateNewSISubProject(SISubProject si, string adminEmail);
        Task<ResponseHandler<StrategicInitiativeDTO>> GetAllInProgressOrganizationSI(int orgId);
        Task<ResponseHandler<SISubProjectDTO>> GetSISubProjects(long projectId);
        Task<ResponseHandler<StrategicInitiativeDTO>> GetPaginatedSIProjects(int orgId, int pageNumber, int pageSize, InitiativeFilter filt);
        Task<ResponseHandler<StrategicInitiativeDTO>> GetSIProject(int orgId, long projectId);
        Task<ResponseHandler> UpdateExistingSIProject(StrategicInitiative si, string adminEmail);
        Task<ResponseHandler<SISubProjectDTO>> GetSISubProject(long id);
        Task<ResponseHandler> UpdateExistingSISubProject(SISubProject si, string adminEmail);
        Task<ResponseHandler<OperationalExcellenceDTO>> GetMiniOEProjects(int orgId);
        Task<ResponseHandler<StrategicInitiativeDTO>> GetMiniSIProjects(int orgId);
        Task<ResponseHandler<OrganizationToolDTO>> GetAllOrganizationTools(int orgId, string method);
        Task<ResponseHandler<MethodologyPhase>> GetMethodologyPhases(string method = null);
        Task<ResponseHandler<OrganizationToolDTO>> GetAllMethodologyTools(string method);
        Task<ResponseHandler> AddOrganizationSoftSaving(OrganizationSoftSaving orgSs, string adminEmail);
        Task<ResponseHandler> RenameOrganizationSoftSaving(long ssId, OrganizationSoftSaving usr, string adminEmail);
        Task<ResponseHandler> DeleteOrganizationSoftSaving(long ssId, string adminEmail, int orgId);
        Task<ResponseHandler<ContinuousImprovement>> CreateNewCIProject(ContinuousImprovement si, string adminEmail);
        Task<bool> CheckIfTenantHasSiteId(string tenantId);
        Task<ResponseHandler> CreateNewCIProjectTeam(List<CIProjectTeamMember> si, string adminEmail);
        Task<ResponseHandler> CreateNewCIProjectTool(List<CIProjectToolDTO> si, string adminEmail);
        Task<ResponseHandler<ContinuousImprovementDTO>> GetPaginatedCIProjects(int orgId, int pageNumber, int pageSize, InitiativeFilter filt);
        Task<ResponseHandler<CIProjectToolDTO>> GetAllProjectSelectedTools(long pid);
        Task<ResponseHandler> UpdateToolId(int toolId, string fileUrl);
        Task<ResponseHandler> CreateNewCIProjectComment(List<CIProjectComment> si, string adminEmail);
        Task<ResponseHandler> CreateNewCIProjectSaving(List<CIProjectSaving> si, ContinuousImprovementDTO ci, string adminEmail);
    }
}
