using Microsoft.Extensions.Options;
using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Configuration;
using PomogatorBot.Web.Constants;
using PomogatorBot.Web.Features.Keyboard;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;
using System.Text;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class LastMessagesCommandHandler(
    IOptions<AdminConfiguration> adminOptions,
    BroadcastHistoryService broadcastHistoryService,
    KeyboardFactory keyboardFactory)
    : AdminRequiredCommandHandler(adminOptions), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("lastmessages", "Просмотр последних отправленных сообщений", true);

    public override string Command => Metadata.Command;

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

        var responseText = FormatBroadcastsResponse(lastBroadcasts, statistics, count);
        var keyboard = keyboardFactory.CreateForLastMessages();

        return new(responseText, keyboard);
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

    // TODO: Вынести а общий класс
    public static string FormatBroadcastsResponse(List<BroadcastHistory> broadcasts, BroadcastStatistics statistics, int requestedCount)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{Emoji.History} Последние {Math.Min(requestedCount, broadcasts.Count)} рассылок:");
        sb.AppendLine();

        sb.AppendLine($"{Emoji.Chart} Общая статистика:");
        sb.AppendLine($"▫️ Всего рассылок: {statistics.Total}");
        sb.AppendLine($"▫️ Завершено: {statistics.Completed} {Emoji.Success}");
        sb.AppendLine($"▫️ В процессе: {statistics.InProgress} ⏳");
        sb.AppendLine($"▫️ Неуспешно: {statistics.Failed} {Emoji.Error}");
        sb.AppendLine($"▫️ Всего сообщений: {statistics.TotalMessagesSent}");
        sb.AppendLine();

        for (var i = 0; i < broadcasts.Count; i++)
        {
            var broadcast = broadcasts[i];

            var statusIcon = broadcast.Status switch
            {
                BroadcastStatus.Completed => Emoji.Success,
                BroadcastStatus.InProgress => "⏳",
                BroadcastStatus.Failed => Emoji.Error,
                _ => "❓",
            };

            var messagePreview = TruncateMessage(broadcast.MessageText, 100);

            var duration = broadcast.CompletedAt.HasValue
                ? $" ({(broadcast.CompletedAt.Value - broadcast.StartedAt).TotalSeconds:F1}с)"
                : " (в процессе)";

            sb.AppendLine($"{i + 1}. {statusIcon} {broadcast.StartedAt:dd.MM.yyyy HH:mm}{duration}");
            sb.AppendLine($"   👥 Получателей: {broadcast.TotalRecipients}");
            sb.AppendLine($"   ✅ Успешно: {broadcast.SuccessfulDeliveries}");
            sb.AppendLine($"   ❌ Неуспешно: {broadcast.FailedDeliveries}");
            sb.AppendLine($"   💬 {messagePreview}");

            if (!string.IsNullOrEmpty(broadcast.ErrorSummary))
            {
                sb.AppendLine($"   🚨 Ошибки: {broadcast.ErrorSummary}");
            }

            if (i < broadcasts.Count - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static string TruncateMessage(string message, int maxLength)
    {
        if (string.IsNullOrEmpty(message) || message.Length <= maxLength)
        {
            return message;
        }

        return message[..(maxLength - 3)] + "...";
    }
}
