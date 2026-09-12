using System.Text.Json;
using OficinaMecanica.Auth.Application.UseCases;
using OficinaMecanica.Auth.Domain.Entities;
using OficinaMecanica.Auth.Function.Events;

namespace OficinaMecanica.Auth.UnitTests;

public sealed class FunctionTests
{
    [Fact]
    public async Task FunctionHandler_DeveRetornarOkParaClienteValido()
    {
        var function = CriarFunction(new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Teste",
            Cpf = "52998224725",
            Ativo = true
        });

        var response = await function.FunctionHandler(Post("{\"cpf\":\"52998224725\"}"), null!);
        using var body = JsonDocument.Parse(response.Body);

        Assert.Equal(200, response.StatusCode);
        Assert.Equal("token-de-teste", body.RootElement.GetProperty("token").GetString());
    }

    [Fact]
    public async Task FunctionHandler_DeveRetornarBadRequestParaJsonInvalido()
    {
        var function = CriarFunction(null);

        var response = await function.FunctionHandler(Post("{"), null!);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("json_invalido", response.Body);
    }

    [Fact]
    public async Task FunctionHandler_DeveRetornarNotFoundParaClienteInexistente()
    {
        var function = CriarFunction(null);

        var response = await function.FunctionHandler(Post("{\"cpf\":\"52998224725\"}"), null!);

        Assert.Equal(404, response.StatusCode);
        Assert.Contains("cliente_nao_encontrado", response.Body);
    }

    [Fact]
    public async Task FunctionHandler_DeveRecusarMetodoDiferenteDePost()
    {
        var function = CriarFunction(null);
        var request = new FunctionUrlRequest
        {
            RequestContext = new FunctionUrlRequestContext
            {
                Http = new FunctionUrlHttpContext { Method = "GET" }
            }
        };

        var response = await function.FunctionHandler(request, null!);

        Assert.Equal(405, response.StatusCode);
    }

    private static OficinaMecanica.Auth.Function.Function CriarFunction(Cliente? cliente)
    {
        var useCase = new AutenticarClienteUseCase(
            new ClienteGatewayFake { Cliente = cliente },
            new CpfValidatorGatewayFake(),
            new TokenGatewayFake());

        return new OficinaMecanica.Auth.Function.Function(useCase);
    }

    private static FunctionUrlRequest Post(string body)
    {
        return new FunctionUrlRequest
        {
            Body = body,
            RequestContext = new FunctionUrlRequestContext
            {
                Http = new FunctionUrlHttpContext { Method = "POST" }
            }
        };
    }
}
