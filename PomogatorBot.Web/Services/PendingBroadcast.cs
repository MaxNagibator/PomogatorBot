namespace PomogatorBot.Web.Services;

public class PendingBroadcast
{
    public required string Id { get; init; }
    public required string Message { get; init; }
    public required Subscribes Subscribes { get; init; }
    public MessageEntity[]? Entities { get; init; }
    public required long AdminUserId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
