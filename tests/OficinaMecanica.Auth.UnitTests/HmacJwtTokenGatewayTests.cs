using System.Text;
using System.Text.Json;
using OficinaMecanica.Auth.Domain.Entities;
using OficinaMecanica.Auth.Infrastructure.Gateways;

namespace OficinaMecanica.Auth.UnitTests;

public sealed class HmacJwtTokenGatewayTests
{
    [Fact]
    public void Gerar_DeveCriarJwtComIdentidadeDoClienteEExpiracao()
    {
        var agora = new DateTimeOffset(2026, 9, 12, 16, 0, 0, TimeSpan.Zero);
        const string secret = "segredo-de-teste-com-mais-de-32-caracteres";
        var cliente = new Cliente
        {
            Id = Guid.Parse("51bb62d7-a815-4f26-8791-2756991da091"),
            Nome = "Joao da Silva",
            Cpf = "52998224725",
            Ativo = true
        };
        var gateway = new HmacJwtTokenGateway(
            "OficinaMecanica.Api",
            "OficinaMecanica.Api",
            secret,
            TimeSpan.FromHours(2),
            new FixedTimeProvider(agora));

        var resultado = gateway.Gerar(cliente);
        var partes = resultado.Token.Split('.');
        using var payload = JsonDocument.Parse(Decode(partes[1]));

        Assert.Equal(3, partes.Length);
        Assert.Equal(cliente.Id.ToString(), payload.RootElement.GetProperty("sub").GetString());
        Assert.Equal(cliente.Cpf, payload.RootElement.GetProperty("cpf").GetString());
        Assert.Equal("Cliente", payload.RootElement.GetProperty("role").GetString());
        Assert.Equal("OficinaMecanica.Api", payload.RootElement.GetProperty("iss").GetString());
        Assert.Equal("OficinaMecanica.Api", payload.RootElement.GetProperty("aud").GetString());
        Assert.Equal(agora.AddHours(2).ToUnixTimeSeconds(), payload.RootElement.GetProperty("exp").GetInt64());
        Assert.Equal(CalcularAssinatura(partes[0], partes[1], secret), partes[2]);
        Assert.Equal(agora.AddHours(2), resultado.ExpiraEm);
    }

    private static string CalcularAssinatura(string header, string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var assinatura = hmac.ComputeHash(Encoding.ASCII.GetBytes($"{header}.{payload}"));

        return Convert.ToBase64String(assinatura)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string Decode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
