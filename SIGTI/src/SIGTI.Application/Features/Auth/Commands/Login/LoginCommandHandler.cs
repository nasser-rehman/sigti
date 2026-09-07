using MediatR;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Domain.Entities;
using SIGTI.Domain.ValueObjects;

namespace SIGTI.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler
        : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator
        )
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponse> Handle(
            LoginCommand request,
            CancellationToken cancellationToken
        )
        {
            var email = new Email(request.Email);
            var user = await _userRepository.GetByEmailAsync(
                email,
                cancellationToken
            );

            if (user is null || !user.IsActive)
                throw new UnauthorizedException(
                    "Credenciais inválidas ou usuário inativo."
                );

            var isPasswordValid = _passwordHasher.Verify(
                request.Password,
                user.PasswordHash
            );
            if (!isPasswordValid)
                throw new UnauthorizedException(
                    "Credenciais inválidas ou usuário inativo."
                );

            var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponse(
                token,
                expiresAt,
                user.Id,
                user.Name,
                user.Email.Value,
                user.Role.ToString()
            );
        }
    }
}
