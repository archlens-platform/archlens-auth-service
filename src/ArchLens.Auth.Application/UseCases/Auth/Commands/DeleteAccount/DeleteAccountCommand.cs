using ArchLens.SharedKernel.Application;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(Guid UserId) : IRequest<Result<bool>>;
