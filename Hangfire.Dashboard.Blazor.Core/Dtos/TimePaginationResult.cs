using System;
using System.Collections.Generic;

namespace Hangfire.Dashboard.Blazor.Core.Dtos;

public class TimePaginationResult<T>
{
    public IEnumerable<T> Data { get; set; }

    public int Total { get; set; }
    public int TotalPages { get; set; }
    
    public DateTimeOffset? NextOffset { get; set; }
    public int Limit { get; set; }
    
    public TimePaginationResult(IEnumerable<T> data, DateTimeOffset? nextOffset, int limit, int total)
    {
        NextOffset = nextOffset;
        Limit = limit;
        Total = total;
        Data = data;

        if (limit == 0) return;

        var totalPages = total / (double)limit;
        var roundedTotalPages = Convert.ToInt32(Math.Ceiling(totalPages));
        TotalPages = roundedTotalPages;
    }
}