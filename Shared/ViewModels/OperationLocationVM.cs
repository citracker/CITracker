using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ViewModels
{
    public class OperationLocationVM
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<OrganizationCountry> Country { get; set; }
        public List<OrganizationFacility> Facility { get; set; }
        public List<OrganizationDepartment> Department { get; set; }
    }
}
