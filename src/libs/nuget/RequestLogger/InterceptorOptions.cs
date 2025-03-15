namespace LogCenter.RequestInterceptor;

//TODO: docs
public class InterceptorOptions : LogCenterOptions
{
    /// <summary>
    /// Hide Exceptions when 500 Internal server error is returned to the user?    
    /// </summary>
    public bool HideResponseExceptions { get ; set; } = false;

    /// <summary>
    /// Log GET requests?
    /// </summary>
    public bool LogGetRequest { get; set; } = false;

    /// <summary>
    /// Save in HTTP Text or JSON?
    /// </summary>
    public SaveFormatType FormatType { get; set; } = SaveFormatType.Json;

    public enum SaveFormatType
    {
        Json,
        HTTPText
    }
}
