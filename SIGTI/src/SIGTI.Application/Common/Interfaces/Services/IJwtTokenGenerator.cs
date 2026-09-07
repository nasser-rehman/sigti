using SIGTI.Domain.Entities;

namespace SIGTI.Application.Common.Interfaces.Services
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
