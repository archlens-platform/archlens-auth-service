using System.Net.Http.Json;
using Reqnroll;

namespace ArchLens.Auth.Tests.BDD.StepDefinitions;

[Binding]
public sealed class RegistroSteps(ScenarioContext scenarioContext)
{
    private object _registrationPayload = null!;

    [Given("que eu tenho os dados de registro válidos")]
    public void DadoQueEuTenhoOsDadosDeRegistroValidos(DataTable table)
    {
        var row = table.Rows[0];
        _registrationPayload = new
        {
            Username = row["Username"],
            Email = row["Email"],
            Password = row["Password"],
            LgpdConsent = bool.Parse(row["LgpdConsent"])
        };
        scenarioContext.Set(_registrationPayload, "RequestPayload");
    }

    [Given("que já existe um usuário registrado com username {string}")]
    public async Task DadoQueJaExisteUmUsuarioRegistradoComUsername(string username)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var payload = new { Username = username, Email = $"{username}@archlens.com", Password = "Test@1234", LgpdConsent = true };
        var response = await client.PostAsJsonAsync("/auth/register", payload);
        response.EnsureSuccessStatusCode();
    }

    [Given("que já existe um usuário registrado com email {string}")]
    public async Task DadoQueJaExisteUmUsuarioRegistradoComEmail(string email)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var payload = new { Username = "existingemail", Email = email, Password = "Test@1234", LgpdConsent = true };
        var response = await client.PostAsJsonAsync("/auth/register", payload);
        response.EnsureSuccessStatusCode();
    }

    [When("eu envio uma requisição POST para {string}")]
    public async Task QuandoEuEnvioUmaRequisicaoPostPara(string endpoint)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var payload = scenarioContext.Get<object>("RequestPayload");
        var response = await client.PostAsJsonAsync(endpoint, payload);
        scenarioContext.Set(response, "Response");
    }
}
