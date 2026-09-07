using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Auth.Commands.Login;

namespace SIGTI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        ISender sender,
        ICurrentUserService currentUserService
    )
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken
    )
    {
        var response = await _sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(
            new
            {
                UserId = _currentUserService.UserId,
                Email = _currentUserService.Email,
                Role = _currentUserService.Role?.ToString(),
                IsAuthenticated = _currentUserService.IsAuthenticated,
            }
        );
    }
}
