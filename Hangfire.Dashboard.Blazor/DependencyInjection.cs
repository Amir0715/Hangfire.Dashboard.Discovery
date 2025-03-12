using Hangfire.Dashboard.Blazor.Components;
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
        services.AddHangfireDbContext(configuration);
        
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
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