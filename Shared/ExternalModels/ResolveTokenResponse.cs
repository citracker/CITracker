using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalModels
{
    public class Beneficiary
    {
        public string EmailId { get; set; }
        public string ObjectId { get; set; }
        public string TenantId { get; set; }
        public string Puid { get; set; }
    }

    public class Purchaser
    {
        public string EmailId { get; set; }
        public string ObjectId { get; set; }
        public string TenantId { get; set; }
        public string Puid { get; set; }
    }

    public class ResolveTokenResponse
    {
        public string Id { get; set; }
        public string SubscriptionName { get; set; }
        public string OfferId { get; set; }
        public string PlanId { get; set; }
        public int Quantity { get; set; }
        public Subscription Subscription { get; set; }
    }

    public class Subscription
    {
        public string Id { get; set; }
        public string PublisherId { get; set; }
        public string OfferId { get; set; }
        public string Name { get; set; }
        public string SaasSubscriptionStatus { get; set; }
        public Beneficiary Beneficiary { get; set; }
        public Purchaser Purchaser { get; set; }
        public string PlanId { get; set; }
        public Term Term { get; set; }
        public bool AutoRenew { get; set; }
        public bool IsTest { get; set; }
        public bool IsFreeTrial { get; set; }
        public List<string> AllowedCustomerOperations { get; set; }
        public string SandboxType { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public int Quantity { get; set; }
        public string SessionMode { get; set; }
    }

    public class Term
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TermUnit { get; set; }
    }
}
