using System;
using System.Text.Json;

namespace Hangfire.Dashboard.Blazor.Core;

public class JobContext
{
    public long Id { get; set; }
    public string State { get; set; }
    public string Queue { get; set; }
    
    public string Type { get; set; }
    public string Method { get; set; }
    public JsonDocument Arguments { get; set; }
    public Params Params { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpireAt { get; set; }
}


/// <summary>
/// Параметры задачи, возможно будут DynamicObject
/// </summary>
public class Params
{
    
}