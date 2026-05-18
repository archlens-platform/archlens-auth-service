using ArchLens.Auth.Domain.ValueObjects.Users;
using ArchLens.SharedKernel.Domain;

namespace ArchLens.Auth.Domain.Entities.UserEntities;

public sealed class User : Entity<Guid>
{
    public const int MaxFailedAttempts = 5;
    public const int LockoutMinutes = 15;

    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime? LgpdConsentGivenAt { get; private set; }

    private User() { }

    public static User Create(string username, string email, string passwordHash, DateTime lgpdConsentGivenAt, UserRole role = UserRole.User)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            FailedLoginAttempts = 0,
            LgpdConsentGivenAt = lgpdConsentGivenAt,
        };
    }

    public bool IsLockedOut() => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;

    public void RegisterFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= MaxFailedAttempts)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutMinutes);
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        LastLoginAt = DateTime.UtcNow;
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
    }
}

public enum UserRole
{
    User = 0,
    Admin = 1,
}
