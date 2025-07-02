namespace PomogatorBot.Web.Services;

public class MessagePreviewResult
{
    public required string PreviewText { get; init; }
    public MessageEntity[]? PreviewEntities { get; init; }
    public required string OriginalMessage { get; init; }
    public MessageEntity[]? OriginalEntities { get; init; }
}
