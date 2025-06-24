namespace PomogatorBot.Web.Configuration;

/// <summary>
/// Конфигурация администратора бота.
/// </summary>
public class AdminConfiguration
{
    /// <summary>
    /// Имя пользователя администратора в Telegram (без символа @).
    /// </summary>
    /// <remarks>
    /// Используется для проверки прав доступа к административным командам.
    /// </remarks>
    public string Username { get; set; } = string.Empty;
}
