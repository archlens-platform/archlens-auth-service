using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.Contracts.Events;
using ArchLens.SharedKernel.Application;
using MassTransit;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Commands.DeleteAccount;

public sealed class DeleteAccountCommandHandler(
    IUserRepository userRepository,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<bool>(new Error("Auth.UserNotFound", "User not found"));

        await userRepository.DeleteAsync(user, cancellationToken);

        await publishEndpoint.Publish(
            new UserAccountDeletedEvent { UserId = request.UserId, Timestamp = DateTime.UtcNow },
            cancellationToken);

        return true;
    }
}
