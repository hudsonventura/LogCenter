namespace LogCenter;

//TODO: docs
public class LogCenterOptions
{
    /// <summary>
    /// LogCenter's URL
    /// </summary>
    public string url { get ; set; } = "http://localhost:9200";

    /// <summary>
    /// Table name
    /// </summary>
    public string table { get ; set; } = "desenv";


    /// <summary>
    /// Your token. Generate it on web interface on user's preference
    /// </summary>
    public string token { get; set; }
    
    /// <summary>
    /// Log the message on the console as a comon Console.WitreLine()
    /// </summary>
    public bool consoleLog { get; set; } = true;
    
    /// <summary>
    /// Log the entry on the console as a comon Console.WitreLine()
    /// </summary>
    public bool consoleLogEntireObject { get; set; } = false;
}
