namespace LogCenter;

/// <summary>
/// Permite aguardar o envio de logs pendentes durante o encerramento da aplicação.
/// Depois que o flush começa, novos logs deixam de usar fila em background.
/// </summary>
public sealed class LogCenterDrain
{
    private readonly LogCenterPendingSendTracker _tracker = new();

    internal LogCenterPendingSendTracker Tracker => _tracker;

    public Task FlushAsync() => _tracker.WaitForPendingSendsAsync();

    public void Flush() => FlushAsync().GetAwaiter().GetResult();
}
