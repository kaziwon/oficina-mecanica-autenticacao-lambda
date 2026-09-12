using System.Text.Json.Serialization;

namespace OficinaMecanica.Auth.Function.Events;

public sealed class FunctionUrlResponse
{
    [JsonPropertyName("statusCode")]
    public required int StatusCode { get; init; }

    [JsonPropertyName("headers")]
    public required IReadOnlyDictionary<string, string> Headers { get; init; }

    [JsonPropertyName("body")]
    public required string Body { get; init; }

    [JsonPropertyName("isBase64Encoded")]
    public bool IsBase64Encoded => false;
}
