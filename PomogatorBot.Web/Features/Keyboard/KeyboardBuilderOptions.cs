namespace PomogatorBot.Web.Features.Keyboard;

/// <summary>
/// Параметры конфигурации для валидации и ограничений KeyboardBuilder
/// </summary>
public class KeyboardBuilderOptions
{
    /// <summary>
    /// Максимальное количество кнопок в строке
    /// </summary>
    /// <remarks>
    /// Лимит Telegram: 8
    /// </remarks>
    public int MaxButtonsPerRow { get; set; } = 8;

    /// <summary>
    /// Максимальное количество строк в клавиатуре
    /// </summary>
    /// <remarks>
    /// Лимит Telegram: 100
    /// </remarks>
    public int MaxRows { get; set; } = 100;

    /// <summary>
    /// Максимальная длина текста кнопки
    /// </summary>
    /// <remarks>
    /// Лимит Telegram: 64 символа
    /// </remarks>
    public int MaxButtonTextLength { get; set; } = 64;

    /// <summary>
    /// Максимальная длина данных callback
    /// </summary>
    /// <remarks>
    /// Лимит Telegram: 64 байта
    /// </remarks>
    public int MaxCallbackDataLength { get; set; } = 64;

    /// <summary>
    /// Валидировать ли длину данных callback
    /// </summary>
    public bool ValidateCallbackData { get; set; } = true;

    /// <summary>
    /// Валидировать ли длину текста кнопки
    /// </summary>
    public bool ValidateButtonText { get; set; } = true;

    /// <summary>
    /// Выбрасывать ли исключения при ошибках валидации или молча игнорировать
    /// </summary>
    public bool ThrowOnValidationFailure { get; set; } = true;
}
