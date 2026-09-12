namespace OficinaMecanica.Auth.Domain.Entities;

public sealed class Cliente
{
    public required Guid Id { get; init; }
    public required string Nome { get; init; }
    public required string Cpf { get; init; }
    public required bool Ativo { get; init; }
}
