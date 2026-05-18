using Reqnroll;

namespace ArchLens.Auth.Tests.BDD.StepDefinitions;

[Binding]
public sealed class DadosPessoaisSteps(ScenarioContext scenarioContext)
{
    [When("eu envio uma requisição GET para {string}")]
    public async Task QuandoEuEnvioUmaRequisicaoGetPara(string endpoint)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var response = await client.GetAsync(endpoint);
        scenarioContext.Set(response, "Response");
    }

    [When("eu envio uma requisição DELETE para {string}")]
    public async Task QuandoEuEnvioUmaRequisicaoDeletePara(string endpoint)
    {
        var client = scenarioContext.Get<HttpClient>("HttpClient");
        var response = await client.DeleteAsync(endpoint);
        scenarioContext.Set(response, "Response");
    }
}
