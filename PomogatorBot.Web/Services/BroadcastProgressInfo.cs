namespace PomogatorBot.Web.Services;

public class BroadcastProgressInfo
{
    public required string BroadcastId { get; init; }
    public required long ChatId { get; init; }
    public required int MessageId { get; init; }
    public required int TotalUsers { get; init; }
    public required DateTime StartedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
