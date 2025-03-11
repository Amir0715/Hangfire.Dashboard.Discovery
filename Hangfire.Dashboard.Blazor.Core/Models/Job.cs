using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Hangfire.Dashboard.Blazor.Core.Models;

public class Job
{
    public long Id { get; set; }

    public string? State { get; set; }

    public Invocation Invocation { get; set; } = null!;

    public JsonDocument Arguments { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime? Expireat { get; set; }

    public virtual ICollection<Jobparameter> Jobparameters { get; set; } = new List<Jobparameter>();
}