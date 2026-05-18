using ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;
using ArchLens.SharedKernel.Application;
using MediatR;

namespace ArchLens.Auth.Application.UseCases.Auth.Commands.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<Result<AuthResponse>>;
