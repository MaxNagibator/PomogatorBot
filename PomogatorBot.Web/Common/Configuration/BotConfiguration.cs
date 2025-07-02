namespace PomogatorBot.Web.Common.Configuration;

/// <summary>
/// Конфигурация Telegram бота.
/// </summary>
public class BotConfiguration
{
    /// <summary>
    /// Токен Telegram бота, полученный от @BotFather.
    /// </summary>
    /// <remarks>
    /// Обязательный параметр для работы бота.
    /// Должен быть установлен в конфигурации приложения.
    /// </remarks>
    public string Token { get; set; } = string.Empty;
}
