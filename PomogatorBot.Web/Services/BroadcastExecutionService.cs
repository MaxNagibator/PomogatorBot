using System.Threading.Channels;

namespace PomogatorBot.Web.Services;

public class BroadcastExecutionService : BackgroundService
{
    private readonly Channel<BroadcastTask> _taskQueue;
    private readonly ChannelWriter<BroadcastTask> _taskWriter;
    private readonly ChannelReader<BroadcastTask> _taskReader;
    private readonly IServiceScopeFactory _serviceProvider;
    private readonly ILogger<BroadcastExecutionService> _logger;

    public BroadcastExecutionService(IServiceScopeFactory serviceProvider, ILogger<BroadcastExecutionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
        };

        _taskQueue = Channel.CreateBounded<BroadcastTask>(options);
        _taskWriter = _taskQueue.Writer;
        _taskReader = _taskQueue.Reader;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Остановка сервиса выполнения рассылок...");

        _taskWriter.Complete();

        await base.StopAsync(cancellationToken);

        _logger.LogInformation("Сервис выполнения рассылок корректно остановлен");
    }

    public async Task<bool> EnqueueAsync(BroadcastTask broadcastTask, CancellationToken cancellationToken = default)
    {
        try
        {
            await _taskWriter.WriteAsync(broadcastTask, cancellationToken);
            _logger.LogInformation("Задача рассылки {BroadcastId} поставлена в очередь на выполнение", broadcastTask.BroadcastId);
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось поставить задачу рассылки {BroadcastId} в очередь", broadcastTask.BroadcastId);
            return false;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Сервис выполнения рассылок запущен");

        await foreach (var broadcastTask in _taskReader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Обработка задачи рассылки {BroadcastId} из очереди", broadcastTask.BroadcastId);

            try
            {
                await ProcessTaskAsync(broadcastTask, stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ошибка при обработке задачи рассылки {BroadcastId}", broadcastTask.BroadcastId);

                await UpdateFailureAsync(broadcastTask, exception.Message);
            }
        }

        _logger.LogInformation("Сервис выполнения рассылок остановлен");
    }

    private async Task ProcessTaskAsync(BroadcastTask broadcastTask, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<UserService>();
        var broadcastProgressService = scope.ServiceProvider.GetRequiredService<BroadcastProgressService>();
        var broadcastPendingService = scope.ServiceProvider.GetRequiredService<BroadcastPendingService>();

        _logger.LogInformation("Выполнение рассылки {BroadcastId} для администратора {AdminUserId}",
            broadcastTask.BroadcastId, broadcastTask.AdminUserId);

        try
        {
            var userCount = await userService.GetCountBySubscriptionAsync(broadcastTask.Subscribes, broadcastTask.AdminUserId, cancellationToken);

            broadcastProgressService.StartProgress(broadcastTask.BroadcastId,
                broadcastTask.ChatId,
                broadcastTask.MessageId,
                userCount);

            await broadcastProgressService.UpdatePreparationStageAsync(broadcastTask.BroadcastId, cancellationToken);

            var response = await userService.NotifyWithProgressAsync(broadcastTask.Message,
                broadcastTask.Subscribes,
                broadcastTask.Entities,
                broadcastTask.AdminUserId,
                ProgressCallback,
                cancellationToken);

            broadcastPendingService.Remove(broadcastTask.BroadcastId);

            await broadcastProgressService.CompleteAsync(broadcastTask.BroadcastId,
                response.SuccessfulSends,
                response.FailedSends,
                response.TotalUsers,
                cancellationToken);

            _logger.LogInformation("Рассылка {BroadcastId} успешно завершена. Успешно: {Success}, Неуспешно: {Failed}, Всего: {Total}",
                broadcastTask.BroadcastId, response.SuccessfulSends, response.FailedSends, response.TotalUsers);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ошибка при выполнении рассылки {BroadcastId}", broadcastTask.BroadcastId);

            await UpdateFailureAsync(broadcastTask, exception.Message);
            broadcastPendingService.Remove(broadcastTask.BroadcastId);

            throw;
        }

        return;

        Task ProgressCallback(int successfulSends, int failedSends)
        {
            return broadcastProgressService.UpdateSendingProgressAsync(broadcastTask.BroadcastId,
                successfulSends,
                failedSends,
                CancellationToken.None);
        }
    }

    private async Task UpdateFailureAsync(BroadcastTask broadcastTask, string errorMessage)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var broadcastProgressService = scope.ServiceProvider.GetRequiredService<BroadcastProgressService>();

            await broadcastProgressService.FailBroadcastAsync(broadcastTask.BroadcastId,
                errorMessage,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не удалось обновить статус ошибки рассылки {BroadcastId}", broadcastTask.BroadcastId);
        }
    }
}
