using System;
using System.Text.Json;

namespace Hangfire.Dashboard.Blazor.Core;

public class JobContext : IDisposable
{
    public long Id { get; set; }
    public string State { get; set; }
    public string Queue { get; set; }
    
    public string Type { get; set; }
    public string Method { get; set; }
    public JsonDocument Args { get; set; }
    public JsonDocument Params { get; set; }
    public JsonDocument? StateData { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public string JobName => $"{Type.Split(',')[0]}.{Method}";

    public string? ExceptionType
    {
        get
        {
            if (StateData == null || 
                !StateData.RootElement.TryGetProperty(nameof(ExceptionType), out var jsonElement))
            {
                return null;
            }

            return jsonElement.GetString();
        }
    }
    
    public string? ExceptionMessage
    {
        get
        {
            if (StateData == null || !StateData.RootElement.TryGetProperty(nameof(ExceptionMessage), out var jsonElement))
            {
                return null;
            }

            return jsonElement.GetString();
        }
    }

    public void Dispose()
    {
        Args.Dispose();
        Params.Dispose();
        StateData?.Dispose();
    }
}