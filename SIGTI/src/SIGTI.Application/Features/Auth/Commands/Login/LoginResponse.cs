namespace SIGTI.Application.Features.Auth.Commands.Login
{
    public sealed record LoginResponse(
        string Token,
        DateTime ExpiresAt,
        Guid UserId,
        string Name,
        string Email,
        string Role
    );
}
