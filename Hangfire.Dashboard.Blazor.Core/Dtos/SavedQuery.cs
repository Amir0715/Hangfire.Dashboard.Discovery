using System;

namespace Hangfire.Dashboard.Blazor.Core.Dtos;

public class SavedQuery
{
    /// <summary>
    /// Id saved query
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of saved query
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Saved query
    /// </summary>
    public string Query { get; set; } = null!;
    
    /// <summary>
    /// Creation date time
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}