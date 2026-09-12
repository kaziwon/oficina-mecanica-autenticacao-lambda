using MySqlConnector;
using OficinaMecanica.Auth.Application.Gateways;
using OficinaMecanica.Auth.Domain.Entities;

namespace OficinaMecanica.Auth.Infrastructure.Gateways;

public sealed class MySqlClienteGateway : IClienteGateway
{
    private const string Query = """
        SELECT CAST(`Id` AS CHAR) AS `Id`, `Nome`, `CpfCnpj`, `Ativo`
        FROM `Clientes`
        WHERE `CpfCnpj` = @cpf
        LIMIT 1;
        """;

    private readonly string _connectionString;

    public MySqlClienteGateway(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Cliente?> ObterPorCpfAsync(
        string cpf,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(Query, connection);
        command.Parameters.AddWithValue("@cpf", cpf);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Cliente
        {
            Id = Guid.Parse(reader.GetString("Id")),
            Nome = reader.GetString("Nome"),
            Cpf = reader.GetString("CpfCnpj"),
            Ativo = reader.GetBoolean("Ativo")
        };
    }
}
