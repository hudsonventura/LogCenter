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
    /// Your token. Generate it on web interface on user's preference //TODO: Put link of the documentation here
    /// </summary>
    public string token { get; set; }
}
