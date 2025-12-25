using PomogatorBot.Web.Common.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Services;

public sealed class BroadcastPreviewService(
    UserService userService,
    MessagePreviewService messagePreviewService)
{
    public async Task<PreviewData> BuildPreviewAsync(
        string message,
        Subscribes subscribes,
        MessageEntity[]? entities,
        Func<InlineKeyboardMarkup> keyboardFactory,
        long? adminId = null,
        CancellationToken cancellationToken = default)
    {
        var userCount = await userService.GetCountBySubscriptionAsync(subscribes, adminId, cancellationToken);
        var subscriptionInfo = GetSubscriptionDisplayInfo(subscribes);
        var preview = messagePreviewService.CreatePreview(message, entities);

        var previewHeader = $"""
                             {Emoji.Megaphone} Подтверждение рассылки:

                             {Emoji.Target} Подписки: {subscriptionInfo}
                             {Emoji.Users} Получателей: {userCount}

                             {Emoji.List} Предварительный просмотр (как увидят пользователи):
                             ━━━━━
                             """;

        var previewFooter = $"""
                             ━━━━━
                             {Emoji.Warning} Подтвердите отправку рассылки всем указанным пользователям.
                             """;

        var confirmationMessage = string.Join(Environment.NewLine, previewHeader, preview.PreviewText, previewFooter);
        var adjustedEntities = MessageEntityHelper.OffsetEntities(preview.PreviewEntities, previewHeader.Length + Environment.NewLine.Length);

        return new(confirmationMessage, adjustedEntities, keyboardFactory(), null);
    }

    public string GetSubscriptionDisplayInfo(Subscribes subscribes)
    {
        if (subscribes == Subscribes.None)
        {
            return $"{Emoji.Users} Всем пользователям";
        }

        var metadata = SubscriptionExtensions.SubscriptionMetadata;

        var activeSubscriptions = metadata.Values
            .Where(x => x.Subscription != Subscribes.None
                        && x.Subscription != Subscribes.All
                        && subscribes.HasFlag(x.Subscription))
            .Select(x => $"{x.Icon} {x.DisplayName}")
            .ToList();

        return activeSubscriptions.Count > 0
            ? string.Join(", ", activeSubscriptions)
            : $"{Emoji.Users} Всем пользователям";
    }

    public sealed record PreviewData(
        string Message,
        MessageEntity[]? Entities,
        InlineKeyboardMarkup Keyboard,
        Action<long, int>? OnMessageSent);
}
