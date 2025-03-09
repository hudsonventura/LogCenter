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
    /// Represents the severity level of a log message.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Informational messages that represent the normal flow of an application.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Informational messages that represent the normal flow of an application.
    /// </summary>
    Information = 1,

    /// <summary>
    /// Debugging messages used for diagnosing issues.
    /// </summary>
    Debug = 2,

    /// <summary>
    /// Warning messages that indicate a potential issue or important event.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Error messages indicating a failure in the application.
    /// </summary>
    Error = 4,
    
    /// <summary>
    /// Critical issues causing an application to crash or terminate.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Success messages indicating a successful operation.
    /// </summary>
    Success = 6
}

