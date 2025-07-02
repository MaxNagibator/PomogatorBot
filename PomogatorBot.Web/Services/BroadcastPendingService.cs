using System.Collections.Concurrent;

namespace PomogatorBot.Web.Services;

public sealed class BroadcastPendingService : IDisposable
{
    private readonly ConcurrentDictionary<string, PendingBroadcast> _pendingBroadcasts = new();
    private readonly Timer _cleanupTimer;

    public BroadcastPendingService()
    {
        _cleanupTimer = new(CleanupExpiredBroadcasts, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public string StorePendingBroadcast(
        string message,
        Subscribes subscribes,
        MessageEntity[]? entities,
        long adminUserId)
    {
        var pendingId = Guid.NewGuid().ToString("N")[..8];

        var pendingBroadcast = new PendingBroadcast
        {
            Id = pendingId,
            Message = message,
            Subscribes = subscribes,
            Entities = entities,
            AdminUserId = adminUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        };

        _pendingBroadcasts[pendingId] = pendingBroadcast;
        return pendingId;
    }

    public PendingBroadcast? GetPendingBroadcast(string pendingId)
    {
        _pendingBroadcasts.TryGetValue(pendingId, out var pendingBroadcast);

        if (pendingBroadcast != null && pendingBroadcast.ExpiresAt < DateTime.UtcNow)
        {
            _pendingBroadcasts.TryRemove(pendingId, out _);
            return null;
        }

        return pendingBroadcast;
    }

    public bool RemovePendingBroadcast(string pendingId)
    {
        return _pendingBroadcasts.TryRemove(pendingId, out _);
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void CleanupExpiredBroadcasts(object? state)
    {
        var now = DateTime.UtcNow;

        var expiredKeys = _pendingBroadcasts
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _pendingBroadcasts.TryRemove(key, out _);
        }
    }
}
