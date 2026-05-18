using System.Net.Http.Json;
using Reqnroll;

namespace ArchLens.Auth.Tests.BDD.StepDefinitions;

[Binding]
public sealed class LoginSteps(ScenarioContext scenarioContext)
{
    [Given("que existe um usuário registrado com username {string} e senha {string}")]
    public async Task DadoQueExisteUmUsuarioRegistradoComUsernameESenha(string username, string password)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var payload = new { Username = username, Email = $"{username}@archlens.com", Password = password, LgpdConsent = true };
        var response = await client.PostAsJsonAsync("/auth/register", payload);
        response.EnsureSuccessStatusCode();
    }

    [Given("que o usuário {string} teve {int} tentativas de login falhas")]
    public async Task DadoQueOUsuarioTeveNTentativasDeLoginFalhas(string username, int attempts)
    {
        // Simulate failed login attempts by sending wrong passwords
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        for (int i = 0; i < attempts; i++)
        {
            await client.PostAsJsonAsync("/auth/login", new { Username = username, Password = $"WrongPass@{i}" });
        }
    }

    [When("eu envio uma requisição de login com username {string} e senha {string}")]
    public async Task QuandoEuEnvioUmaRequisicaoDeLoginComUsernameESenha(string username, string password)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var payload = new { Username = username, Password = password };
        var response = await client.PostAsJsonAsync("/auth/login", payload);
        scenarioContext.Set(response, "Response");
    }
}
