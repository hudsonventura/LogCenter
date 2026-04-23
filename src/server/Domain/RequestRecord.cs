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
    public LogCategory Category { get; set; } = LogCategory.Log;

    /// <summary>
    /// 
    /// </summary> 
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 
    /// </summary> 
    public Level Level { get; set; } = Level.Info;

    /// <summary>
    /// 
    /// </summary> 
    public string? TraceId { get; set; }

    /// <summary>
    /// 
    /// </summary> 
    public dynamic? Content { get; set; }
    
}
