using PomogatorBot.Web.CallbackQueries;
using PomogatorBot.Web.Commands;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

public class KeyboardFactory(UserService userService)
{
    public static InlineKeyboardButton[] CreateConfirmationRow(string confirmText, string confirmCallback, string cancelText, string cancelCallback)
    {
        return
        [
            InlineKeyboardButton.WithCallbackData(confirmText, confirmCallback),
            InlineKeyboardButton.WithCallbackData(cancelText, cancelCallback),
        ];
    }

    public InlineKeyboardMarkup CreateForSubscriptions(Subscribes subscriptions)
    {
        var builder = KeyboardBuilder.Create();

        var subscriptionMetas = SubscriptionExtensions.SubscriptionMetadata
            .Values
            .Where(x => x.Subscription is not Subscribes.None and not Subscribes.All);

        foreach (var meta in subscriptionMetas)
        {
            builder.AddSubscriptionButton(meta, subscriptions);
        }

        builder.AddButtonRow(("✅ Включить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.All)),
                ("❌ Выключить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.None)))
            .AddButton("🔙 Назад", NavigationHandler.MenuBack);

        return builder.Build();
    }

    public async Task<InlineKeyboardMarkup> CreateForWelcome(long? userId = null, CancellationToken cancellationToken = default)
    {
        var builder = KeyboardBuilder.Create();
        var exists = userId != null && await userService.ExistsAsync(userId.Value, cancellationToken);

        if (exists)
        {
            builder.AddButtonRow(("📌 Мой профиль", MeCommandHandler.Metadata.Command),
                ("🚪 Покинуть", LeaveCommandHandler.Metadata.Command));

            builder.AddButtonRow(("🎚️ Управление подписками", SubscriptionsCommandHandler.Metadata.Command),
                ("❓ Помощь", HelpCommandHandler.Metadata.Command));
        }
        else
        {
            builder.AddButton("🎯 Присоединиться", JoinCommandHandler.Metadata.Command)
                .AddButton("❓ Помощь", HelpCommandHandler.Metadata.Command);
        }

        return builder.Build();
    }

    public InlineKeyboardMarkup CreateForBroadcastConfirmation(string pendingId)
    {
        var builder = KeyboardBuilder.Create();

        builder.AddButtonRow(("✅ Подтвердить рассылку", BroadcastConfirmationHandler.ConfirmPrefix + pendingId),
            ("❌ Отменить", BroadcastConfirmationHandler.CancelPrefix + pendingId));

        return builder.Build();
    }
}
