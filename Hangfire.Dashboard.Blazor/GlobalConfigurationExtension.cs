using Hangfire.Dashboard.Blazor.Core.Hangfire;
using Hangfire.Dashboard.Blazor.Pages;

namespace Hangfire.Dashboard.Blazor;

public static class GlobalConfigurationExtension
{
    private static string _defaultRouteBase = "/discovery";

    public static IGlobalConfiguration UseBlazorDiscoveryPanel(this IGlobalConfiguration configuration)
    {
        DashboardRoutes.Routes.AddRazorPage($"{_defaultRouteBase}", match => new DiscoveryIFrameRazorPage());
        NavigationMenu.Items.Add(page => new MenuItem("Discovery", page.Url.To(_defaultRouteBase))
        {
            Active = page.RequestPath == _defaultRouteBase || page.RequestPath.StartsWith($"{_defaultRouteBase}/")
        });

        configuration.UseFilter(new JobArgumentScrapFilter());

        return configuration;
    } 
}