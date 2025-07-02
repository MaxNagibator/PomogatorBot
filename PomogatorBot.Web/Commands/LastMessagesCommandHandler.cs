using Microsoft.Extensions.Options;
using PomogatorBot.Web.Common.Configuration;
using PomogatorBot.Web.Common.Constants;
using PomogatorBot.Web.Common.Keyboard;
using PomogatorBot.Web.Infrastructure.Entities;
using System.Text;

namespace PomogatorBot.Web.Commands;

public class LastMessagesCommandHandler(
    IOptions<AdminConfiguration> adminOptions,
    BroadcastHistoryService broadcastHistoryService,
    KeyboardFactory keyboardFactory)
    : AdminRequiredCommandHandler(adminOptions), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("lastmessages", "Просмотр последних отправленных сообщений", true);

    public override string Command => Metadata.Command;

    public static (string responseText, MessageEntity[]? entities) FormatBroadcastsResponseWithEntities(
        List<BroadcastHistory> broadcasts,
        BroadcastStatistics statistics,
        int requestedCount)
    {
        var stringBuilder = new StringBuilder();
        var allEntities = new List<MessageEntity>();

        var headerText = $"{Emoji.History} Последние {Math.Min(requestedCount, broadcasts.Count)} рассылок:\n\n";
        stringBuilder.Append(headerText);

        var statsText = $"""
                         {Emoji.Chart} Общая статистика:
                         {Emoji.Bullet} Всего рассылок: {statistics.Total}
                         {Emoji.Bullet} Завершено: {statistics.Completed} {Emoji.Success}
                         {Emoji.Bullet} В процессе: {statistics.InProgress} {Emoji.Loading}
                         {Emoji.Bullet} Неуспешно: {statistics.Failed} {Emoji.Error}
                         {Emoji.Bullet} Всего сообщений: {statistics.TotalMessagesSent}


                         """;

        stringBuilder.Append(statsText);

        for (var i = 0; i < broadcasts.Count; i++)
        {
            var broadcast = broadcasts[i];

            var statusIcon = broadcast.Status switch
            {
                BroadcastStatus.Completed => Emoji.Success,
                BroadcastStatus.InProgress => Emoji.Loading,
                BroadcastStatus.Failed => Emoji.Error,
                _ => Emoji.Question,
            };

            var duration = broadcast.CompletedAt.HasValue
                ? $" ({(broadcast.CompletedAt.Value - broadcast.StartedAt).Milliseconds} мс)"
                : " (в процессе)";

            var broadcastInfo = $"""
                                 {i + 1}. {statusIcon} {broadcast.StartedAt:dd.MM.yyyy HH:mm}{duration}
                                    {Emoji.Users} Получателей: {broadcast.TotalRecipients}
                                    {Emoji.Success} Успешно: {broadcast.SuccessfulDeliveries}
                                    {Emoji.Error} Неуспешно: {broadcast.FailedDeliveries}
                                    {Emoji.Message}
                                 """;

            stringBuilder.Append(broadcastInfo);

            var messageStartOffset = stringBuilder.Length;
            var messagePreview = TruncateMessage(broadcast.MessageText, 200);
            stringBuilder.Append(messagePreview);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("━━━━━");

            var adaptedEntities = MessageEntityHelper.AdaptEntitiesForTruncatedMessage(broadcast.MessageEntities,
                broadcast.MessageText,
                messagePreview,
                messageStartOffset);

            if (adaptedEntities != null)
            {
                allEntities.AddRange(adaptedEntities);
            }

            if (string.IsNullOrEmpty(broadcast.ErrorSummary) == false)
            {
                stringBuilder.AppendLine($"\n   {Emoji.Alert} Ошибки: {broadcast.ErrorSummary}");
            }

            if (i < broadcasts.Count - 1)
            {
                stringBuilder.AppendLine();
            }
        }

        return (stringBuilder.ToString(), allEntities.Count > 0 ? allEntities.ToArray() : null);
    }

    protected override async Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var count = ParseMessageCount(message.Text);
        var lastBroadcasts = await broadcastHistoryService.GetLastsAsync(count, cancellationToken);
        var statistics = await broadcastHistoryService.GetStatisticsAsync(cancellationToken);

        if (lastBroadcasts.Count == 0)
        {
            var emptyResponse = $"{Emoji.Info} История рассылок пуста.";
            var emptyKeyboard = keyboardFactory.CreateForLastMessages();
            return new(emptyResponse, emptyKeyboard);
        }

        var (responseText, responseEntities) = FormatBroadcastsResponseWithEntities(lastBroadcasts, statistics, count);
        var keyboard = keyboardFactory.CreateForLastMessages();

        return new(responseText, keyboard, responseEntities);
    }

    private static int ParseMessageCount(string? messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return 1;
        }

        var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            return 1;
        }

        if (int.TryParse(parts[1], out var count) && count > 0)
        {
            return Math.Min(count, 50);
        }

        return 1;
    }

    private static string TruncateMessage(string message, int maxLength)
    {
        if (string.IsNullOrEmpty(message) || message.Length <= maxLength)
        {
            return message;
        }

        return $"{message[..(maxLength - 3)]}...";
    }
}
