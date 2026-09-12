using System.Text.Json.Serialization;

namespace OficinaMecanica.Auth.Function.Events;

public sealed class AutenticacaoRequest
{
    [JsonPropertyName("cpf")]
    public string? Cpf { get; init; }
}
