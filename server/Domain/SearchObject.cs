namespace server.Domain;

public class SearchObject
{
    /// <summary>
    /// Term of search
    /// </summary>
    public string search { get; set; } = string.Empty;

    /// <summary>
    /// Search from datetime
    /// </summary>
    public DateTime datetime1 { get; set; } = DateTime.UtcNow.AddHours(-1);

    /// <summary>
    /// Search until datetime
    /// </summary>
    public DateTime datetime2 { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Inform the page that you want
    /// </summary>
    public int page { get; set; } = 1;

    /// <summary>
    /// Inform the page size
    /// </summary>
    public  int take {get; set; } = 100;
    
    /// <summary>
    /// Read only. Represents how many itens the paginations must skip during DB select
    /// </summary>
    internal int skip {
        get{
            return take*(page-1);
        }
        private set{

        }
    }
}
