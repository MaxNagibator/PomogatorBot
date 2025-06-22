using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.CallbackQueries;

public class BroadcastConfirmationHandler(
    BroadcastPendingService broadcastPendingService,
    UserService userService,
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
            logger.LogWarning("Unknown broadcast callback action: {CallbackData}", callbackData);
            return new("❓ Неизвестное действие");
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
            _ => new("❓ Неизвестное действие"),
        };
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
                pendingBroadcast.AdminUserId,
                cancellationToken);

            broadcastPendingService.RemovePendingBroadcast(pendingBroadcast.Id);

            var successMessage = $"""
                                  ✅ Рассылка успешно выполнена!

                                  📊 Статистика:
                                  👥 Всего пользователей: {response.TotalUsers}
                                  ✅ Успешно отправлено: {response.SuccessfulSends}
                                  ❌ С ошибкой: {response.FailedSends}
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
}
