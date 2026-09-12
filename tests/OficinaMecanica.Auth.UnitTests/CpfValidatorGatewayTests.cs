using OficinaMecanica.Auth.Infrastructure.Gateways;

namespace OficinaMecanica.Auth.UnitTests;

public sealed class CpfValidatorGatewayTests
{
    private readonly CpfValidatorGateway _validator = new();

    [Theory]
    [InlineData("52998224725")]
    [InlineData("39053344705")]
    public void EhValido_DeveAceitarCpfValido(string cpf)
    {
        Assert.True(_validator.EhValido(cpf));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("52998224724")]
    [InlineData("5299822472A")]
    public void EhValido_DeveRejeitarCpfInvalido(string cpf)
    {
        Assert.False(_validator.EhValido(cpf));
    }
}
