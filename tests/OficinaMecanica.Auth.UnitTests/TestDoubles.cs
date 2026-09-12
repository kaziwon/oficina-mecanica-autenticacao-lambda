using OficinaMecanica.Auth.Application.Gateways;
using OficinaMecanica.Auth.Domain.Entities;

namespace OficinaMecanica.Auth.UnitTests;

internal sealed class ClienteGatewayFake : IClienteGateway
{
    public Cliente? Cliente { get; set; }
    public int Chamadas { get; private set; }

    public Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        Chamadas++;
        return Task.FromResult(Cliente);
    }
}

internal sealed class CpfValidatorGatewayFake : ICpfValidatorGateway
{
    public bool Resultado { get; set; } = true;

    public bool EhValido(string cpf) => Resultado;
}

internal sealed class TokenGatewayFake : ITokenGateway
{
    public TokenGerado Resultado { get; set; } =
        new("token-de-teste", new DateTimeOffset(2026, 9, 12, 18, 0, 0, TimeSpan.Zero));

    public TokenGerado Gerar(Cliente cliente) => Resultado;
}

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
