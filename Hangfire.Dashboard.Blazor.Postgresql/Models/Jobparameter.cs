namespace Hangfire.Dashboard.Blazor.Postgresql.Models;

public class Jobparameter
{
    public long Id { get; set; }
    public long Jobid { get; set; }

    public string Name { get; set; } = null!;

    public string? Value { get; set; }
}
