namespace PomogatorBot.Web.Infrastructure.Entities;

/// <summary>
/// Статус рассылки
/// </summary>
public enum BroadcastStatus
{
    /// <summary>
    /// Рассылка в процессе выполнения
    /// </summary>
    InProgress = 0,

    /// <summary>
    /// Рассылка успешно завершена
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Рассылка прервана из-за критической ошибки
    /// </summary>
    Failed = 2,
}
