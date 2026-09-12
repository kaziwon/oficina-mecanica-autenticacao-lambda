using OficinaMecanica.Auth.Domain.Entities;

namespace OficinaMecanica.Auth.Application.Gateways;

public interface ITokenGateway
{
    TokenGerado Gerar(Cliente cliente);
}
