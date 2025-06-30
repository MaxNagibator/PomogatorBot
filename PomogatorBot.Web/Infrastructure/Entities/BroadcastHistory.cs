using System.ComponentModel.DataAnnotations;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Infrastructure.Entities;

/// <summary>
/// Сущность для хранения истории рассылок
/// </summary>
public class BroadcastHistory
{
    /// <summary>
    /// Уникальный идентификатор рассылки
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Текст рассылки
    /// </summary>
    [Required]
    [MaxLength(4096)]
    public string MessageText { get; set; } = null!;

    /// <summary>
    /// Дата и время начала рассылки
    /// </summary>
    [Required]
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Дата и время завершения рассылки
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// ID администратора, запустившего рассылку
    /// </summary>
    public long? AdminUserId { get; set; }

    /// <summary>
    /// Общее количество получателей
    /// </summary>
    [Required]
    public int TotalRecipients { get; set; }

    /// <summary>
    /// Количество успешно доставленных сообщений
    /// </summary>
    [Required]
    public int SuccessfulDeliveries { get; set; }

    /// <summary>
    /// Количество неуспешных доставок
    /// </summary>
    [Required]
    public int FailedDeliveries { get; set; }

    /// <summary>
    /// Статус рассылки
    /// </summary>
    [Required]
    public BroadcastStatus Status { get; set; }

    /// <summary>
    /// Краткое описание ошибок (если были)
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorSummary { get; set; }

    /// <summary>
    /// Сущности форматирования сообщения (жирный, курсив, ссылки и т.д.)
    /// </summary>
    public MessageEntity[]? MessageEntities { get; set; }
}
