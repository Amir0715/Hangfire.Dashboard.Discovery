using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Hangfire.Client;
using Hangfire.Dashboard.Blazor.Core.Extensions;
using Hangfire.Server;

namespace Hangfire.Dashboard.Blazor.Core.Hangfire;

// TODO: Лучше сделать Internal
public class JobArgumentScrapFilter : IClientFilter
{
    // TODO: Придумать как прокинуть нормальный логер из DI
    public JobArgumentScrapFilter()
    {
    }

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

        var filteredJobArgs = job.Args.Where(x => x is not CancellationToken or PerformContext);

        foreach (var (parameterInfo, value) in parameterInfos.Zip(filteredJobArgs))
        {
            if (string.IsNullOrWhiteSpace(parameterInfo.Name))
            {
                return;
            }

            try
            {
                arguments[parameterInfo.Name] = JsonSerializer.SerializeToElement(value);
            }
            catch (Exception e)
            {
                // _logger.LogWarning(e,
                //     "Job {jobName} argument {argType} {argName} cant be scrapper for using at discovery page",
                //     job.Method.Name, parameterInfo.ParameterType, parameterInfo.Name);
            }
        }

        var jsonArguments = JsonSerializer.Serialize(arguments);
        using var transaction = filterContext.Connection.CreateWriteTransaction();
        transaction.AddToSet(filterContext.BackgroundJob.GetSetKey(), jsonArguments);

        transaction.Commit();
    }
}