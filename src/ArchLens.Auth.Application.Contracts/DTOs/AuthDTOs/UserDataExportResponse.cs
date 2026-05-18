namespace ArchLens.Auth.Application.Contracts.DTOs.AuthDTOs;

public sealed record UserDataExportResponse(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    DateTime? LgpdConsentGivenAt);
