using ArchLens.Auth.Application.Contracts.Auth;
using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using ArchLens.SharedKernel.Application;
using MediatR;
using Microsoft.Extensions.Options;

namespace ArchLens.Auth.Application.UseCases.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtGenerator,
    IOptions<JwtOptions> jwtOptions) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username.ToLowerInvariant(), cancellationToken);

        if (user is null)
            return Result.Failure<AuthResponse>(new Error("Auth.InvalidCredentials", "Invalid username or password"));

        if (!user.IsActive)
            return Result.Failure<AuthResponse>(new Error("Auth.AccountDisabled", "Account is disabled"));

        if (user.IsLockedOut())
            return Result.Failure<AuthResponse>(new Error("Auth.AccountLocked", "Account is temporarily locked. Try again later."));

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin();
            await userRepository.UpdateAsync(user, cancellationToken);
            return Result.Failure<AuthResponse>(new Error("Auth.InvalidCredentials", "Invalid username or password"));
        }

        user.RegisterSuccessfulLogin();
        await userRepository.UpdateAsync(user, cancellationToken);

        var token = jwtGenerator.Generate(user.Id.ToString(), user.Username, user.Role.ToString());

        return new AuthResponse(token, jwtOptions.Value.ExpirationMinutes, user.Username, user.Role.ToString());
    }
}
