using OficinaMecanica.Auth.Application.Gateways;

namespace OficinaMecanica.Auth.Infrastructure.Gateways;

public sealed class CpfValidatorGateway : ICpfValidatorGateway
{
    public bool EhValido(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1 || cpf.Any(caractere => !char.IsDigit(caractere)))
        {
            return false;
        }

        return CalcularDigito(cpf, 9) == cpf[9] - '0'
            && CalcularDigito(cpf, 10) == cpf[10] - '0';
    }

    private static int CalcularDigito(string cpf, int quantidadeDigitos)
    {
        var soma = 0;

        for (var indice = 0; indice < quantidadeDigitos; indice++)
        {
            soma += (cpf[indice] - '0') * (quantidadeDigitos + 1 - indice);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
