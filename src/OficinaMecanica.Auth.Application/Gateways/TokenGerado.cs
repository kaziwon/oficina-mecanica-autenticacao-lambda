namespace OficinaMecanica.Auth.Application.Gateways;

public sealed record TokenGerado(string Token, DateTimeOffset ExpiraEm);
