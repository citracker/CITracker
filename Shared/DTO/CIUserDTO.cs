using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class CIUserDTO
    {
        public long Id { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationTenantId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsOrganizationSubscribed { get; set; }
        public int SubscriptionId { get; set; }
    }
}
