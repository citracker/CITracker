
namespace Shared.DTO
{
    public sealed record UserIdentityContext(
        string Provider,
        string Email,
        string ExternalId,                 // per-user: Microsoft oid | Google sub
        string OrganizationKey,            // per-org:  Microsoft tid | Google hd | custom domain | null
        bool IsConsumerPersonalAccount,
        string FirstName,
        string LastName)
    {
        public bool HasOrganization => !string.IsNullOrEmpty(OrganizationKey);
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }
}
