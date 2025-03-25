namespace LogCenter;

/// <summary>
/// LogCenter's options to configure the comunication
/// </summary>
public class LogCenterOptions
{
    /// <summary>
    /// LogCenter's URL
    /// </summary>
    public string url { get ; set; } = string.Empty;

    /// <summary>
    /// Table name
    /// </summary>
    public string table { get ; set; } = string.Empty;


    /// <summary>
    /// Your token. Generate it on web interface on user's preference
    /// </summary>
    public string token { get; set; } = string.Empty;
    
    /// <summary>
    /// Log the message on the console as a comon Console.WitreLine()
    /// </summary>
    public bool consoleLog { get; set; } = true;
    
    /// <summary>
    /// Log the entry on the console as a comon Console.WitreLine()
    /// </summary>
    public bool consoleLogEntireObject { get; set; }
}
