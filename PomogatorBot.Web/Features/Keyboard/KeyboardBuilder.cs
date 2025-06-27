using PomogatorBot.Web.CallbackQueries;
using PomogatorBot.Web.Constants;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Features.Keyboard;

/// <summary>
/// Fluent-строитель для создания Telegram inline-клавиатур
/// </summary>
public class KeyboardBuilder
{
    private readonly List<InlineKeyboardButton[]> _buttons = [];
    private readonly KeyboardBuilderOptions _options;

    /// <summary>
    /// Инициализирует новый экземпляр KeyboardBuilder с пользовательскими настройками
    /// </summary>
    /// <param name="options">Параметры конфигурации для валидации и ограничений</param>
    private KeyboardBuilder(KeyboardBuilderOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Инициализирует новый экземпляр KeyboardBuilder с настройками по умолчанию
    /// </summary>
    private KeyboardBuilder() : this(new())
    {
    }

    /// <summary>
    /// Получает текущее количество строк в клавиатуре
    /// </summary>
    public int RowCount => _buttons.Count;

    /// <summary>
    /// Получает текущее количество кнопок в клавиатуре
    /// </summary>
    public int ButtonCount => _buttons.Sum(row => row.Length);

    /// <summary>
    /// Создает новый экземпляр KeyboardBuilder
    /// </summary>
    /// <returns>Новый экземпляр KeyboardBuilder</returns>
    public static KeyboardBuilder Create()
    {
        return new();
    }

    /// <summary>
    /// Создает новый экземпляр KeyboardBuilder с пользовательскими настройками
    /// </summary>
    /// <param name="options">Параметры конфигурации</param>
    /// <returns>Новый экземпляр KeyboardBuilder</returns>
    public static KeyboardBuilder Create(KeyboardBuilderOptions options)
    {
        return new(options);
    }

    /// <summary>
    /// Добавляет одну callback-кнопку как новую строку
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddButton(string text, string callbackData)
    {
        ValidateButtonText(text);
        ValidateCallbackData(callbackData);

        _buttons.Add([InlineKeyboardButton.WithCallbackData(text, callbackData)]);
        return this;
    }

    /// <summary>
    /// Добавляет одну callback-кнопку с эмодзи как новую строку
    /// </summary>
    /// <param name="emoji">Эмодзи для кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    /// <remarks>
    /// Эмодзи и текст объединяются через пробел. Если эмодзи пустое или null, используется только текст.<br />
    /// Пример: AddButton("🎯", "Цель", "target") создаст кнопку с текстом "🎯 Цель".<br />
    /// Метод следует тем же правилам валидации, что и стандартный AddButton.
    /// </remarks>
    public KeyboardBuilder AddButton(string emoji, string text, string callbackData)
    {
        var buttonText = string.IsNullOrEmpty(emoji) ? text : $"{emoji} {text}";
        return AddButton(buttonText, callbackData);
    }

    /// <summary>
    /// Добавляет строку callback-кнопок
    /// </summary>
    /// <param name="buttons">Массив кортежей с текстом кнопки и данными callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddButtonRow(params (string text, string callbackData)[] buttons)
    {
        if (buttons.Length > _options.MaxButtonsPerRow)
        {
            HandleValidationFailure($"Строка не может содержать более {_options.MaxButtonsPerRow} кнопок");
        }

        var buttonRow = new List<InlineKeyboardButton>();

        foreach (var (text, callbackData) in buttons)
        {
            ValidateButtonText(text);
            ValidateCallbackData(callbackData);
            buttonRow.Add(InlineKeyboardButton.WithCallbackData(text, callbackData));
        }

        _buttons.Add([.. buttonRow]);
        return this;
    }

    /// <summary>
    /// Добавляет кнопку переключения подписки (обратная совместимость)
    /// </summary>
    /// <param name="meta">Метаданные подписки</param>
    /// <param name="current">Текущее состояние подписки</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddSubscriptionButton(SubscriptionMeta meta, Subscribes current)
    {
        var isActive = current.HasFlag(meta.Subscription);
        var buttonText = $"{meta.Icon} {meta.DisplayName} {(isActive ? Emoji.Success : Emoji.Error)}";
        AddButton(buttonText, ToggleSubscriptionHandler.GetFormatedToggle(meta.Subscription));
        return this;
    }

    /// <summary>
    /// Добавляет callback-кнопку как новую строку
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddCallbackButton(string text, string callbackData)
    {
        return AddButton(text, callbackData);
    }

    /// <summary>
    /// Добавляет URL-кнопку как новую строку
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="url">URL для открытия</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddUrlButton(string text, string url)
    {
        ValidateButtonText(text);
        ValidateUrl(url);

        _buttons.Add([InlineKeyboardButton.WithUrl(text, url)]);
        return this;
    }

    /// <summary>
    /// Добавляет кнопку Web App как новую строку
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="webAppUrl">URL Web App</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddWebAppButton(string text, string webAppUrl)
    {
        ValidateButtonText(text);
        ValidateUrl(webAppUrl);

        _buttons.Add([InlineKeyboardButton.WithWebApp(text, new(webAppUrl))]);
        return this;
    }

    /// <summary>
    /// Добавляет кнопку переключения inline-запроса как новую строку
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="query">Inline-запрос</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddSwitchInlineButton(string text, string query)
    {
        ValidateButtonText(text);

        _buttons.Add([InlineKeyboardButton.WithSwitchInlineQuery(text, query)]);
        return this;
    }

    /// <summary>
    /// Добавляет кнопку переключения inline-запроса в текущем чате как новую строку
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="query">Inline-запрос</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddSwitchInlineCurrentChatButton(string text, string query)
    {
        ValidateButtonText(text);

        _buttons.Add([InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text, query)]);
        return this;
    }

    /// <summary>
    /// Добавляет кнопку с иконкой как новую строку
    /// </summary>
    /// <param name="icon">Иконка кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddButtonWithIcon(string icon, string text, string callbackData)
    {
        var displayText = $"{icon} {text}";
        return AddButton(displayText, callbackData);
    }

    /// <summary>
    /// Добавляет кнопку, только если условие истинно
    /// </summary>
    /// <param name="condition">Условие для проверки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddButtonIf(bool condition, string text, string callbackData)
    {
        if (condition)
        {
            AddButton(text, callbackData);
        }

        return this;
    }

    /// <summary>
    /// Добавляет кнопку, только если условие ложно
    /// </summary>
    /// <param name="condition">Условие для проверки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Данные callback</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddButtonUnless(bool condition, string text, string callbackData)
    {
        return AddButtonIf(!condition, text, callbackData);
    }

    /// <summary>
    /// Добавляет кнопку на основе функции-предиката
    /// </summary>
    /// <param name="predicate">Функция, возвращающая true, если кнопка должна быть добавлена</param>
    /// <param name="buttonFactory">Функция, создающая конфигурацию кнопки</param>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder AddButtonWhen(Func<bool> predicate, Func<(string text, string callbackData)> buttonFactory)
    {
        if (predicate.Invoke() == false)
        {
            return this;
        }

        var (text, callbackData) = buttonFactory.Invoke();
        AddButton(text, callbackData);

        return this;
    }

    /// <summary>
    /// Создает финальную InlineKeyboardMarkup
    /// </summary>
    /// <returns>Экземпляр InlineKeyboardMarkup</returns>
    public InlineKeyboardMarkup Build()
    {
        if (_buttons.Count > _options.MaxRows)
        {
            HandleValidationFailure($"Клавиатура не может содержать более {_options.MaxRows} строк");
        }

        return new(_buttons);
    }

    /// <summary>
    /// Инициирует режим сетки для создания клавиатуры в формате таблицы
    /// </summary>
    /// <returns>Экземпляр GridBuilder для создания сетки кнопок</returns>
    public GridBuilder Grid()
    {
        return new(this, _options);
    }

    /// <summary>
    /// Очищает все кнопки и сбрасывает строитель
    /// </summary>
    /// <returns>Этот экземпляр KeyboardBuilder для цепочки методов</returns>
    public KeyboardBuilder Clear()
    {
        _buttons.Clear();
        return this;
    }

    /// <summary>
    /// Проверяет текст кнопки на соответствие требованиям и ограничениям
    /// </summary>
    /// <param name="text">Текст кнопки для проверки</param>
    /// <remarks>
    /// Проверяет, что текст кнопки не пустой и его длина не превышает максимально допустимую.<br />
    /// Если проверка не проходит, вызывается HandleValidationFailure с соответствующим сообщением.<br />
    /// Проверка может быть отключена через настройки опций (ValidateButtonText = false).
    /// </remarks>
    public void ValidateButtonText(string text)
    {
        if (_options.ValidateButtonText == false)
        {
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            HandleValidationFailure("Текст кнопки не может быть null или пустым");
        }

        if (text.Length > _options.MaxButtonTextLength)
        {
            HandleValidationFailure($"Текст кнопки не может превышать {_options.MaxButtonTextLength} символов");
        }
    }

    /// <summary>
    /// Проверяет данные callback на соответствие требованиям и ограничениям
    /// </summary>
    /// <param name="callbackData">Данные callback для проверки</param>
    /// <remarks>
    /// Проверяет, что данные callback не пустые и их размер в байтах не превышает максимально допустимый.<br />
    /// Если проверка не проходит, вызывается HandleValidationFailure с соответствующим сообщением.<br />
    /// Проверка может быть отключена через настройки опций (ValidateCallbackData = false).
    /// </remarks>
    public void ValidateCallbackData(string callbackData)
    {
        if (_options.ValidateCallbackData == false)
        {
            return;
        }

        if (string.IsNullOrEmpty(callbackData))
        {
            HandleValidationFailure("Данные callback не могут быть null или пустыми");
        }

        if (Encoding.UTF8.GetByteCount(callbackData) > _options.MaxCallbackDataLength)
        {
            HandleValidationFailure($"Данные callback не могут превышать {_options.MaxCallbackDataLength} байт");
        }
    }

    /// <summary>
    /// Проверяет URL на соответствие требованиям и формату
    /// </summary>
    /// <param name="url">URL для проверки</param>
    /// <remarks>
    /// Проверяет, что URL не пустой и соответствует формату абсолютного URI.<br />
    /// Если проверка не проходит, вызывается HandleValidationFailure с соответствующим сообщением.<br />
    /// Этот метод всегда выполняет проверку, независимо от настроек опций.
    /// </remarks>
    public void ValidateUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            HandleValidationFailure("URL не может быть null или пустым");
        }

        if (Uri.IsWellFormedUriString(url, UriKind.Absolute) == false)
        {
            HandleValidationFailure("Неверный формат URL");
        }
    }

    /// <summary>
    /// Обрабатывает ошибки валидации в соответствии с настройками
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <remarks>
    /// Если в настройках опций включен режим выброса исключений (ThrowOnValidationFailure = true),<br />
    /// метод выбрасывает ArgumentException с указанным сообщением.<br />
    /// В противном случае ошибка игнорируется, что может привести к некорректному поведению клавиатуры.<br />
    /// Рекомендуется включать режим выброса исключений во время разработки для выявления проблем.
    /// </remarks>
    public void HandleValidationFailure(string message)
    {
        if (_options.ThrowOnValidationFailure)
        {
            throw new ArgumentException(message);
        }
    }

    /// <summary>
    /// Внутренний метод для добавления готовой строки кнопок
    /// </summary>
    /// <param name="buttonRow">Массив кнопок для добавления</param>
    internal void AddButtonRowInternal(InlineKeyboardButton[] buttonRow)
    {
        _buttons.Add(buttonRow);
    }
}
