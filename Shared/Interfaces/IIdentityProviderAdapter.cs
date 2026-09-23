using Microsoft.AspNetCore.Http;
using Shared.Enumerations;

namespace Shared.Interfaces
{
    public interface IIdentityProviderAdapter
    {
        IdentityProviderKind Kind { get; }
        Task<ExternalIdentity> ValidateAsync(HttpContext ctx);
    }

    public record ExternalIdentity(
        string ExternalId,
        string Email,
        string? TenantHint,
        IdentityProviderKind Provider);
}
