using System.Collections.Concurrent;

namespace PomogatorBot.Web.Services;

public sealed class BroadcastPendingService : IDisposable
{
    private readonly ConcurrentDictionary<string, PendingBroadcast> _pendingBroadcasts = new();
    private readonly PeriodicTimer _cleanupTimer;
    private readonly ILogger<BroadcastPendingService> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly BroadcastNotificationService _notificationService;

    public BroadcastPendingService(BroadcastNotificationService notificationService, ILogger<BroadcastPendingService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
        _cleanupTimer = new(TimeSpan.FromDays(0.5));
        _ = StartCleanupLoopAsync();
    }

    public string Store(
        string message,
        Subscribes subscribes,
        MessageEntity[]? entities,
        long adminUserId)
    {
        var pendingId = Guid.NewGuid().ToString("N")[..8];
        var now = DateTime.UtcNow;

        var pendingBroadcast = new PendingBroadcast
        {
            Id = pendingId,
            Message = message,
            Subscribes = subscribes,
            Entities = entities,
            AdminUserId = adminUserId,
            CreatedAt = now,
            ExpiresAt = now.AddHours(1),
        };

        _pendingBroadcasts[pendingId] = pendingBroadcast;
        return pendingId;
    }

    public PendingBroadcast? Get(string pendingId)
    {
        _pendingBroadcasts.TryGetValue(pendingId, out var pendingBroadcast);

        if (pendingBroadcast == null || pendingBroadcast.ExpiresAt >= DateTime.UtcNow)
        {
            return pendingBroadcast;
        }

        _pendingBroadcasts.TryRemove(pendingId, out _);
        return null;
    }

    public void Remove(string pendingId)
    {
        _pendingBroadcasts.TryRemove(pendingId, out _);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cleanupTimer.Dispose();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task StartCleanupLoopAsync()
    {
        try
        {
            while (await _cleanupTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
            {
                await CleanupExpiredAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ошибка в цикле очистки истекших рассылок");
        }
    }

    private async Task CleanupExpiredAsync()
    {
        var now = DateTime.UtcNow;

        var expiredBroadcasts = _pendingBroadcasts
            .Where(x => x.Value.ExpiresAt < now)
            .ToList();

        foreach (var (key, broadcast) in expiredBroadcasts)
        {
            if (_pendingBroadcasts.TryRemove(key, out _) == false)
            {
                continue;
            }

            _logger.LogInformation("Удалена истекшая рассылка {BroadcastId}", key);

            if (_notificationService != null)
            {
                await _notificationService.NotifyBroadcastExpiredAsync(key, broadcast.AdminUserId);
            }
        }
    }
}
