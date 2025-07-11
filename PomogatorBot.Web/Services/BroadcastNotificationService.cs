using PomogatorBot.Web.Common.Constants;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

public sealed class BroadcastNotificationService(
    ITelegramBotClient botClient,
    ILogger<BroadcastNotificationService> logger) : BackgroundService
{
    private static readonly LinkPreviewOptions LinkPreviewOptions = new()
    {
        IsDisabled = true,
    };

    private readonly ConcurrentDictionary<string, BroadcastNotificationInfo> _notifications = new();
    private readonly PeriodicTimer _notificationTimer = new(TimeSpan.FromMinutes(1));

    public override void Dispose()
    {
        _notificationTimer.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public void AddNotification(
        string broadcastId,
        long adminUserId,
        long chatId,
        int messageId,
        DateTime createdAt,
        DateTime expiresAt,
        string originalConfirmationMessage,
        MessageEntity[]? originalConfirmationEntities,
        InlineKeyboardMarkup? originalConfirmationKeyboard)
    {
        var notification = new BroadcastNotificationInfo
        {
            BroadcastId = broadcastId,
            ChatId = chatId,
            MessageId = messageId,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
            OriginalConfirmationMessage = originalConfirmationMessage,
            OriginalConfirmationEntities = originalConfirmationEntities,
            OriginalConfirmationKeyboard = originalConfirmationKeyboard,
            NextReminderAt = createdAt.AddSeconds(30),
            ReminderInterval = TimeSpan.FromSeconds(30),
        };

        _notifications[broadcastId] = notification;

        logger.LogInformation("Добавлено уведомление для рассылки {BroadcastId} администратору {AdminUserId}",
            broadcastId, adminUserId);

        // TODO: Подумать
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30));
                await ProcessSingleNotification(notification, DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при отправке первого уведомления для рассылки {BroadcastId}", broadcastId);
            }
        });
    }

    public void RemoveNotification(string broadcastId)
    {
        if (_notifications.TryRemove(broadcastId, out var notification) == false)
        {
            return;
        }

        notification.IsConfirmed = true;
        logger.LogInformation("Удалено уведомление для подтвержденной рассылки {BroadcastId}", broadcastId);
    }

    public async Task NotifyBroadcastExpiredAsync(string broadcastId, long adminUserId)
    {
        try
        {
            var message = FormatExpiredNotificationMessage();

            await botClient.SendMessage(adminUserId, message,
                linkPreviewOptions: LinkPreviewOptions,
                cancellationToken: CancellationToken.None);

            logger.LogInformation("Отправлено уведомление об удалении рассылки {BroadcastId} администратору {AdminUserId}",
                broadcastId, adminUserId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ошибка при отправке уведомления об удалении рассылки {BroadcastId}", broadcastId);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Сервис уведомлений о рассылках запущен");

        try
        {
            while (await _notificationTimer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessNotificationsAsync();
            }
        }
        catch (OperationCanceledException exception)
        {
            logger.LogInformation(exception, "Сервис уведомлений о рассылках остановлен");
        }
    }

    private static string FormatReminderMessage(TimeSpan waitingTime, TimeSpan timeUntilExpiry)
    {
        var waitingMinutes = (int)waitingTime.TotalMinutes;
        var expiryMinutes = (int)timeUntilExpiry.TotalMinutes;

        return $"""
                {Emoji.Bell} Напоминание о рассылке

                {Emoji.Clock} Рассылка ожидает подтверждения уже {waitingMinutes} мин
                {Emoji.Warning} Автоматическое удаление через {expiryMinutes} мин
                """;
    }

    private static string FormatPreWarningMessage(TimeSpan timeUntilExpiry)
    {
        var expiryMinutes = (int)timeUntilExpiry.TotalMinutes;

        return $"""
                {Emoji.Alert} Предупреждение об удалении

                {Emoji.Clock} Рассылка будет удалена через {expiryMinutes} мин!
                """;
    }

    private static string FormatExpiredNotificationMessage()
    {
        return $"""
                {Emoji.Error} Рассылка удалена

                {Emoji.Clock} Неподтвержденная рассылка была автоматически удалена
                """;
    }

    private async Task ProcessNotificationsAsync()
    {
        var now = DateTime.UtcNow;

        var notificationsToProcess = _notifications.Values
            .Where(x => x.IsConfirmed == false && now >= x.NextReminderAt)
            .ToList();

        foreach (var notification in notificationsToProcess)
        {
            try
            {
                await ProcessSingleNotification(notification, now);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка при обработке уведомления для рассылки {BroadcastId}", notification.BroadcastId);
            }
        }

        var expiredNotifications = _notifications
            .Where(x => x.Value.IsConfirmed || x.Value.ExpiresAt < now)
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expiredNotifications)
        {
            _notifications.TryRemove(key, out _);
        }
    }

    private async Task ProcessSingleNotification(BroadcastNotificationInfo notification, DateTime now)
    {
        if (now >= notification.ExpiresAt)
        {
            return;
        }

        var timeUntilExpiry = notification.ExpiresAt - now;

        if (notification.PreWarningNotificationSent == false && timeUntilExpiry <= TimeSpan.FromMinutes(5))
        {
            await SendPreWarningNotification(notification, timeUntilExpiry);
            notification.PreWarningNotificationSent = true;
            return;
        }

        if (timeUntilExpiry > TimeSpan.FromMinutes(5))
        {
            await SendReminderNotification(notification, now, timeUntilExpiry);

            notification.NextReminderAt = now.Add(notification.ReminderInterval);
            notification.ReminderInterval = TimeSpan.FromMinutes(Math.Min(notification.ReminderInterval.TotalMinutes * 2, 32));
        }
    }

    private async Task SendReminderNotification(BroadcastNotificationInfo notification, DateTime now, TimeSpan timeUntilExpiry)
    {
        var waitingTime = now - notification.CreatedAt;
        var message = FormatReminderMessage(waitingTime, timeUntilExpiry);

        await UpdateNotificationMessage(notification, message);

        logger.LogInformation("Отправлено напоминание о рассылке {BroadcastId} (ожидает {WaitingMinutes} мин)",
            notification.BroadcastId, (int)waitingTime.TotalMinutes);
    }

    private async Task SendPreWarningNotification(BroadcastNotificationInfo notification, TimeSpan timeUntilExpiry)
    {
        var message = FormatPreWarningMessage(timeUntilExpiry);

        await UpdateNotificationMessage(notification, message);

        logger.LogInformation("Отправлено предупреждение об удалении рассылки {BroadcastId} через {Minutes} мин",
            notification.BroadcastId, (int)timeUntilExpiry.TotalMinutes);
    }

    private async Task UpdateNotificationMessage(BroadcastNotificationInfo notification, string notificationText)
    {
        var separator = "\n\n━━━━━\n";
        var modifiedMessage = $"{notificationText}{separator}{notification.OriginalConfirmationMessage}";

        var prefixLength = notificationText.Length + separator.Length;
        var adjustedEntities = MessageEntityHelper.OffsetEntities(notification.OriginalConfirmationEntities, prefixLength);

        if (notification.MessageId > 0)
        {
            try
            {
                await botClient.DeleteMessage(notification.ChatId, notification.MessageId);
            }
            catch (ApiRequestException exception) when (exception.Message.Contains("message to delete not found", StringComparison.OrdinalIgnoreCase))
            {
            }
        }

        try
        {
            await SendNewNotificationMessage(notification, modifiedMessage, adjustedEntities);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ошибка при обновлении уведомления для рассылки {BroadcastId}",
                notification.BroadcastId);
        }
    }

    private async Task SendNewNotificationMessage(BroadcastNotificationInfo notification, string message, MessageEntity[]? entities)
    {
        var sentMessage = await botClient.SendMessage(notification.ChatId,
            message,
            entities: entities,
            replyMarkup: notification.OriginalConfirmationKeyboard,
            linkPreviewOptions: LinkPreviewOptions,
            cancellationToken: CancellationToken.None);

        notification.MessageId = sentMessage.MessageId;
    }
}
