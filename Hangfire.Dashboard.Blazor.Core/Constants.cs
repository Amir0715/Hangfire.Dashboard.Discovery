using System;

namespace Hangfire.Dashboard.Blazor.Core;

public class Constants
{
    /// <summary>
    ///   
    /// </summary>
    public static readonly TimeSpan StartDateTimeOffsetByNow = TimeSpan.FromMinutes(15).Negate();
}