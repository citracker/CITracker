using DocumentFormat.OpenXml.Spreadsheet;
using Shared.DTO;
using Shared.Models;

namespace Shared.ViewModels
{
    public class ContinuousImprovementVM 
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<OrganizationCountry> OrganizationCountry { get; set; }
        public List<OrganizationFacility> OrganizationFacility { get; set; }
        public List<OrganizationDepartment> OrganizationDepartment { get; set; }
        public List<OperationalExcellenceDTO> OperationalExcellence { get; set; }
        public List<StrategicInitiativeDTO> StrategicInitiative { get; set; }
        public List<CIUser> OrganizationUser { get; set; }
        public List<CIUser> OrganizationRoles { get; set; }
        public List<OrganizationToolDTO> MethodologyTool { get; set; }
        public ResponseHandler<ContinuousImprovementDTO> Projects { get; set; }
        public ContinuousImprovement Project { get; set; }

    }
}
