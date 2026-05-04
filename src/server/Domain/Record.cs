using System.ComponentModel.DataAnnotations;

namespace server.Domain;

public class Record
{
    /// <summary>
    /// Snowflake id as a primary key
    /// </summary>
    public Guid Id { get; set; } = SnowflakeGuid.NewGuid();

    /// <summary>
    /// Represents a type of record
    /// </summary>
    public RecordLevel Level { get; set; }

    /// <summary>
    /// You can use this to put an ID, type, extra info or any other information. Limited to 255 characters
    /// </summary>
    /// <summary>
    /// You can use this to put an ID, type, extra info or any other information. Limited to 255 characters
    /// </summary>
    [StringLength(255, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
    public string TraceId { get; set; } = null;

    /// <summary>
    /// Record created at a datetime
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// A message or title to explain the content
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Your content. It can be string or json object
    /// </summary>
    public dynamic Content { get; set; }

    /// <summary>
    /// Optional tags extracted from the payload field "_tags_"
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Indicates that the content was fully represented in the rendered message.
    /// </summary>
    public bool HideContentWhenMessageIsRendered { get; set; }

}
