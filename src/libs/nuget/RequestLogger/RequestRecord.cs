namespace LogCenter.RequestInterceptor;

/// <summary>
/// Payload enviado ao endpoint do LogCenter pelo interceptor.
/// Mantém o mesmo formato esperado pelo backend, mas é definido localmente
/// para evitar dependência do projeto do servidor.
/// </summary>
internal sealed class RequestRecord
{
    public string Message { get; set; } = string.Empty;


    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int Level { get; set; } = 1;

    public string? TraceId { get; set; }

    public object? Content { get; set; }
}
