using PomogatorBot.Web.CallbackQueries;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

public class KeyboardBuilder
{
    private readonly List<InlineKeyboardButton[]> _buttons = [];

    public static KeyboardBuilder Create()
    {
        return new();
    }

    public KeyboardBuilder AddButton(string text, string callbackData)
    {
        _buttons.Add([InlineKeyboardButton.WithCallbackData(text, callbackData)]);
        return this;
    }

    public KeyboardBuilder AddButtonRow(params (string text, string callbackData)[] buttons)
    {
        var buttonRow = buttons.Select(b => InlineKeyboardButton.WithCallbackData(b.text, b.callbackData)).ToArray();
        _buttons.Add(buttonRow);
        return this;
    }

    public KeyboardBuilder AddSubscriptionButton(SubscriptionMeta meta, Subscribes current)
    {
        var isActive = current.HasFlag(meta.Subscription);
        var buttonText = $"{meta.Icon} {meta.DisplayName} {(isActive ? "✅" : "❌")}";
        AddButton(buttonText, ToggleSubscriptionHandler.GetFormatedToggle(meta.Subscription));
        return this;
    }

    public InlineKeyboardMarkup Build()
    {
        return new(_buttons);
    }
}
