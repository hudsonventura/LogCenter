namespace LogCenter;

//TODO: docs
public class LogCenterOptions
{
    public bool HideResponseExceptions { get ; set; } = false;

    public string url { get ; set; } = "http://localhost:9200";
    public string table { get ; set; } = "desenv";
    public Guid correlation { get ; set; } = Guid.NewGuid();

    public string token { get; set; }




    public bool LogGetRequest { get; set; } = false;
    public SaveFormatType formatType { get; set; } = SaveFormatType.Json;

    public enum SaveFormatType
    {
        Json,
        HTTPText
    }
}
