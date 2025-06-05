using Bit.BlazorUI;
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

namespace Hangfire.Dashboard.Blazor;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireBlazorDashboard(this IServiceCollection services)
    {
        // TODO: remove FluentValidation
        services.AddValidatorsFromAssemblyContaining(typeof(QueryValidator));
        services.AddScoped<ITokenizer, Tokenizer>();
        services.AddScoped<IJobProvider, JobProvider>();
        services.AddScoped<IExpressionGenerator, ExpressionGenerator>();
        
        // TODO: remove Bit.BlazorUI;
        services.AddBitBlazorUIServices();
        
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
        services.AddHostedService<DashboardBackgroundService>();
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