using Hangfire.Dashboard.Blazor.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hangfire.Dashboard.Blazor;

public class DashboardBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DashboardBackgroundService> _logger;

    public DashboardBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DashboardBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var processors = scope.ServiceProvider.GetServices<IDashboardBackgroundProcessor>();
        
        // Создаем отдельные задачи для каждого процессора
        var processorTasks = processors
            .Select(processor => ProcessSingleProcessor(processor, stoppingToken))
            .ToList();

        await Task.WhenAll(processorTasks);
    }

    private async Task ProcessSingleProcessor(
        IDashboardBackgroundProcessor processor, 
        CancellationToken stoppingToken)
    {
        var processorName = processor.GetType().Name;
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.UtcNow;
            var interval = processor.ExecuteInterval;

            try
            {
                await processor.ExecuteAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, 
                    "Error in dashboard processor {ProcessorName}", processorName);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Processor {ProcessorName} execution canceled", processorName);
                return;
            }

            // Рассчет оставшегося времени до следующего запуска
            var elapsed = DateTime.UtcNow - startTime;
            var delay = interval - elapsed;

            if (delay > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return; // Корректный выход при отмене
                }
            }
            else
            {
                // Предупреждение, если выполнение занимает больше интервала
                _logger.LogWarning(
                    "Processor {ProcessorName} execution took {ElapsedTime} which exceeds its interval {Interval}",
                    processorName, elapsed, interval);
            }
        }
    }
}