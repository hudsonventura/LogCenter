namespace nuget;

public class LogCenterOptions
{
    public string url { get ; set; } = "http://localhost:9200";
    public string table { get ; set; } = "desenv";
    public string timezone { get ; set; } = "UTC";
    public Guid correlation { get ; set; } = Guid.NewGuid();
}
