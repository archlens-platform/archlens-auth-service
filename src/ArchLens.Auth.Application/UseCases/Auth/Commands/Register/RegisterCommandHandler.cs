using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.Auth.Domain.Entities.UserEntities;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;
using ArchLens.SharedKernel.Application;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsByUsernameOrEmailAsync(
            request.Username.ToLowerInvariant(),
            request.Email.ToLowerInvariant(),
            cancellationToken);

        if (exists)
            return Result.Failure<RegisterResponse>(new Error("Auth.UserExists", "Username or email already taken"));

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Username, request.Email, hash, lgpdConsentGivenAt: DateTime.UtcNow);

        await userRepository.AddAsync(user, cancellationToken);

        return new RegisterResponse(user.Id, user.Username, user.Email);
    }
}
