namespace server.Domain;

public class SearchObject
{
    public string search { get; set; }
    public DateTime datetime1 { get; set; } = DateTime.UtcNow.AddHours(-1);
    public DateTime datetime2 { get; set; } = DateTime.UtcNow;

    public int page { get; set; } = 1;

    public  int take {get; set; } = 100;
    
    public int skip {
        get{
            return take*(page-1);
        }
    }
}
