using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Domain;

public class RequestRecord
{
    /// <summary>
    /// A message
    /// </summary>
    public string Message { get; set; } = string.Empty;


    /// <summary>
    /// 
    /// </summary> 
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 
    /// </summary> 
    public RecordLevel Level { get; set; } = RecordLevel.Information;

    /// <summary>
    /// 
    /// </summary> 
    public string? TraceId { get; set; }

    /// <summary>
    /// 
    /// </summary> 
    public dynamic? Content { get; set; }
    
}
