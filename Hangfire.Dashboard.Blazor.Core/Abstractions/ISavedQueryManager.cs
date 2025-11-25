using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire.Dashboard.Blazor.Core.Dtos;

namespace Hangfire.Dashboard.Blazor.Core.Abstractions;

public interface ISavedQueryManager
{
    /// <summary>
    /// Get saved query from storage
    /// </summary>
    /// <returns>IEnumerable of all saved queries</returns>
    public Task<IEnumerable<SavedQuery>> GetSavedQueriesAsync();
    
    /// <summary>
    /// Save query in storage
    /// </summary>
    /// <param name="name">Saved query name</param>
    /// <param name="query">Query</param>
    /// <returns>Result with saved instance</returns>
    public Task<Result<SavedQuery>> SaveQueryAsync(string name, string query);
    
    /// <summary>
    /// Update query in storage
    /// </summary>
    /// <param name="query">Saved query instance for update</param>
    /// <returns>Result with updated instance</returns>
    public Task<Result<SavedQuery>> UpdateQueryAsync(SavedQuery query);
    
    /// <summary>
    /// Remove query from storage
    /// </summary>
    /// <param name="query">Query for remove</param>
    /// <returns>Result for removing</returns>
    public Task<Result> RemoveQueryAsync(SavedQuery query);
}