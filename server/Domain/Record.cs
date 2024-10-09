using System.ComponentModel.DataAnnotations;

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
    /// You can use this to put an ID, type, extra info or any other information. Limited to 255 characters
    /// </summary>
    /// <summary>
    /// You can use this to put an ID, type, extra info or any other information. Limited to 255 characters
    /// </summary>
    [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
    public string description { get; set; } = null;

    /// <summary>
    /// Record created at a datetime
    /// </summary>
    public DateTime created_ad { get; set; }

    /// <summary>
    /// Your content. It can be string or json object
    /// </summary>
    public dynamic content { get; set; }

}
