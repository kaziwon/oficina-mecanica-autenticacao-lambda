using OficinaMecanica.Auth.Application.Gateways;

namespace OficinaMecanica.Auth.Application.UseCases;

public sealed class AutenticarClienteUseCase
{
    private readonly IClienteGateway _clienteGateway;
    private readonly ICpfValidatorGateway _cpfValidatorGateway;
    private readonly ITokenGateway _tokenGateway;

    public AutenticarClienteUseCase(
        IClienteGateway clienteGateway,
        ICpfValidatorGateway cpfValidatorGateway,
        ITokenGateway tokenGateway)
    {
        _clienteGateway = clienteGateway;
        _cpfValidatorGateway = cpfValidatorGateway;
        _tokenGateway = tokenGateway;
    }

    public async Task<AutenticacaoResultado> ExecutarAsync(
        AutenticarClienteInput input,
        CancellationToken cancellationToken = default)
    {
        var cpf = Normalizar(input.Cpf);

        if (!_cpfValidatorGateway.EhValido(cpf))
        {
            return AutenticacaoResultado.ComFalha(FalhaAutenticacao.CpfInvalido);
        }

        var cliente = await _clienteGateway.ObterPorCpfAsync(cpf, cancellationToken);

        if (cliente is null)
        {
            return AutenticacaoResultado.ComFalha(FalhaAutenticacao.ClienteNaoEncontrado);
        }

        if (!cliente.Ativo)
        {
            return AutenticacaoResultado.ComFalha(FalhaAutenticacao.ClienteInativo);
        }

        var token = _tokenGateway.Gerar(cliente);
        var output = new AutenticarClienteOutput(
            cliente.Id,
            cliente.Nome,
            cliente.Cpf,
            "Ativo",
            token.Token,
            token.ExpiraEm);

        return AutenticacaoResultado.ComSucesso(output);
    }

    private static string Normalizar(string? cpf)
    {
        return string.IsNullOrWhiteSpace(cpf)
            ? string.Empty
            : new string(cpf.Where(char.IsDigit).ToArray());
    }
}
