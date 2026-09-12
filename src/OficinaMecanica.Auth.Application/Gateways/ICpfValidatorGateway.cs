namespace OficinaMecanica.Auth.Application.Gateways;

public interface ICpfValidatorGateway
{
    bool EhValido(string cpf);
}
