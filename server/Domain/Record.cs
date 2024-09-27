namespace server.Domain;

public class Record
{
    /// <summary>
    /// Snowflake id as a primary key
    /// </summary>
    public long id { get; set; }

    /// <summary>
    /// Represents a type of record
    /// </summary>
    public Level level { get; set; }

    /// <summary>
    /// Record created at a datetime
    /// </summary>
    public DateTime created_ad { get; set; }

    /// <summary>
    /// Your content. It can be string or json object
    /// </summary>
    public dynamic content { get; set; }

}
