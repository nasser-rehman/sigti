using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Auth.Commands.Login;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Tests.Builders;
using SIGTI.Domain.ValueObjects;

namespace SIGTI.Application.Tests.Features.Auth.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

            _handler = new LoginCommandHandler(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenGeneratorMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenCredentialsAreValid_ShouldReturnTokenAndUserDetails()
        {
            // Arrange
            var user = new UserBuilder()
                .WithEmail("tech@sigti.local")
                .WithRole(Role.Technician)
                .Build();

            var command = new LoginCommand("tech@sigti.local", "Senha@123");
            var expectedExpiration = DateTime.UtcNow.AddMinutes(60);

            _userRepositoryMock
                .Setup(s =>
                    s.GetByEmailAsync(
                        It.Is<Email>(e => e.Value == command.Email),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(s => s.Verify(command.Password, user.PasswordHash))
                .Returns(true);

            _jwtTokenGeneratorMock
                .Setup(g => g.GenerateToken(user))
                .Returns(("jwt-token-valido", expectedExpiration));

            // Act
            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.Token.Should().Be("jwt-token-valido");
            response.ExpiresAt.Should().Be(expectedExpiration);
            response.UserId.Should().Be(user.Id);
            response.Name.Should().Be(user.Name);
            response.Email.Should().Be(user.Email.Value);
            response.Role.Should().Be(Role.Technician.ToString());
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedException()
        {
            // Arrange

            var command = new LoginCommand("inex@sigti.local", "anonymous");

            _userRepositoryMock
                .Setup(s =>
                    s.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync((User?)null);

            // act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Credenciais inválidas ou usuário inativo.");

            _passwordHasherMock.Verify(
                h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );

            _jwtTokenGeneratorMock.Verify(
                g => g.GenerateToken(It.IsAny<User>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_WhenPasswordIsInvalid_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var user = new UserBuilder().WithEmail("user@sigti.local").Build();

            var command = new LoginCommand(
                "user@sigti.local",
                "SenhaAleatória"
            );

            _userRepositoryMock
                .Setup(s =>
                    s.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(h => h.Verify(command.Password, user.PasswordHash))
                .Returns(false);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<UnauthorizedException>()
                .WithMessage("Credenciais inválidas ou usuário inativo.");

            _jwtTokenGeneratorMock.Verify(
                g => g.GenerateToken(It.IsAny<User>()),
                Times.Never
            );
        }
    }
}
