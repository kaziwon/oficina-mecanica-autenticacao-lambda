using MySqlConnector;

namespace OficinaMecanica.Auth.Infrastructure.Configuration;

public sealed class LambdaSettings
{
    public required string DatabaseConnectionString { get; init; }
    public required string JwtIssuer { get; init; }
    public required string JwtAudience { get; init; }
    public required string JwtSecret { get; init; }
    public required TimeSpan TokenLifetime { get; init; }

    public static LambdaSettings FromEnvironment()
    {
        var databaseHost = Required("DATABASE_HOST");
        var databaseName = Required("DATABASE_NAME");
        var databaseUser = Required("DATABASE_USER");
        var databasePassword = Required("DATABASE_PASSWORD");
        var jwtSecret = Required("JWT_SECRET");

        if (jwtSecret.Length < 32)
        {
            throw new InvalidOperationException("JWT_SECRET deve possuir pelo menos 32 caracteres.");
        }

        var portText = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "3306";
        if (!uint.TryParse(portText, out var databasePort))
        {
            throw new InvalidOperationException("DATABASE_PORT deve ser numerica.");
        }

        var expirationText = Environment.GetEnvironmentVariable("TOKEN_EXPIRATION_MINUTES") ?? "120";
        if (!int.TryParse(expirationText, out var expirationMinutes) || expirationMinutes <= 0)
        {
            throw new InvalidOperationException("TOKEN_EXPIRATION_MINUTES deve ser um inteiro positivo.");
        }

        var connectionString = new MySqlConnectionStringBuilder
        {
            Server = databaseHost,
            Port = databasePort,
            Database = databaseName,
            UserID = databaseUser,
            Password = databasePassword,
            SslMode = MySqlSslMode.Required,
            ConnectionTimeout = 5,
            DefaultCommandTimeout = 5
        }.ConnectionString;

        return new LambdaSettings
        {
            DatabaseConnectionString = connectionString,
            JwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "OficinaMecanica.Api",
            JwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "OficinaMecanica.Api",
            JwtSecret = jwtSecret,
            TokenLifetime = TimeSpan.FromMinutes(expirationMinutes)
        };
    }

    private static string Required(string name)
    {
        return Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Variavel de ambiente obrigatoria nao configurada: {name}.");
    }
}
