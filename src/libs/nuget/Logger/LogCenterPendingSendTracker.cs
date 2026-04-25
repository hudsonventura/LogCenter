namespace LogCenter;

internal sealed class LogCenterPendingSendTracker
{
    private readonly object _sync = new();
    private TaskCompletionSource? _drainedTcs;
    private int _pendingCount;
    private bool _stopping;

    public bool TryBegin()
    {
        lock (_sync)
        {
            if (_stopping)
                return false;

            _pendingCount++;
            return true;
        }
    }

    public void Complete()
    {
        TaskCompletionSource? drainedTcs = null;

        lock (_sync)
        {
            if (_pendingCount > 0)
                _pendingCount--;

            if (_stopping && _pendingCount == 0)
                drainedTcs = _drainedTcs;
        }

        drainedTcs?.TrySetResult();
    }

    public Task WaitForPendingSendsAsync()
    {
        lock (_sync)
        {
            _stopping = true;

            if (_pendingCount == 0)
                return Task.CompletedTask;

            _drainedTcs ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _drainedTcs.Task;
        }
    }
}
