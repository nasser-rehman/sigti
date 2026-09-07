using SIGTI.Domain.Enums;
using SIGTI.Domain.ValueObjects;

namespace SIGTI.Application.Common.Interfaces.Services
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        Role? Role { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(Role role);
    }
}
