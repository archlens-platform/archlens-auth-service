using System.Net.Http.Json;
using System.Text.Json;
using ArchLens.Auth.Tests.BDD.Hooks;
using FluentAssertions;
using Reqnroll;

namespace ArchLens.Auth.Tests.BDD.StepDefinitions;

[Binding]
public sealed class CommonSteps(ScenarioContext scenarioContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Given("que eu sou um usuário autenticado com role {string}")]
    public void DadoQueEuSouUmUsuarioAutenticadoComRole(string role)
    {
        var userId = Guid.NewGuid().ToString();
        scenarioContext.Set(userId, "AuthUserId");
        BddTestAuthHandler.SetAuthenticated(userId, role);
    }

    [Given("que eu não estou autenticado")]
    public void DadoQueEuNaoEstouAutenticado()
    {
        BddTestAuthHandler.Reset();
    }

    [Given("que o usuário autenticado existe no banco de dados")]
    public async Task DadoQueOUsuarioAutenticadoExisteNoBancoDeDados()
    {
        // Register a user via the API
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var payload = new { Username = "authtestuser", Email = "authtest@archlens.com", Password = "Test@1234", LgpdConsent = true };
        var response = await client.PostAsJsonAsync("/auth/register", payload);
        response.EnsureSuccessStatusCode();

        // Read the created user ID and update the auth handler
        var content = await response.Content.ReadAsStringAsync();
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content, JsonOptions);

        // Try both camelCase and PascalCase property names
        string? userId = null;
        if (json.TryGetProperty("userId", out var camelProp))
            userId = camelProp.GetString();
        else if (json.TryGetProperty("UserId", out var pascalProp))
            userId = pascalProp.GetString();

        if (userId != null)
        {
            scenarioContext.Set(userId, "AuthUserId");
            BddTestAuthHandler.SetAuthenticated(userId, "User");
        }
        else
        {
            throw new InvalidOperationException($"Could not find userId in register response: {content}");
        }
    }

    [Then("a resposta deve ter status code {int}")]
    public void EntaoARespostaDeveTerStatusCode(int statusCode)
    {
        var response = scenarioContext.Get<HttpResponseMessage>("Response");
        ((int)response.StatusCode).Should().Be(statusCode);
    }

    [Then("a resposta deve conter a mensagem {string}")]
    public async Task EntaoARespostaDeveConterAMensagem(string expectedMessage)
    {
        var response = scenarioContext.Get<HttpResponseMessage>("Response");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainEquivalentOf(expectedMessage);
    }

    [Then("a resposta deve conter o campo {string}")]
    public async Task EntaoARespostaDeveConterOCampo(string fieldName)
    {
        var response = scenarioContext.Get<HttpResponseMessage>("Response");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        json.TryGetProperty(fieldName, out _).Should().BeTrue($"expected field '{fieldName}' in response body");
    }

    [Then("a resposta deve conter o campo {string} com valor {string}")]
    public async Task EntaoARespostaDeveConterOCampoComValor(string fieldName, string expectedValue)
    {
        var response = scenarioContext.Get<HttpResponseMessage>("Response");
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content, JsonOptions);
        json.TryGetProperty(fieldName, out var value).Should().BeTrue($"expected field '{fieldName}' in response body");
        value.ToString().Should().BeEquivalentTo(expectedValue);
    }
}
