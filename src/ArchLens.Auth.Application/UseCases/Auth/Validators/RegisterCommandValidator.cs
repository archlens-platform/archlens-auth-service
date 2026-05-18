using ArchLens.Auth.Application.UseCases.Auth.Commands.Register;
using FluentValidation;

namespace ArchLens.Auth.Application.UseCases.Auth.Validators;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
        RuleFor(x => x.LgpdConsent).Equal(true)
            .WithMessage("Consentimento LGPD é obrigatório para criar uma conta");
    }
}
