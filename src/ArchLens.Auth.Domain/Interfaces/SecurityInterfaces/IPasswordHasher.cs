namespace ArchLens.Auth.Domain.Interfaces.SecurityInterfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
