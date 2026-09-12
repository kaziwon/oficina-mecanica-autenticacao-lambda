using OficinaMecanica.Auth.Application.UseCases;
using OficinaMecanica.Auth.Domain.Entities;

namespace OficinaMecanica.Auth.UnitTests;

public sealed class AutenticarClienteUseCaseTests
{
    [Fact]
    public async Task ExecutarAsync_DeveRejeitarCpfInvalidoSemConsultarBanco()
    {
        var clienteGateway = new ClienteGatewayFake();
        var useCase = new AutenticarClienteUseCase(
            clienteGateway,
            new CpfValidatorGatewayFake { Resultado = false },
            new TokenGatewayFake());

        var resultado = await useCase.ExecutarAsync(new AutenticarClienteInput("123"));

        Assert.Equal(FalhaAutenticacao.CpfInvalido, resultado.Falha);
        Assert.Equal(0, clienteGateway.Chamadas);
    }

    [Fact]
    public async Task ExecutarAsync_DeveInformarQuandoClienteNaoExistir()
    {
        var useCase = new AutenticarClienteUseCase(
            new ClienteGatewayFake(),
            new CpfValidatorGatewayFake(),
            new TokenGatewayFake());

        var resultado = await useCase.ExecutarAsync(new AutenticarClienteInput("529.982.247-25"));

        Assert.Equal(FalhaAutenticacao.ClienteNaoEncontrado, resultado.Falha);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRejeitarClienteInativo()
    {
        var useCase = new AutenticarClienteUseCase(
            new ClienteGatewayFake { Cliente = CriarCliente(ativo: false) },
            new CpfValidatorGatewayFake(),
            new TokenGatewayFake());

        var resultado = await useCase.ExecutarAsync(new AutenticarClienteInput("52998224725"));

        Assert.Equal(FalhaAutenticacao.ClienteInativo, resultado.Falha);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarTokenParaClienteAtivo()
    {
        var cliente = CriarCliente(ativo: true);
        var useCase = new AutenticarClienteUseCase(
            new ClienteGatewayFake { Cliente = cliente },
            new CpfValidatorGatewayFake(),
            new TokenGatewayFake());

        var resultado = await useCase.ExecutarAsync(new AutenticarClienteInput("529.982.247-25"));

        Assert.True(resultado.Sucesso);
        Assert.Equal(cliente.Id, resultado.Dados!.ClienteId);
        Assert.Equal("52998224725", resultado.Dados.Cpf);
        Assert.Equal("Ativo", resultado.Dados.Status);
        Assert.Equal("token-de-teste", resultado.Dados.Token);
    }

    private static Cliente CriarCliente(bool ativo)
    {
        return new Cliente
        {
            Id = Guid.NewGuid(),
            Nome = "Cliente Teste",
            Cpf = "52998224725",
            Ativo = ativo
        };
    }
}
