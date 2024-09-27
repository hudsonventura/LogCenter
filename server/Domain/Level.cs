namespace server.Domain;

/// <summary>
/// Níveis de log
/// <para>1 - </para> Info
/// <para>2 - </para> Debug
/// <para>3 - </para> Warning
/// <para>4 - </para> Error
/// <para>5 - </para> Critical
/// </summary>
public enum Level
{
    /// <summary>
    /// Info
    /// </summary>
    Info = 1,
    /// <summary>
    /// Debug
    /// </summary>
    Debug = 2,
    /// <summary>
    /// Avisos
    /// </summary>
    Warning = 3,
    /// <summary>
    /// Erros
    /// </summary>
    Error = 4,
    /// <summary>
    /// Críticos
    /// </summary>
    Critical = 5
}

