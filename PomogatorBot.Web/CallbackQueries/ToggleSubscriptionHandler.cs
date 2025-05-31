using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.CallbackQueries;

public class ToggleSubscriptionHandler(UserService userService, ILogger<ToggleSubscriptionHandler> logger) : ICallbackQueryHandler
{
    private const string TogglePrefix = "toggle_";

    public static string GetFormatedToggle(Subscribes subscription)
    {
        return TogglePrefix + subscription;
    }

    public bool CanHandle(string callbackData)
    {
        return callbackData.StartsWith(TogglePrefix, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var subscriptionName = callbackQuery.Data!.Split('_')[1];

        if (Enum.TryParse<Subscribes>(subscriptionName, out var subscription) == false)
        {
            logger.LogWarning("Unknown subscription: {Subscription}", subscriptionName);
            return new("Неизвестный тип подписки");
        }

        var user = await userService.GetAsync(userId, cancellationToken);

        if (user == null)
        {
            return new(Messages.JoinBefore);
        }

        user.Subscriptions = subscription switch
        {
            Subscribes.All => Subscribes.All,
            Subscribes.None => Subscribes.None,
            _ => user.Subscriptions ^= subscription,
        };

        await userService.SaveAsync(user, cancellationToken);

        logger.LogInformation("Toggled subscription {Subscription} for user {UserId}", subscription, userId);

        return new(string.Empty);
    }
}
