using System.Text.Json.Serialization;

namespace OficinaMecanica.Auth.Function.Events;

public sealed class FunctionUrlRequest
{
    [JsonPropertyName("body")]
    public string? Body { get; init; }

    [JsonPropertyName("isBase64Encoded")]
    public bool IsBase64Encoded { get; init; }

    [JsonPropertyName("requestContext")]
    public FunctionUrlRequestContext? RequestContext { get; init; }
}

public sealed class FunctionUrlRequestContext
{
    [JsonPropertyName("http")]
    public FunctionUrlHttpContext? Http { get; init; }
}

public sealed class FunctionUrlHttpContext
{
    [JsonPropertyName("method")]
    public string? Method { get; init; }
}
