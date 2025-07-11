using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Common.Constants;

namespace PomogatorBot.Web.CallbackQueries;

public class BroadcastConfirmationHandler(
    BroadcastPendingService broadcastPendingService,
    BroadcastExecutionService broadcastExecutionService,
    BroadcastNotificationService notificationService,
    ILogger<BroadcastConfirmationHandler> logger) : ICallbackQueryHandler
{
    public const string ConfirmPrefix = "broadcast_confirm_";
    public const string CancelPrefix = "broadcast_cancel_";

    private static readonly IReadOnlyDictionary<string, string> PrefixActions = new Dictionary<string, string>
    {
        { ConfirmPrefix, "confirm" },
        { CancelPrefix, "cancel" },
    };

    public bool CanHandle(string callbackData)
    {
        return CallbackDataParser.TryParseWithMultiplePrefixes(callbackData, PrefixActions, out _, out _);
    }

    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var callbackData = callbackQuery.Data!;

        if (CallbackDataParser.TryParseWithMultiplePrefixes(callbackData, PrefixActions, out var action, out var pendingId) == false)
        {
            logger.LogWarning("Неизвестное действие callback рассылки: {CallbackData}", callbackData);
            return new($"{Emoji.Question} Неизвестное действие");
        }

        var pendingBroadcast = broadcastPendingService.Get(pendingId);

        if (pendingBroadcast == null)
        {
            return new($"{Emoji.Warning} Рассылка не найдена или истекла. Попробуйте создать новую рассылку.");
        }

        if (pendingBroadcast.AdminUserId != userId)
        {
            logger.LogWarning("Пользователь {UserId} попытался получить доступ к рассылке, созданной пользователем {AdminUserId}", userId, pendingBroadcast.AdminUserId);
            return new($"{Emoji.Error} У вас нет прав для выполнения этого действия.");
        }

        return action switch
        {
            "confirm" => HandleConfirmBroadcast(pendingBroadcast, callbackQuery),
            "cancel" => HandleCancelBroadcast(pendingBroadcast),
            _ => new($"{Emoji.Question} Неизвестное действие"),
        };
    }

    private BotResponse HandleConfirmBroadcast(PendingBroadcast pendingBroadcast, CallbackQuery callbackQuery)
    {
        notificationService.Remove(pendingBroadcast.Id);

        if (callbackQuery.Message == null)
        {
            logger.LogWarning("CallbackQuery.Message равно null для рассылки {BroadcastId}", pendingBroadcast.Id);
            return new($"{Emoji.Error} Ошибка обработки запроса. Попробуйте еще раз.");
        }

        var chatId = callbackQuery.Message.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        logger.LogInformation("Постановка рассылки {BroadcastId} в очередь для администратора {AdminUserId} в чате {ChatId}",
            pendingBroadcast.Id, pendingBroadcast.AdminUserId, chatId);

        var broadcastTask = new BroadcastTask
        {
            BroadcastId = pendingBroadcast.Id,
            Message = pendingBroadcast.Message,
            Subscribes = pendingBroadcast.Subscribes,
            Entities = pendingBroadcast.Entities,
            AdminUserId = pendingBroadcast.AdminUserId,
            ChatId = chatId,
            MessageId = messageId,
        };

        _ = broadcastExecutionService.EnqueueAsync(broadcastTask, CancellationToken.None);

        return new($"{Emoji.Refresh} Рассылка поставлена в очередь на выполнение...");
    }

    private BotResponse HandleCancelBroadcast(PendingBroadcast pendingBroadcast)
    {
        notificationService.Remove(pendingBroadcast.Id);
        broadcastPendingService.Remove(pendingBroadcast.Id);

        logger.LogInformation("Рассылка {BroadcastId} отменена администратором {AdminUserId}",
            pendingBroadcast.Id, pendingBroadcast.AdminUserId);

        return new($"{Emoji.Error} Рассылка отменена.");
    }
}
