using FluentAssertions;
using SIGTI.Application.Features.Auth.Commands.Login;

namespace SIGTI.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
    {
        var command = new LoginCommand("valido@sigti.local", "SenhaForte123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenEmailIsEmpty_ShouldHaveValidationError(
        string? email
    )
    {
        var command = new LoginCommand(email!, "Senha123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@dominio.com")]
    public void Validate_WhenEmailFormatIsInvalid_ShouldHaveValidationError(
        string email
    )
    {
        var command = new LoginCommand(email, "Senha123");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e => e.PropertyName == nameof(LoginCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveValidationError(
        string? password
    )
    {
        var command = new LoginCommand("valido@sigti.local", password!);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }
}
