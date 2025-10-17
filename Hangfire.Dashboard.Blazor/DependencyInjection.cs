using FluentValidation;
using Hangfire.Dashboard.Blazor.Components;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Services;
using Hangfire.Dashboard.Blazor.Core.Tokenization;
using Hangfire.Dashboard.Blazor.Core.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hangfire.Dashboard.Blazor;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireBlazorDashboard(this IServiceCollection services, Action<HangfireDiscoveryOptions>? optionConfigure = null)
    {
        // TODO: remove FluentValidation
        services.AddValidatorsFromAssemblyContaining(typeof(QueryValidator));
        services.AddScoped<ITokenizer, Tokenizer>();
        services.AddScoped<IJobProvider, JobProvider>();
        services.AddScoped<IExpressionGenerator, ExpressionGenerator>();
        
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<BrowserTimeService>();
        
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddHostedService<DashboardBackgroundService>();

        var optionsBuilder = services.AddOptions<HangfireDiscoveryOptions>();
        if (optionConfigure != null)
            optionsBuilder.Configure(optionConfigure);
        
        return services;
    }
    
    public static IEndpointRouteBuilder MapHangfireBlazorDashboard(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder
            .MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .DisableAntiforgery();
        
        return endpointRouteBuilder;
    }
}