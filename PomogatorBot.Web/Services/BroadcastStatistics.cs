namespace PomogatorBot.Web.Services;

/// <summary>
/// Статистика рассылок
/// </summary>
public class BroadcastStatistics
{
    /// <summary>
    /// Общее количество рассылок
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Количество завершенных рассылок
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// Количество рассылок в процессе
    /// </summary>
    public int InProgress { get; set; }

    /// <summary>
    /// Количество неуспешных рассылок
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Общее количество отправленных сообщений
    /// </summary>
    public int TotalMessagesSent { get; set; }

    /// <summary>
    /// Общее количество успешно доставленных сообщений
    /// </summary>
    public int TotalSuccessfulDeliveries { get; set; }

    /// <summary>
    /// Общее количество неуспешных доставок
    /// </summary>
    public int TotalFailedDeliveries { get; set; }
}
