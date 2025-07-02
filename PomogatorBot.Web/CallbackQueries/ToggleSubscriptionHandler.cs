using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Constants;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.CallbackQueries;

public class ToggleSubscriptionHandler(
    UserService userService,
    ILogger<ToggleSubscriptionHandler> logger)
    : UserRequiredCallbackQueryHandler(userService)
{
    private const string TogglePrefix = "toggle_";

    public static string GetFormatedToggle(Subscribes subscription)
    {
        return CallbackDataParser.CreateWithPrefix(TogglePrefix, subscription.ToString());
    }

    public override bool CanHandle(string callbackData)
    {
        return CallbackDataParser.TryParseWithPrefix(callbackData, TogglePrefix, out _);
    }

    protected override async Task<BotResponse> HandleUserCallbackAsync(CallbackQuery callbackQuery, PomogatorUser user, CancellationToken cancellationToken)
    {
        if (CallbackDataParser.TryParseWithPrefix(callbackQuery.Data!, TogglePrefix, out var subscriptionName) == false)
        {
            logger.LogWarning("Неверный формат данных колбэка: {CallbackData}", callbackQuery.Data);
            return new($"{Emoji.Error} Неверный формат данных");
        }

        if (Enum.TryParse<Subscribes>(subscriptionName, out var subscription) == false)
        {
            logger.LogWarning("Неизвестный тип подписки: {Subscription}", subscriptionName);
            return new($"{Emoji.Question} Неизвестный тип подписки");
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
