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


    /// <summary>
    /// The name of response header that contains the TraceId. You can set the header name here as you want. Default is 'X-Trace-Id'
    /// </summary>
    public string TraceIdReponseHeader { get; set; } = "X-Trace-Id";

    public enum SaveFormatType
    {
        Json,
        HTTPText
    }
}
