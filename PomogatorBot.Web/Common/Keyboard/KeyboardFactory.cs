using PomogatorBot.Web.CallbackQueries;
using PomogatorBot.Web.Commands;
using PomogatorBot.Web.Common.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Common.Keyboard;

public class KeyboardFactory(UserService userService)
{
    /// <summary>
    /// Создает строку кнопок подтверждения
    /// </summary>
    /// <param name="confirmText">Текст кнопки подтверждения</param>
    /// <param name="confirmCallback">Callback для подтверждения</param>
    /// <param name="cancelText">Текст кнопки отмены</param>
    /// <param name="cancelCallback">Callback для отмены</param>
    /// <returns>Массив кнопок</returns>
    public static InlineKeyboardButton[] CreateConfirmationRow(string confirmText, string confirmCallback, string cancelText, string cancelCallback)
    {
        return
        [
            InlineKeyboardButton.WithCallbackData(confirmText, confirmCallback),
            InlineKeyboardButton.WithCallbackData(cancelText, cancelCallback),
        ];
    }

    /// <summary>
    /// Создает callback-кнопку с иконкой и текстом
    /// </summary>
    /// <param name="icon">Иконка кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Кнопка клавиатуры</returns>
    public static InlineKeyboardButton CreateCallbackButton(string icon, string text, string callbackData)
    {
        var buttonText = string.IsNullOrEmpty(icon) ? text : $"{icon} {text}";
        return InlineKeyboardButton.WithCallbackData(buttonText, callbackData);
    }

    /// <summary>
    /// Создает callback-кнопку с текстом
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Кнопка клавиатуры</returns>
    public static InlineKeyboardButton CreateCallbackButton(string text, string callbackData)
    {
        return InlineKeyboardButton.WithCallbackData(text, callbackData);
    }

    /// <summary>
    /// Создает строку кнопок из переданных кнопок
    /// </summary>
    /// <param name="buttons">Кнопки для строки</param>
    /// <returns>Массив кнопок</returns>
    public static InlineKeyboardButton[] CreateButtonRow(params InlineKeyboardButton[] buttons)
    {
        return buttons;
    }

    /// <summary>
    /// Создает кнопку "Назад"
    /// </summary>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Кнопка "Назад"</returns>
    public static InlineKeyboardButton CreateBackButton(string callbackData)
    {
        return InlineKeyboardButton.WithCallbackData($"{Emoji.Back} Назад", callbackData);
    }

    /// <summary>
    /// Создает клавиатуру для управления подписками
    /// </summary>
    /// <param name="subscriptions">Текущие подписки пользователя</param>
    /// <returns>Клавиатура управления подписками</returns>
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

        return builder
            .Grid()
            .AddButton(Emoji.Success, "Включить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.All))
            .AddButton(Emoji.Error, "Выключить все", ToggleSubscriptionHandler.GetFormatedToggle(Subscribes.None))
            .End()
            .AddButton(Emoji.Back, "Назад", NavigationHandler.MenuBack)
            .Build();
    }

    /// <summary>
    /// Создает приветственную клавиатуру
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Приветственная клавиатура</returns>
    public async Task<InlineKeyboardMarkup> CreateForWelcome(long? userId = null, CancellationToken cancellationToken = default)
    {
        var exists = userId != null && await userService.ExistsAsync(userId.Value, cancellationToken);

        var gridBuilder = KeyboardBuilder
            .Create()
            .Grid();

        if (exists)
        {
            gridBuilder
                .AddButton(Emoji.Pin, "Мой профиль", MeCommandHandler.Metadata.Command)
                .AddButton(Emoji.Door, "Покинуть", LeaveCommandHandler.Metadata.Command)
                .End()
                .AddButton(Emoji.Settings, "Управление подписками", SubscriptionsCommandHandler.Metadata.Command)
                .AddButton(Emoji.Question, "Помощь", HelpCommandHandler.Metadata.Command);
        }
        else
        {
            gridBuilder
                .AddButton(Emoji.Target, "Присоединиться", JoinCommandHandler.Metadata.Command)
                .End()
                .AddButton(Emoji.Question, "Помощь", HelpCommandHandler.Metadata.Command);
        }

        return gridBuilder.Build();
    }

    /// <summary>
    /// Создает клавиатуру подтверждения рассылки
    /// </summary>
    /// <param name="pendingId">ID ожидающей рассылки</param>
    /// <returns>Клавиатура подтверждения рассылки</returns>
    public InlineKeyboardMarkup CreateForBroadcastConfirmation(string pendingId)
    {
        return KeyboardBuilder
            .Create()
            .Grid()
            .AddButton(Emoji.Success, "Подтвердить рассылку", BroadcastConfirmationHandler.ConfirmPrefix + pendingId)
            .AddButton(Emoji.Error, "Отменить", BroadcastConfirmationHandler.CancelPrefix + pendingId)
            .Build();
    }

    public InlineKeyboardMarkup CreateForLastMessages()
    {
        return KeyboardBuilder
            .Create()
            .Grid()
            .AddButton(Emoji.History, "Последние 5", LastMessagesCallbackHandler.ShowPrefix + "5")
            .AddButton(Emoji.History, "Последние 10", LastMessagesCallbackHandler.ShowPrefix + "10")
            .End()
            .AddButton(Emoji.History, "Последние 20", LastMessagesCallbackHandler.ShowPrefix + "20")
            .AddButton(Emoji.Refresh, "Обновить", LastMessagesCallbackHandler.RefreshAction)
            .Build();
    }
}
