using Shared.DTO;
using Shared.Models;

namespace Shared.ViewModels
{
    public class StrategicInitiativeVM
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<OrganizationCountry> OrganizationCountry { get; set; }
        public List<OrganizationFacility> OrganizationFacility { get; set; }
        public List<OrganizationDepartment> OrganizationDepartment { get; set; }
        public List<CIUser> OrganizationUser { get; set; }
        public ResponseHandler<StrategicInitiativeDTO> Initiative { get; set; }
        public List<SISubProjectDTO> SubProject { get; set; }
        public SISubProjectDTO SISubProject { get; set; }
    }
}
