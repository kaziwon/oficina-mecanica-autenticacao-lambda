namespace OficinaMecanica.Auth.Application.UseCases;

public sealed record AutenticarClienteOutput(
    Guid ClienteId,
    string Nome,
    string Cpf,
    string Status,
    string Token,
    DateTimeOffset ExpiraEm);
