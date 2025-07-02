using Microsoft.EntityFrameworkCore;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Infrastructure.Entities;

namespace PomogatorBot.Web.Services;

public class BroadcastHistoryService(
    ApplicationDbContext dbContext,
    ILogger<BroadcastHistoryService> logger)
{
    public async Task<BroadcastHistory> StartAsync(
        string messageText,
        long? adminUserId,
        int totalRecipients,
        MessageEntity[]? messageEntities = null,
        CancellationToken cancellationToken = default)
    {
        var broadcast = new BroadcastHistory
        {
            MessageText = messageText,
            MessageEntities = messageEntities,
            AdminUserId = adminUserId,
            TotalRecipients = totalRecipients,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.InProgress,
            SuccessfulDeliveries = 0,
            FailedDeliveries = 0,
        };

        dbContext.BroadcastHistory.Add(broadcast);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Начата рассылка ID {BroadcastId} для {TotalRecipients} получателей администратором {AdminUserId}",
            broadcast.Id, totalRecipients, adminUserId);

        return broadcast;
    }

    public async Task UpdateProgressAsync(
        long broadcastId,
        int successCount,
        int failedCount,
        CancellationToken cancellationToken = default)
    {
        var broadcast = await dbContext.BroadcastHistory.FindAsync([broadcastId], cancellationToken);

        if (broadcast == null)
        {
            logger.LogWarning("Рассылка с ID {BroadcastId} не найдена для обновления прогресса", broadcastId);
            return;
        }

        broadcast.SuccessfulDeliveries = successCount;
        broadcast.FailedDeliveries = failedCount;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAsync(
        long broadcastId,
        int finalSuccessCount,
        int finalFailedCount,
        string? errorSummary = null,
        CancellationToken cancellationToken = default)
    {
        var broadcast = await dbContext.BroadcastHistory.FindAsync([broadcastId], cancellationToken);

        if (broadcast == null)
        {
            logger.LogWarning("Рассылка с ID {BroadcastId} не найдена для завершения", broadcastId);
            return;
        }

        broadcast.SuccessfulDeliveries = finalSuccessCount;
        broadcast.FailedDeliveries = finalFailedCount;
        broadcast.CompletedAt = DateTime.UtcNow;

        broadcast.Status = finalSuccessCount + finalFailedCount == broadcast.TotalRecipients
            ? BroadcastStatus.Completed
            : BroadcastStatus.Failed;

        broadcast.ErrorSummary = errorSummary;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Завершена рассылка ID {BroadcastId}: {SuccessCount} успешно, {FailedCount} неуспешно",
            broadcastId, finalSuccessCount, finalFailedCount);
    }

    public Task<List<BroadcastHistory>> GetLastsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
        {
            count = 1;
        }

        if (count > 100)
        {
            count = 100;
        }

        return dbContext.BroadcastHistory
            .OrderByDescending(x => x.StartedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<BroadcastStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var broadcasts = await dbContext.BroadcastHistory.ToListAsync(cancellationToken);

        return new()
        {
            Total = broadcasts.Count,
            Completed = broadcasts.Count(x => x.Status == BroadcastStatus.Completed),
            InProgress = broadcasts.Count(x => x.Status == BroadcastStatus.InProgress),
            Failed = broadcasts.Count(x => x.Status == BroadcastStatus.Failed),
            TotalMessagesSent = broadcasts.Sum(x => x.SuccessfulDeliveries + x.FailedDeliveries),
            TotalSuccessfulDeliveries = broadcasts.Sum(x => x.SuccessfulDeliveries),
            TotalFailedDeliveries = broadcasts.Sum(x => x.FailedDeliveries),
        };
    }

    public async Task<int> ClearHistoryAsync(int? olderThanDays = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.BroadcastHistory.AsQueryable();

        if (olderThanDays.HasValue)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays.Value);
            query = query.Where(x => x.StartedAt < cutoffDate);
        }

        var broadcastsToDelete = await query.ToListAsync(cancellationToken);
        var count = broadcastsToDelete.Count;

        if (count > 0)
        {
            dbContext.BroadcastHistory.RemoveRange(broadcastsToDelete);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Удалено {Count} записей из истории рассылок", count);
        }

        return count;
    }
}
