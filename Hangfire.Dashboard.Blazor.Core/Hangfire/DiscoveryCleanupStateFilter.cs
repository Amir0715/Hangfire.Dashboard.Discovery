using Hangfire.Dashboard.Blazor.Core.Extensions;
using Hangfire.States;
using Hangfire.Storage;

namespace Hangfire.Dashboard.Blazor.Core.Hangfire;

public class DiscoveryCleanupStateFilter : IApplyStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        using var jobStorageTransaction = transaction as JobStorageTransaction;

        var setId = context.BackgroundJob.GetSetKey();
        if (context.NewState.IsFinal)
        {
            jobStorageTransaction.ExpireSet(setId, context.JobExpirationTimeout);
        }
        else
        {
            jobStorageTransaction.PersistSet(setId);
        }
        
        jobStorageTransaction.Commit();
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        
    }
}