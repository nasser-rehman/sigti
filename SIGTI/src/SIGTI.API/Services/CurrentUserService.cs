using System.Security.Claims;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Domain.Enums;

namespace SIGTI.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var idClaim =
                User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var guid) ? guid : null;
        }
    }

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public Role? Role
    {
        get
        {
            var roleClaim =
                User?.FindFirst(ClaimTypes.Role)?.Value
                ?? User?.FindFirst("role")?.Value;

            return Enum.TryParse<Role>(roleClaim, true, out var role)
                ? role
                : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(Role role) => User?.IsInRole(role.ToString()) ?? false;
}
