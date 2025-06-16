using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using DatabaseUser = PomogatorBot.Web.Infrastructure.Entities.User;

namespace PomogatorBot.Web.CallbackQueries;

public class ToggleSubscriptionHandler(
    UserService userService,
    ILogger<ToggleSubscriptionHandler> logger)
    : UserRequiredCallbackQueryHandler(userService)
{
    private const string TogglePrefix = "toggle_";

    public static string GetFormatedToggle(Subscribes subscription)
    {
        return TogglePrefix + subscription;
    }

    public override bool CanHandle(string callbackData)
    {
        return callbackData.StartsWith(TogglePrefix, StringComparison.OrdinalIgnoreCase);
    }

    protected override async Task<BotResponse> HandleUserCallbackAsync(CallbackQuery callbackQuery, DatabaseUser user, CancellationToken cancellationToken)
    {
        var subscriptionName = callbackQuery.Data!.Split('_')[1];

        if (Enum.TryParse<Subscribes>(subscriptionName, out var subscription) == false)
        {
            logger.LogWarning("Unknown subscription: {Subscription}", subscriptionName);
            return new("Неизвестный тип подписки");
        }

        user.Subscriptions = subscription switch
        {
            Subscribes.All => Subscribes.All,
            Subscribes.None => Subscribes.None,
            _ => user.Subscriptions ^ subscription,
        };

        await UserService.SaveAsync(user, cancellationToken);

        logger.LogInformation("Toggled subscription {Subscription} for user {UserId}", subscription, user.UserId);

        return new(string.Empty);
    }
}
