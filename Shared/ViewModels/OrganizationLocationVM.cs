using Shared.DTO;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ViewModels
{
    public class OrganizationLocationVM
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<OrganizationCountry> OrganizationCountry { get; set; }
        public List<OrganizationFacility> OrganizationFacility { get; set; }
        public List<OrganizationDepartment> OrganizationDepartment { get; set; }
    }
}
