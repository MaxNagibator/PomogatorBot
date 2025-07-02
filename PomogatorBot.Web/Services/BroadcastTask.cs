using Telegram.Bot.Types;

namespace PomogatorBot.Web.Services;

/// <summary>
/// Задача рассылки для обработки в фоновом сервисе
/// </summary>
public class BroadcastTask
{
    /// <summary>
    /// ID рассылки
    /// </summary>
    public required string BroadcastId { get; init; }

    /// <summary>
    /// Текст сообщения для рассылки
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Подписки для фильтрации получателей
    /// </summary>
    public required Subscribes Subscribes { get; init; }

    /// <summary>
    /// Сущности форматирования сообщения
    /// </summary>
    public MessageEntity[]? Entities { get; init; }

    /// <summary>
    /// ID администратора (исключается из рассылки)
    /// </summary>
    public required long AdminUserId { get; init; }

    /// <summary>
    /// ID чата для обновления прогресса
    /// </summary>
    public required long ChatId { get; init; }

    /// <summary>
    /// ID сообщения для обновления прогресса
    /// </summary>
    public required int MessageId { get; init; }

    /// <summary>
    /// Время создания задачи
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
