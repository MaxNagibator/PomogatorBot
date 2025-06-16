using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.CallbackQueries;

public class BroadcastConfirmationHandler(
    BroadcastPendingService broadcastPendingService,
    UserService userService,
    KeyboardFactory keyboardFactory,
    MessagePreviewService messagePreviewService,
    ILogger<BroadcastConfirmationHandler> logger)
    : ICallbackQueryHandler
{
    private const string ConfirmPrefix = "broadcast_confirm_";
    private const string CancelPrefix = "broadcast_cancel_";
    private const string ShowSubsPrefix = "broadcast_show_subs_";

    public bool CanHandle(string callbackData)
    {
        return callbackData.StartsWith(ConfirmPrefix, StringComparison.OrdinalIgnoreCase)
               || callbackData.StartsWith(CancelPrefix, StringComparison.OrdinalIgnoreCase)
               || callbackData.StartsWith(ShowSubsPrefix, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var callbackData = callbackQuery.Data!;

        string pendingId;
        string action;

        if (callbackData.StartsWith(ConfirmPrefix, StringComparison.OrdinalIgnoreCase))
        {
            action = "confirm";
            pendingId = callbackData[ConfirmPrefix.Length..];
        }
        else if (callbackData.StartsWith(CancelPrefix, StringComparison.OrdinalIgnoreCase))
        {
            action = "cancel";
            pendingId = callbackData[CancelPrefix.Length..];
        }
        else if (callbackData.StartsWith(ShowSubsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            action = "show_subs";
            pendingId = callbackData[ShowSubsPrefix.Length..];
        }
        else
        {
            logger.LogWarning("Unknown broadcast callback action: {CallbackData}", callbackData);
            return new("Неизвестное действие");
        }

        var pendingBroadcast = broadcastPendingService.GetPendingBroadcast(pendingId);

        if (pendingBroadcast == null)
        {
            return new("⚠️ Рассылка не найдена или истекла. Попробуйте создать новую рассылку.");
        }

        if (pendingBroadcast.AdminUserId != userId)
        {
            logger.LogWarning("User {UserId} tried to access broadcast created by {AdminUserId}", userId, pendingBroadcast.AdminUserId);
            return new("❌ У вас нет прав для выполнения этого действия.");
        }

        return action switch
        {
            "confirm" => await HandleConfirmBroadcast(pendingBroadcast, cancellationToken),
            "cancel" => HandleCancelBroadcast(pendingBroadcast),
            "show_subs" => await HandleShowSubscriptions(pendingBroadcast, cancellationToken),
            _ => new("Неизвестное действие"),
        };
    }

    private static string GetSubscriptionDisplayInfo(Subscribes subscribes)
    {
        if (subscribes == Subscribes.None)
        {
            return "Всем пользователям";
        }

        var metadata = SubscriptionExtensions.SubscriptionMetadata;

        var activeSubscriptions = metadata.Values
            .Where(x => x.Subscription != Subscribes.None
                        && x.Subscription != Subscribes.All
                        && subscribes.HasFlag(x.Subscription))
            .Select(x => $"{x.Icon} {x.DisplayName}")
            .ToList();

        return activeSubscriptions.Count > 0
            ? string.Join(", ", activeSubscriptions)
            : "Всем пользователям";
    }

    private static MessageEntity[]? AdjustEntitiesForMessage(MessageEntity[]? entities, int offset)
    {
        if (entities == null || entities.Length == 0)
        {
            return null;
        }

        return entities.Select(x => new MessageEntity
            {
                Type = x.Type,
                Offset = x.Offset + offset,
                Length = x.Length,
                Url = x.Url,
                User = x.User,
                Language = x.Language,
                CustomEmojiId = x.CustomEmojiId,
            })
            .ToArray();
    }

    private async Task<BotResponse> HandleConfirmBroadcast(PendingBroadcast pendingBroadcast, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Executing broadcast {BroadcastId} for admin {AdminUserId}",
                pendingBroadcast.Id, pendingBroadcast.AdminUserId);

            var response = await userService.NotifyAsync(pendingBroadcast.Message,
                pendingBroadcast.Subscribes,
                pendingBroadcast.Entities,
                cancellationToken);

            broadcastPendingService.RemovePendingBroadcast(pendingBroadcast.Id);

            var successMessage = $"""
                                  ✅ Рассылка успешно выполнена!

                                  📊 Статистика:
                                  👥 Всего пользователей: {response.TotalUsers}
                                  ✅ Успешно отправлено: {response.SuccessfulSends}
                                  ❌ С ошибкой: {response.FailedSends}

                                  📝 Сообщение: {pendingBroadcast.Message}
                                  """;

            logger.LogInformation("Broadcast {BroadcastId} completed successfully. Success: {Success}, Failed: {Failed}, Total: {Total}",
                pendingBroadcast.Id, response.SuccessfulSends, response.FailedSends, response.TotalUsers);

            return new(successMessage);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error executing broadcast {BroadcastId}", pendingBroadcast.Id);
            return new("❌ Произошла ошибка при выполнении рассылки. Попробуйте еще раз.");
        }
    }

    private BotResponse HandleCancelBroadcast(PendingBroadcast pendingBroadcast)
    {
        broadcastPendingService.RemovePendingBroadcast(pendingBroadcast.Id);

        logger.LogInformation("Broadcast {BroadcastId} cancelled by admin {AdminUserId}",
            pendingBroadcast.Id, pendingBroadcast.AdminUserId);

        return new("❌ Рассылка отменена.");
    }

    private async Task<BotResponse> HandleShowSubscriptions(PendingBroadcast pendingBroadcast, CancellationToken cancellationToken)
    {
        var userCount = await userService.GetUserCountBySubscriptionAsync(pendingBroadcast.Subscribes, cancellationToken);
        var subscriptionInfo = GetSubscriptionDisplayInfo(pendingBroadcast.Subscribes);
        var preview = messagePreviewService.CreatePreview(pendingBroadcast.Message, pendingBroadcast.Entities);

        var messageHeader = $"""
                             📋 Детальная информация о рассылке:

                             🎯 Целевые подписки: {subscriptionInfo}
                             👥 Количество получателей: {userCount}

                             📋 Предварительный просмотр (как увидят пользователи):
                             ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                             """;

        var messageFooter = """
                            ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

                            ⚠️ Выберите действие:
                            """;

        var message = messageHeader + preview.PreviewText + messageFooter;
        var adjustedEntities = AdjustEntitiesForMessage(preview.PreviewEntities, messageHeader.Length);
        var keyboard = keyboardFactory.CreateForBroadcastConfirmation(pendingBroadcast.Id);

        return new(message, keyboard, adjustedEntities);
    }
}
