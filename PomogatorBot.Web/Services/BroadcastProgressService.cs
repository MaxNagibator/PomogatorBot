using PomogatorBot.Web.Common.Constants;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace PomogatorBot.Web.Services;

public sealed class BroadcastProgressService : IDisposable
{
    private static readonly LinkPreviewOptions LinkPreviewOptions = new()
    {
        IsDisabled = true,
    };

    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BroadcastProgressService> _logger;
    private readonly ConcurrentDictionary<string, BroadcastProgressInfo> _activeProgresses = new();
    private readonly Timer _cleanupTimer;

    public BroadcastProgressService(ITelegramBotClient botClient, ILogger<BroadcastProgressService> logger)
    {
        _botClient = botClient;
        _logger = logger;
        _cleanupTimer = new(CleanupExpiredProgresses, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public void StartProgress(string broadcastId, long chatId, int messageId, int totalUsers)
    {
        var progressInfo = new BroadcastProgressInfo
        {
            BroadcastId = broadcastId,
            ChatId = chatId,
            MessageId = messageId,
            TotalUsers = totalUsers,
            StartedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        _activeProgresses[broadcastId] = progressInfo;
        _logger.LogInformation("Начато отслеживание прогресса рассылки {BroadcastId} в чате {ChatId}", broadcastId, chatId);
    }

    public async Task UpdatePreparationStageAsync(string broadcastId, CancellationToken cancellationToken = default)
    {
        if (_activeProgresses.TryGetValue(broadcastId, out var progressInfo) == false)
        {
            return;
        }

        var message = $"{Emoji.Refresh} Подготовка рассылки...";
        await UpdateMessageSafelyAsync(progressInfo, message, cancellationToken);
    }

    public async Task UpdateSendingProgressAsync(string broadcastId, int successfulSends, int failedSends, CancellationToken cancellationToken = default)
    {
        if (_activeProgresses.TryGetValue(broadcastId, out var progressInfo) == false)
        {
            return;
        }

        var totalProcessed = successfulSends + failedSends;

        var message = $"""
                       {Emoji.Send} Отправка сообщений... ({totalProcessed} из {progressInfo.TotalUsers})

                       {Emoji.Success} Успешно: {successfulSends}
                       {Emoji.Error} С ошибкой: {failedSends}
                       """;

        await UpdateMessageSafelyAsync(progressInfo, message, cancellationToken);
    }

    public async Task CompleteAsync(string broadcastId, int successfulSends, int failedSends, int totalUsers, CancellationToken cancellationToken = default)
    {
        if (_activeProgresses.TryGetValue(broadcastId, out var progressInfo) == false)
        {
            return;
        }

        var message = $"""
                       {Emoji.Success} Рассылка завершена успешно!

                       {Emoji.Chart} Статистика:
                       {Emoji.Users} Всего пользователей: {totalUsers}
                       {Emoji.Success} Успешно отправлено: {successfulSends}
                       {Emoji.Error} С ошибкой: {failedSends}
                       """;

        await UpdateMessageSafelyAsync(progressInfo, message, cancellationToken);
        _activeProgresses.TryRemove(broadcastId, out _);

        _logger.LogInformation("Завершено отслеживание прогресса рассылки {BroadcastId}. Успешно: {Success}, Неуспешно: {Failed}",
            broadcastId, successfulSends, failedSends);
    }

    public async Task FailBroadcastAsync(string broadcastId, string errorMessage, CancellationToken cancellationToken = default)
    {
        if (_activeProgresses.TryGetValue(broadcastId, out var progressInfo) == false)
        {
            return;
        }

        var message = $"""
                       {Emoji.Error} Ошибка при рассылке

                       {Emoji.Warning} Произошла ошибка при выполнении рассылки. Попробуйте еще раз.
                       """;

        await UpdateMessageSafelyAsync(progressInfo, message, cancellationToken);
        _activeProgresses.TryRemove(broadcastId, out _);

        _logger.LogError("Ошибка при отслеживании прогресса рассылки {BroadcastId}: {Error}", broadcastId, errorMessage);
    }

    public void Dispose()
    {
        _cleanupTimer.Dispose();
    }

    private Task UpdateMessageSafelyAsync(BroadcastProgressInfo progressInfo, string message, CancellationToken cancellationToken)
    {
        try
        {
            return _botClient.EditMessageText(progressInfo.ChatId,
                progressInfo.MessageId,
                message,
                linkPreviewOptions: LinkPreviewOptions,
                cancellationToken: cancellationToken);
        }
        catch (ApiRequestException exception) when (exception.Message.Contains("message is not modified", StringComparison.OrdinalIgnoreCase))
        {
        }
        catch (ApiRequestException exception) when (exception.ErrorCode == 400 && exception.Message.Contains("message to edit not found", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(exception, "Сообщение для редактирования не найдено для рассылки {BroadcastId} в чате {ChatId}. Сообщение могло быть удалено.",
                progressInfo.BroadcastId, progressInfo.ChatId);

            _activeProgresses.TryRemove(progressInfo.BroadcastId, out _);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ошибка при обновлении сообщения с прогрессом рассылки {BroadcastId} в чате {ChatId}",
                progressInfo.BroadcastId, progressInfo.ChatId);
        }

        return Task.CompletedTask;
    }

    private void CleanupExpiredProgresses(object? state)
    {
        var now = DateTime.UtcNow;

        var expiredKeys = _activeProgresses
            .Where(x => x.Value.ExpiresAt < now)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            if (_activeProgresses.TryRemove(key, out var progressInfo))
            {
                _logger.LogInformation("Очищен истекший прогресс рассылки {BroadcastId}", progressInfo.BroadcastId);
            }
        }
    }
}
