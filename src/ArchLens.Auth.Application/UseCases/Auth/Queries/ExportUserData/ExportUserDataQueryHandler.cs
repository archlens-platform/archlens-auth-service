using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.Auth.Domain.Interfaces.UserInterfaces;
using ArchLens.SharedKernel.Application;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Queries.ExportUserData;

public sealed class ExportUserDataQueryHandler(IUserRepository userRepository)
    : IRequestHandler<ExportUserDataQuery, Result<UserDataExportResponse>>
{
    public async Task<Result<UserDataExportResponse>> Handle(ExportUserDataQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure<UserDataExportResponse>(new Error("Auth.UserNotFound", "User not found"));

        return new UserDataExportResponse(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString(),
            user.CreatedAt,
            user.LastLoginAt,
            user.LgpdConsentGivenAt);
    }
}
