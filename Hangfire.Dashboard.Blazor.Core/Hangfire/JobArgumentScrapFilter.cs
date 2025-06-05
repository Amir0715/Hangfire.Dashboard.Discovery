using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hangfire.Client;
using Hangfire.Dashboard.Blazor.Core.Extensions;

namespace Hangfire.Dashboard.Blazor.Core.Hangfire;

public class JobArgumentScrapFilter : IClientFilter
{
    public void OnCreating(CreatingContext filterContext)
    {
        // Skip...
    }

    public void OnCreated(CreatedContext filterContext)
    {
        // Scrap name, and other from job type with reflection

        var job = filterContext.BackgroundJob.Job;
        var parameterInfos = job.Method.GetParameters();
        var arguments = new Dictionary<string, JsonElement?>();

        foreach (var (parameterInfo, value) in parameterInfos.Zip(job.Args))
        {
            if (string.IsNullOrWhiteSpace(parameterInfo.Name))
            {
                return;
            }

            arguments[parameterInfo.Name] = JsonSerializer.SerializeToElement(value);
        }
                
        var jsonArguments = JsonSerializer.Serialize(arguments);
        using var transaction = filterContext.Connection.CreateWriteTransaction();
        transaction.AddToSet(filterContext.BackgroundJob.GetSetKey(), jsonArguments);
        
        transaction.Commit();
    }
}