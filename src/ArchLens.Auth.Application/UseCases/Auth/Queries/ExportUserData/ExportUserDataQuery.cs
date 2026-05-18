using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.SharedKernel.Application;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Queries.ExportUserData;

public sealed record ExportUserDataQuery(Guid UserId) : IRequest<Result<UserDataExportResponse>>;
