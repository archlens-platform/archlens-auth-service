namespace ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;

public sealed record AuthResponse(
    string Token,
    int ExpiresInMinutes,
    string Username,
    string Role);
