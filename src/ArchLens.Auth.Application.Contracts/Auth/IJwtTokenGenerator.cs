namespace ArchLens.Auth.Application.Contracts.Auth;

public interface IJwtTokenGenerator
{
    string Generate(string userId, string username, string role);
}
