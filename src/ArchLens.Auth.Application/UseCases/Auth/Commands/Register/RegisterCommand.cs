using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.SharedKernel.Application;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Commands.Register;

public sealed record RegisterCommand(string Username, string Email, string Password, bool LgpdConsent) : IRequest<Result<RegisterResponse>>;
