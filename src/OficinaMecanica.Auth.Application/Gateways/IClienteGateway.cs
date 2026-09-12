using OficinaMecanica.Auth.Domain.Entities;

namespace OficinaMecanica.Auth.Application.Gateways;

public interface IClienteGateway
{
    Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken cancellationToken = default);
}
