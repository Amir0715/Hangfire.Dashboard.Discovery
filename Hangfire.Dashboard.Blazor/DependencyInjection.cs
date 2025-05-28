using Bit.BlazorUI;
using FluentValidation;
using Hangfire.Dashboard.Blazor.Components;
using Hangfire.Dashboard.Blazor.Core;
using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Hangfire.Dashboard.Blazor.Core.Services;
using Hangfire.Dashboard.Blazor.Core.Tokenization;
using Hangfire.Dashboard.Blazor.Core.Validators;
using Hangfire.Dashboard.Blazor.Postgresql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hangfire.Dashboard.Blazor;

public static class DependencyInjection
{
    public static IServiceCollection AddHangfireBlazorDashboard(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(QueryValidator));
        services.AddScoped<ITokenizer, Tokenizer>();
        services.AddScoped<IJobProvider, JobProvider>();
        services.AddScoped<IExpressionGenerator, ExpressionGenerator>();
        services.AddHangfireDbContext(configuration);
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