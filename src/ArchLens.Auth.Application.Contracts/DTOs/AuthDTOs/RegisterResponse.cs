namespace ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;

public sealed record RegisterResponse(
    Guid UserId,
    string Username,
    string Email);
