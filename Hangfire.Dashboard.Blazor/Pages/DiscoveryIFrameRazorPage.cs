using Hangfire.Dashboard.Pages;

namespace Hangfire.Dashboard.Blazor.Pages;

public class DiscoveryIFrameRazorPage : RazorPage
{
    public override void Execute()
    {
        Layout = new LayoutPage("Discovery");
        WriteLiteral("""
                     <iframe src="/_hangfireBlazorDiscovery" style="width: 100%;height: calc(100dvh - 160px);border: none;"></iframe>
                     """);
    }
}