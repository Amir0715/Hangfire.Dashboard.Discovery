using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Services;

public class SavedQueryManager : ISavedQueryManager
{
    private static List<SavedQuery> _savedQueries = new()
    {
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Failed Jobs",
            Query = "State == \"Failed\"",
            CreatedAt = DateTimeOffset.UtcNow,
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Succeed Jobs",
            Query = "State == \"Succeed\"",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
        }
    };

    public Task<IEnumerable<SavedQuery>> GetSavedQueriesAsync()
    {
        return Task.FromResult(_savedQueries.AsEnumerable());
    }

    public Task<Result<SavedQuery>> SaveQueryAsync(string name, string query)
    {
        var savedQuery = new SavedQuery { Id = Guid.NewGuid(), Name = name, Query = query, CreatedAt = DateTimeOffset.UtcNow };
        _savedQueries.Add(savedQuery);
        return Task.FromResult(Result<SavedQuery>.Success(savedQuery));
    }

    public Task<Result<SavedQuery>> UpdateQueryAsync(SavedQuery query)
    {
        var findedQuery = _savedQueries.FirstOrDefault(x => x.Id == query.Id);
        if (findedQuery == null)
        {
            return Task.FromResult(Result<SavedQuery>.Failed($"Query with id {query.Id} not found"));
        }
        
        findedQuery.Name = query.Name;
        findedQuery.Query = query.Query;
        
        return Task.FromResult(Result<SavedQuery>.Success(findedQuery));
    }

    public Task<Result> RemoveQueryAsync(SavedQuery query)
    {
        var findedQuery = _savedQueries.FirstOrDefault(x => x.Id == query.Id);
        if (findedQuery == null)
        {
            return Task.FromResult(Result.Failed($"Query with id {query.Id} not found"));
        }
        
        _savedQueries.Remove(findedQuery);
        
        return Task.FromResult(Result.Success());
    }
}