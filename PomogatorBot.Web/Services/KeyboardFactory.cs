using PomogatorBot.Web.CallbackQueries;
using PomogatorBot.Web.Commands;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

public class KeyboardFactory(UserService userService)
{
    public InlineKeyboardMarkup CreateForSubscriptions(Subscribes subscriptions)
    {
        var buttons = SubscriptionExtensions.GetSubscriptionMetadata()
            .Values
            .Where(x => x.Subscription is not Subscribes.None and not Subscribes.All)
            .Select(x => MakeSubscriptionButton(x, subscriptions))
            .Select(x => new[] { x })
            .ToList();

        buttons.Add([
            InlineKeyboardButton.WithCallbackData("✅ Включить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.All)),
            InlineKeyboardButton.WithCallbackData("❌ Выключить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.None)),
        ]);

        buttons.Add([
            InlineKeyboardButton.WithCallbackData("🔙 Назад", NavigationHandler.MenuBack),
        ]);

        return new(buttons);
    }

    public async Task<InlineKeyboardMarkup> CreateForWelcome(long? userId = null, CancellationToken cancellationToken = default)
    {
        List<InlineKeyboardButton[]> buttons = [];
        var exists = userId != null && await userService.ExistsAsync(userId.Value, cancellationToken);

        if (exists)
        {
            buttons.Add([
                InlineKeyboardButton.WithCallbackData("📌 Мой профиль", MeCommandHandler.Metadata.Command),
                InlineKeyboardButton.WithCallbackData("🚪 Покинуть", LeaveCommandHandler.Metadata.Command),
            ]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData("🎚️ Управление подписками", SubscriptionsCommandHandler.Metadata.Command),
                InlineKeyboardButton.WithCallbackData("❓ Помощь", HelpCommandHandler.Metadata.Command),
            ]);
        }
        else
        {
            buttons.Add([
                InlineKeyboardButton.WithCallbackData("🎯 Присоединиться", JoinCommandHandler.Metadata.Command),
            ]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData("❓ Помощь", HelpCommandHandler.Metadata.Command),
            ]);
        }

        return new(buttons);
    }

    private static InlineKeyboardButton MakeSubscriptionButton(SubscriptionMeta meta, Subscribes current)
    {
        var isActive = current.HasFlag(meta.Subscription);
        var buttonText = $"{meta.Icon} {meta.DisplayName} {(isActive ? "✅" : "❌")}";
        return InlineKeyboardButton.WithCallbackData(buttonText, ToggleSubscriptionHandler.GetFormatedToggle(meta.Subscription));
    }
}
