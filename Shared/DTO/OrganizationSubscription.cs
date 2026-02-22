using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class OrganizationSubscription
    {
        public long OrganizationId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfLicences { get; set; }
        public int NumberOfUsedLicences { get; set; }
    }
}
