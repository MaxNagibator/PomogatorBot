using PomogatorBot.Web.CallbackQueries;
using PomogatorBot.Web.Commands;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

public class KeyboardFactory(UserService userService)
{
    public static InlineKeyboardButton CreateCallbackButton(string icon, string text, string callbackData)
    {
        return InlineKeyboardButton.WithCallbackData($"{icon} {text}", callbackData);
    }

    public static InlineKeyboardButton CreateCallbackButton(string text, string callbackData)
    {
        return InlineKeyboardButton.WithCallbackData(text, callbackData);
    }

    public static InlineKeyboardButton[] CreateButtonRow(InlineKeyboardButton leftButton, InlineKeyboardButton rightButton)
    {
        return [leftButton, rightButton];
    }

    public static InlineKeyboardButton[] CreateButtonRow(InlineKeyboardButton button)
    {
        return [button];
    }

    public static InlineKeyboardButton[] CreateConfirmationRow(string confirmText, string confirmCallback, string cancelText, string cancelCallback)
    {
        return
        [
            InlineKeyboardButton.WithCallbackData(confirmText, confirmCallback),
            InlineKeyboardButton.WithCallbackData(cancelText, cancelCallback),
        ];
    }

    public static InlineKeyboardButton CreateBackButton(string callbackData)
    {
        return InlineKeyboardButton.WithCallbackData("🔙 Назад", callbackData);
    }

    public InlineKeyboardMarkup CreateForSubscriptions(Subscribes subscriptions)
    {
        var buttons = SubscriptionExtensions.SubscriptionMetadata
            .Values
            .Where(x => x.Subscription is not Subscribes.None and not Subscribes.All)
            .Select(x => MakeSubscriptionButton(x, subscriptions))
            .Select(x => new[] { x })
            .ToList();

        buttons.Add(CreateButtonRow(CreateCallbackButton("✅", "Включить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.All)),
            CreateCallbackButton("❌", "Выключить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.None))));

        buttons.Add(CreateButtonRow(CreateBackButton(NavigationHandler.MenuBack)));

        return new(buttons);
    }

    public async Task<InlineKeyboardMarkup> CreateForWelcome(long? userId = null, CancellationToken cancellationToken = default)
    {
        List<InlineKeyboardButton[]> buttons = [];
        var exists = userId != null && await userService.ExistsAsync(userId.Value, cancellationToken);

        if (exists)
        {
            buttons.Add(CreateButtonRow(CreateCallbackButton("📌", "Мой профиль", MeCommandHandler.Metadata.Command),
                CreateCallbackButton("🚪", "Покинуть", LeaveCommandHandler.Metadata.Command)));

            buttons.Add(CreateButtonRow(CreateCallbackButton("🎚️", "Управление подписками", SubscriptionsCommandHandler.Metadata.Command),
                CreateCallbackButton("❓", "Помощь", HelpCommandHandler.Metadata.Command)));
        }
        else
        {
            buttons.Add(CreateButtonRow(CreateCallbackButton("🎯", "Присоединиться", JoinCommandHandler.Metadata.Command)));
            buttons.Add(CreateButtonRow(CreateCallbackButton("❓", "Помощь", HelpCommandHandler.Metadata.Command)));
        }

        return new(buttons);
    }

    public InlineKeyboardMarkup CreateForBroadcastConfirmation(string pendingId)
    {
        List<InlineKeyboardButton[]> buttons =
        [
            CreateConfirmationRow("✅ Подтвердить рассылку", $"broadcast_confirm_{pendingId}",
                "❌ Отменить", $"broadcast_cancel_{pendingId}"),
            CreateButtonRow(CreateCallbackButton("📋", "Показать подписки", $"broadcast_show_subs_{pendingId}")),
        ];

        return new(buttons);
    }

    private static InlineKeyboardButton MakeSubscriptionButton(SubscriptionMeta meta, Subscribes current)
    {
        var isActive = current.HasFlag(meta.Subscription);
        var buttonText = $"{meta.Icon} {meta.DisplayName} {(isActive ? "✅" : "❌")}";
        return InlineKeyboardButton.WithCallbackData(buttonText, ToggleSubscriptionHandler.GetFormatedToggle(meta.Subscription));
    }
}
