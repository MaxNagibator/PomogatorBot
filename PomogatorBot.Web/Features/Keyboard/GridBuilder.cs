using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Features.Keyboard;

/// <summary>
/// Строитель для создания клавиатур в режиме сетки с поддержкой fluent API
/// </summary>
public class GridBuilder
{
    private readonly KeyboardBuilder _keyboardBuilder;
    private readonly KeyboardBuilderOptions _options;
    private readonly List<InlineKeyboardButton> _currentRow = [];

    /// <summary>
    /// Инициализирует новый экземпляр GridBuilder
    /// </summary>
    /// <param name="keyboardBuilder">Родительский строитель клавиатуры</param>
    /// <param name="options">Параметры конфигурации</param>
    internal GridBuilder(KeyboardBuilder keyboardBuilder, KeyboardBuilderOptions options)
    {
        _keyboardBuilder = keyboardBuilder ?? throw new ArgumentNullException(nameof(keyboardBuilder));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Добавляет callback-кнопку в текущую строку сетки
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddButton(string text, string callbackData)
    {
        _keyboardBuilder.ValidateButtonText(text);
        _keyboardBuilder.ValidateCallbackData(callbackData);
        ValidateRowCapacity();

        _currentRow.Add(InlineKeyboardButton.WithCallbackData(text, callbackData));
        return this;
    }

    /// <summary>
    /// Добавляет callback-кнопку с эмодзи в текущую строку сетки
    /// </summary>
    /// <param name="emoji">Эмодзи для кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    /// <remarks>
    /// Эмодзи и текст объединяются через пробел. Если эмодзи пустое или null, используется только текст.<br />
    /// Пример: AddButton("🎯", "Цель", "target") создаст кнопку с текстом "🎯 Цель".<br />
    /// Метод следует тем же правилам валидации, что и стандартный AddButton в режиме сетки.
    /// </remarks>
    public GridBuilder AddButton(string emoji, string text, string callbackData)
    {
        var buttonText = string.IsNullOrEmpty(emoji) ? text : $"{emoji} {text}";
        return AddButton(buttonText, callbackData);
    }

    /// <summary>
    /// Добавляет URL-кнопку в текущую строку сетки
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="url">URL для открытия</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddUrlButton(string text, string url)
    {
        _keyboardBuilder.ValidateButtonText(text);
        _keyboardBuilder.ValidateUrl(url);
        ValidateRowCapacity();

        _currentRow.Add(InlineKeyboardButton.WithUrl(text, url));
        return this;
    }

    /// <summary>
    /// Добавляет кнопку Web App в текущую строку сетки
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="webAppUrl">URL Web App</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddWebAppButton(string text, string webAppUrl)
    {
        _keyboardBuilder.ValidateButtonText(text);
        _keyboardBuilder.ValidateUrl(webAppUrl);
        ValidateRowCapacity();

        _currentRow.Add(InlineKeyboardButton.WithWebApp(text, new(webAppUrl)));
        return this;
    }

    /// <summary>
    /// Добавляет кнопку переключения inline-запроса в текущую строку сетки
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="query">Inline-запрос</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddSwitchInlineButton(string text, string query)
    {
        _keyboardBuilder.ValidateButtonText(text);
        ValidateRowCapacity();

        _currentRow.Add(InlineKeyboardButton.WithSwitchInlineQuery(text, query));
        return this;
    }

    /// <summary>
    /// Добавляет кнопку переключения inline-запроса в текущем чате в текущую строку сетки
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="query">Inline-запрос</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddSwitchInlineCurrentChatButton(string text, string query)
    {
        _keyboardBuilder.ValidateButtonText(text);
        ValidateRowCapacity();

        _currentRow.Add(InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text, query));
        return this;
    }

    /// <summary>
    /// Добавляет кнопку с иконкой в текущую строку сетки
    /// </summary>
    /// <param name="icon">Иконка кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddButtonWithIcon(string icon, string text, string callbackData)
    {
        var displayText = $"{icon} {text}";
        return AddButton(displayText, callbackData);
    }

    /// <summary>
    /// Добавляет кнопку в текущую строку сетки, только если условие истинно
    /// </summary>
    /// <param name="condition">Условие для проверки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddButtonIf(bool condition, string text, string callbackData)
    {
        if (condition)
        {
            AddButton(text, callbackData);
        }

        return this;
    }

    /// <summary>
    /// Добавляет кнопку в текущую строку сетки, только если условие ложно
    /// </summary>
    /// <param name="condition">Условие для проверки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder AddButtonUnless(bool condition, string text, string callbackData)
    {
        return AddButtonIf(!condition, text, callbackData);
    }

    /// <summary>
    /// Завершает текущую строку и переходит к следующей строке в сетке
    /// </summary>
    /// <returns>Этот экземпляр GridBuilder для цепочки методов</returns>
    public GridBuilder End()
    {
        if (_currentRow.Count > 0)
        {
            _keyboardBuilder.AddButtonRowInternal([.. _currentRow]);
            _currentRow.Clear();
        }

        return this;
    }

    /// <summary>
    /// Завершает создание сетки и возвращает готовую клавиатуру
    /// </summary>
    /// <returns>Экземпляр InlineKeyboardMarkup</returns>
    public InlineKeyboardMarkup Build()
    {
        if (_currentRow.Count > 0)
        {
            End();
        }

        return _keyboardBuilder.Build();
    }

    private void ValidateRowCapacity()
    {
        if (_currentRow.Count >= _options.MaxButtonsPerRow)
        {
            _keyboardBuilder.HandleValidationFailure($"Строка не может содержать более {_options.MaxButtonsPerRow} кнопок");
        }
    }
}
