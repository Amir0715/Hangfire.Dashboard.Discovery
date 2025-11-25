using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Services;

public class SavedQueryManager : ISavedQueryManager
{
    private readonly JobStorage _storage;
    public SavedQueryManager(JobStorage storage)
    {
        _storage = storage;
    }

    public async Task<IEnumerable<SavedQuery>> GetSavedQueriesAsync()
    {
        var savedQueries = await GetSavedQueriesFromStorageAsync();
        return savedQueries;
    }

    public async Task<Result<SavedQuery>> SaveQueryAsync(string name, string query)
    {
        var savedQuery = new SavedQuery { Id = Guid.NewGuid(), Name = name, Query = query, CreatedAt = DateTimeOffset.UtcNow };
        var savedQueries = await GetSavedQueriesFromStorageAsync();
        
        savedQueries.Add(savedQuery); 
        
        await SaveQueriesToStorageAsync(savedQueries);
        
        return Result<SavedQuery>.Success(savedQuery);
    }

    public async Task<Result<SavedQuery>> UpdateQueryAsync(SavedQuery query)
    {
        var savedQueries = await GetSavedQueriesFromStorageAsync();
        var findedQuery = savedQueries.FirstOrDefault(x => x.Id == query.Id);
        if (findedQuery == null)
        {
            return Result<SavedQuery>.Failed($"Query with id {query.Id} not found");
        }
        
        findedQuery.Name = query.Name;
        findedQuery.Query = query.Query;
        
        await SaveQueriesToStorageAsync(savedQueries);
        
        return Result<SavedQuery>.Success(findedQuery);
    }

    public async Task<Result> RemoveQueryAsync(SavedQuery query)
    {
        var savedQueries = await GetSavedQueriesFromStorageAsync();
        var findedQuery = savedQueries.FirstOrDefault(x => x.Id == query.Id);
        if (findedQuery == null)
        {
            return Result.Failed($"Query with id {query.Id} not found");
        }
        
        await SaveQueriesToStorageAsync(savedQueries);
        
        return Result.Success();
    }

    private Task<List<SavedQuery>> GetSavedQueriesFromStorageAsync()
    {
        using var readOnlyConnection = _storage.GetReadOnlyConnection();
        var set = readOnlyConnection.GetAllItemsFromSet(Constants.DiscoverySetSavedQueriesPrefix);
        var queriesString = set.FirstOrDefault();
        if (queriesString != null)
        {
            try
            {
                var queries = JsonSerializer.Deserialize<List<SavedQuery>>(queriesString);
                return Task.FromResult(queries);
            }
            catch (Exception e)
            {
                return Task.FromResult(new List<SavedQuery>());
            }
        }
        
        return Task.FromResult(new List<SavedQuery>());
    }

    private Task SaveQueriesToStorageAsync(List<SavedQuery> queries)
    {
        using var connection = _storage.GetConnection();
        using var writeOnlyTransaction = connection.CreateWriteTransaction();
        writeOnlyTransaction.AddToSet(Constants.DiscoverySetSavedQueriesPrefix, JsonSerializer.Serialize(queries));
        writeOnlyTransaction.Commit();
        
        return Task.CompletedTask;
    }
}