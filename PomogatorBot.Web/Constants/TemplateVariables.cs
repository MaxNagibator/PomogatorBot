namespace PomogatorBot.Web.Constants;

/// <summary>
/// Централизованное управление шаблонами переменных для сообщений.
/// </summary>
public static class TemplateVariables
{
    /// <summary>
    /// Переменные пользователя для подстановки в сообщения
    /// </summary>
    public static class User
    {
        /// <summary>
        /// Имя пользователя (FirstName)
        /// </summary>
        public const string FirstName = "<first_name>";

        /// <summary>
        /// Имя пользователя в Telegram (@username)
        /// </summary>
        public const string Username = "<username>";

        /// <summary>
        /// Псевдоним пользователя (если установлен, иначе FirstName)
        /// </summary>
        public const string Alias = "<alias>";
    }

    /// <summary>
    /// Переменные для предварительного просмотра сообщений
    /// </summary>
    public static class Preview
    {
        /// <summary>
        /// Пример имени для предварительного просмотра
        /// </summary>
        public const string FirstName = "Иван";

        /// <summary>
        /// Пример username для предварительного просмотра
        /// </summary>
        public const string Username = "@admin";

        /// <summary>
        /// Пример псевдонима для предварительного просмотра
        /// </summary>
        public const string Alias = "Админ";
    }
}
