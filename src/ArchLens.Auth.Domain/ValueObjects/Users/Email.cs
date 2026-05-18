using ArchLens.SharedKernel.Domain;

namespace ArchLens.Auth.Domain.ValueObjects.Users;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalized = email.Trim().ToLowerInvariant();

        if (!normalized.Contains('@') || !normalized.Contains('.'))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(normalized);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
