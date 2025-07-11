using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

/// <summary>
/// Информация об уведомлениях для рассылки, ожидающей подтверждения
/// </summary>
public class BroadcastNotificationInfo
{
    /// <summary>
    /// ID рассылки
    /// </summary>
    public required string BroadcastId { get; init; }

    /// <summary>
    /// ID чата для отправки уведомлений
    /// </summary>
    public required long ChatId { get; init; }

    /// <summary>
    /// ID текущего сообщения с подтверждением (для редактирования при обновлении)
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// Исходный текст сообщения пользователя с подтверждением рассылки
    /// </summary>
    public required string OriginalConfirmationMessage { get; init; }

    /// <summary>
    /// Исходные сущности форматирования сообщения пользователя с подтверждением
    /// </summary>
    public MessageEntity[]? OriginalConfirmationEntities { get; init; }

    /// <summary>
    /// Исходная клавиатура сообщения с подтверждением рассылки
    /// </summary>
    public InlineKeyboardMarkup? OriginalConfirmationKeyboard { get; init; }

    /// <summary>
    /// Время создания рассылки
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Время истечения рассылки
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Время следующего напоминания
    /// </summary>
    public DateTime NextReminderAt { get; set; }

    /// <summary>
    /// Текущий интервал между напоминаниями
    /// </summary>
    public TimeSpan ReminderInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Было ли отправлено предупреждение до удаления
    /// </summary>
    public bool PreWarningNotificationSent { get; set; }

    /// <summary>
    /// Была ли рассылка подтверждена (для отмены уведомлений)
    /// </summary>
    public bool IsConfirmed { get; set; }
}
