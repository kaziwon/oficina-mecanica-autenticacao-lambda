using System.Text;
using System.Text.Json;
using Amazon.Lambda.Core;
using OficinaMecanica.Auth.Application.UseCases;
using OficinaMecanica.Auth.Function.Events;
using OficinaMecanica.Auth.Infrastructure.Configuration;
using OficinaMecanica.Auth.Infrastructure.Gateways;

namespace OficinaMecanica.Auth.Function;

public sealed class Function
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly IReadOnlyDictionary<string, string> ResponseHeaders =
        new Dictionary<string, string>
        {
            ["content-type"] = "application/json; charset=utf-8",
            ["cache-control"] = "no-store",
            ["x-content-type-options"] = "nosniff"
        };

    private readonly AutenticarClienteUseCase _useCase;

    public Function()
        : this(CreateUseCase(LambdaSettings.FromEnvironment()))
    {
    }

    public Function(AutenticarClienteUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<FunctionUrlResponse> FunctionHandler(
        FunctionUrlRequest request,
        ILambdaContext context)
    {
        var requestId = context?.AwsRequestId ?? Guid.NewGuid().ToString();

        try
        {
            if (!string.Equals(request.RequestContext?.Http?.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                return Error(405, "metodo_nao_permitido", "Utilize POST para autenticar.");
            }

            var body = DecodeBody(request);
            var payload = JsonSerializer.Deserialize<AutenticacaoRequest>(body, JsonOptions);

            if (string.IsNullOrWhiteSpace(payload?.Cpf))
            {
                return Error(400, "cpf_obrigatorio", "Informe o CPF.");
            }

            var resultado = await _useCase.ExecutarAsync(new AutenticarClienteInput(payload.Cpf));
            Log("Information", "autenticacao_cpf_processada", requestId, payload.Cpf, resultado.Falha.ToString());

            return resultado.Falha switch
            {
                FalhaAutenticacao.CpfInvalido =>
                    Error(400, "cpf_invalido", "O CPF informado e invalido."),
                FalhaAutenticacao.ClienteNaoEncontrado =>
                    Error(404, "cliente_nao_encontrado", "Cliente nao encontrado."),
                FalhaAutenticacao.ClienteInativo =>
                    Error(403, "cliente_inativo", "O cliente esta inativo."),
                _ => Json(200, resultado.Dados!)
            };
        }
        catch (JsonException)
        {
            return Error(400, "json_invalido", "O corpo da requisicao possui JSON invalido.");
        }
        catch (FormatException)
        {
            return Error(400, "corpo_invalido", "Nao foi possivel interpretar o corpo da requisicao.");
        }
        catch (Exception exception)
        {
            Log("Error", "autenticacao_cpf_falhou", requestId, null, exception.GetType().Name);
            return Error(500, "erro_interno", "Nao foi possivel processar a autenticacao.");
        }
    }

    private static AutenticarClienteUseCase CreateUseCase(LambdaSettings settings)
    {
        return new AutenticarClienteUseCase(
            new MySqlClienteGateway(settings.DatabaseConnectionString),
            new CpfValidatorGateway(),
            new HmacJwtTokenGateway(
                settings.JwtIssuer,
                settings.JwtAudience,
                settings.JwtSecret,
                settings.TokenLifetime));
    }

    private static string DecodeBody(FunctionUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return "{}";
        }

        return request.IsBase64Encoded
            ? Encoding.UTF8.GetString(Convert.FromBase64String(request.Body))
            : request.Body;
    }

    private static FunctionUrlResponse Json(int statusCode, object body)
    {
        return new FunctionUrlResponse
        {
            StatusCode = statusCode,
            Headers = ResponseHeaders,
            Body = JsonSerializer.Serialize(body, JsonOptions)
        };
    }

    private static FunctionUrlResponse Error(int statusCode, string codigo, string mensagem)
    {
        return Json(statusCode, new { codigo, mensagem });
    }

    private static void Log(
        string level,
        string eventName,
        string requestId,
        string? cpf,
        string resultado)
    {
        var cpfNormalizado = string.IsNullOrWhiteSpace(cpf)
            ? null
            : new string(cpf.Where(char.IsDigit).ToArray());
        var cpfMascarado = cpfNormalizado is { Length: 11 }
            ? $"***.***.***-{cpfNormalizado[^2..]}"
            : null;

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            timestamp = DateTimeOffset.UtcNow,
            level,
            eventName,
            requestId,
            cpf = cpfMascarado,
            resultado
        }, JsonOptions));
    }
}
