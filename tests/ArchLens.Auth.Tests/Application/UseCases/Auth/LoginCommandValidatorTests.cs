using ArchLens.Auth.Application.UseCases.Auth.Commands.Login;
using ArchLens.Auth.Application.UseCases.Auth.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ArchLens.Auth.Tests.Application.UseCases.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new LoginCommand("john_doe", "Secret123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyUsername_ShouldFail()
    {
        var command = new LoginCommand("", "Secret123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameTooLong_ShouldFail()
    {
        var command = new LoginCommand(new string('a', 51), "Secret123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameMaxLength_ShouldPass()
    {
        var command = new LoginCommand(new string('a', 50), "Secret123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        var command = new LoginCommand("john_doe", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordTooShort_ShouldFail()
    {
        var command = new LoginCommand("john_doe", "abc12");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordExactMinLength_ShouldPass()
    {
        var command = new LoginCommand("john_doe", "abc123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_BothFieldsEmpty_ShouldHaveTwoErrors()
    {
        var command = new LoginCommand("", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
