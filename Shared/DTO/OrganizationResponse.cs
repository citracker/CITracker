using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class OrganizationResponse
    {
        public List<Organisation> value { get; set; }
    }

    public class Organisation
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public List<VerifiedDomain> verifiedDomains { get; set; }
    }

    public class VerifiedDomain
    {
        public string name { get; set; }
        public bool isDefault { get; set; }
    }
}
