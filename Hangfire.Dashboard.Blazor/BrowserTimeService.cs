using Microsoft.JSInterop;

namespace Hangfire.Dashboard.Blazor;

public class BrowserTimeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly TimeProvider _timeProvider;
    private Lazy<Task<TimeSpan>> _userOffset;

    public BrowserTimeService(IJSRuntime jsRuntime, TimeProvider timeProvider)
    {
        _jsRuntime = jsRuntime;
        _timeProvider = timeProvider;
        _userOffset = new(async () =>
        {
            int offsetInMinutes = await _jsRuntime.InvokeAsync<int>("getTimezoneOffset");
            // getTimezoneOffset() возвращает отрицательное значение для зон восточнее UTC
            return TimeSpan.FromMinutes(-offsetInMinutes); 
        });
    }

    public async ValueTask<TimeSpan> GetBrowserOffsetAsync() => await _userOffset.Value;

    public async ValueTask<DateTimeOffset> BrowserNowAsync()
    {
        return await ToBrowserDateTimeAsync(_timeProvider.GetUtcNow());
    }

    public async ValueTask<DateTimeOffset> ToBrowserDateTimeAsync(DateTimeOffset dateTimeOffset)
    {
        // Получаем смещение из браузера при первом вызове
        var userOffset = await _userOffset.Value;
        
        // Преобразуем время с учетом смещения
        return dateTimeOffset.ToOffset(userOffset);
    }

    public async ValueTask<DateTimeOffset> FromBrowserDateTimeAsync(DateTime browserDateTime)
    {
        var userOffset = await _userOffset.Value;
        return new DateTimeOffset(browserDateTime, userOffset);
    }
}