﻿using System.Text.Json;

namespace Hangfire.Dashboard.Blazor.Postgresql.Models;

public class Job
{
    public long Id { get; set; }

    public string State { get; set; }

    public Invocation Invocation { get; set; } = null!;

    public JsonDocument? Arguments { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime? Expireat { get; set; }
}