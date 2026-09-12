namespace OficinaMecanica.Auth.Application.UseCases;

public enum FalhaAutenticacao
{
    Nenhuma,
    CpfInvalido,
    ClienteNaoEncontrado,
    ClienteInativo
}

public sealed record AutenticacaoResultado(
    FalhaAutenticacao Falha,
    AutenticarClienteOutput? Dados)
{
    public bool Sucesso => Falha == FalhaAutenticacao.Nenhuma;

    public static AutenticacaoResultado ComSucesso(AutenticarClienteOutput dados) =>
        new(FalhaAutenticacao.Nenhuma, dados);

    public static AutenticacaoResultado ComFalha(FalhaAutenticacao falha) =>
        new(falha, null);
}
