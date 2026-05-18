using ArchLens.Auth.Application.UseCases.Auth.Commands.Login;
using FluentValidation;

namespace ArchLens.Auth.Application.UseCases.Auth.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
