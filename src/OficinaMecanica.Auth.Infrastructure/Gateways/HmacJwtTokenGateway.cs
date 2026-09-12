using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Auth.Application.Gateways;
using OficinaMecanica.Auth.Domain.Entities;

namespace OficinaMecanica.Auth.Infrastructure.Gateways;

public sealed class HmacJwtTokenGateway : ITokenGateway
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _secret;
    private readonly TimeSpan _tokenLifetime;
    private readonly TimeProvider _timeProvider;

    public HmacJwtTokenGateway(
        string issuer,
        string audience,
        string secret,
        TimeSpan tokenLifetime,
        TimeProvider? timeProvider = null)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new ArgumentException("Issuer nao pode ser vazio.", nameof(issuer));
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new ArgumentException("Audience nao pode ser vazia.", nameof(audience));
        }

        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
        {
            throw new ArgumentException("O segredo JWT deve possuir pelo menos 32 caracteres.", nameof(secret));
        }

        _issuer = issuer;
        _audience = audience;
        _secret = Encoding.UTF8.GetBytes(secret);
        _tokenLifetime = tokenLifetime;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public TokenGerado Gerar(Cliente cliente)
    {
        var agora = _timeProvider.GetUtcNow();
        var expiraEm = agora.Add(_tokenLifetime);
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, cliente.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, cliente.Nome),
            new("cpf", cliente.Cpf),
            new("role", "Cliente"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(_secret),
            SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = agora.UtcDateTime,
            NotBefore = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            SigningCredentials = credentials
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(handler.CreateToken(descriptor));

        return new TokenGerado(token, expiraEm);
    }
}
