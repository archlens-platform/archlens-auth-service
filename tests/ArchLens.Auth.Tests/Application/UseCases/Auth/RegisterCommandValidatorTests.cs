using ArchLens.Auth.Application.UseCases.Auth.Commands.Register;
using ArchLens.Auth.Application.UseCases.Auth.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ArchLens.Auth.Tests.Application.UseCases.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("john_doe", "john@example.com", "Secret123", true);

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    // Username rules

    [Fact]
    public void Validate_EmptyUsername_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = "" });

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameTooShort_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = "ab" });

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameExactMinLength_ShouldPass()
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = "abc" });

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameTooLong_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = new string('a', 51) });

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_UsernameWithSpecialChars_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = "john-doe!" });

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("User_Name")]
    [InlineData("ALLCAPS123")]
    [InlineData("with_underscore")]
    public void Validate_UsernameValidPatterns_ShouldPass(string username)
    {
        var result = _validator.TestValidate(ValidCommand() with { Username = username });

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    // Email rules

    [Fact]
    public void Validate_EmptyEmail_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Email = "" });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidEmail_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Email = "notanemail" });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // Password rules

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = "" });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordTooShort_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = "Ab1" });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordWithoutUppercase_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = "secret123" });

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Fact]
    public void Validate_PasswordWithoutLowercase_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = "SECRET123" });

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Fact]
    public void Validate_PasswordWithoutDigit_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { Password = "SecretPass" });

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit");
    }

    // LgpdConsent rules

    [Fact]
    public void Validate_LgpdConsentFalse_ShouldFail()
    {
        var result = _validator.TestValidate(ValidCommand() with { LgpdConsent = false });

        result.ShouldHaveValidationErrorFor(x => x.LgpdConsent)
            .WithErrorMessage("Consentimento LGPD é obrigatório para criar uma conta");
    }

    // Multiple errors

    [Fact]
    public void Validate_MultipleInvalidFields_ShouldReturnMultipleErrors()
    {
        var command = new RegisterCommand("a", "bademail", "weak", false);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.LgpdConsent);
    }
}
