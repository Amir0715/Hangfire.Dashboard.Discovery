using System.Text.Json;

namespace Hangfire.Dashboard.Blazor.Postgresql.Models;

public class JobInfo
{
    public string JobType { get; set; }
    public string JobMethod { get; set; }
    
    /// <summary>
    /// Обработанный json объект аргументов.
    /// </summary>
    public JsonDocument Args { get; set; }
}