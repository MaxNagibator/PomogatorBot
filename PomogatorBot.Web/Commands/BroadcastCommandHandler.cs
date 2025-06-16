using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PomogatorBot.Web.Commands;

public class BroadcastCommandHandler(IConfiguration configuration, UserService userService, KeyboardFactory keyboardFactory, BroadcastPendingService broadcastPendingService, MessagePreviewService messagePreviewService) : BotAdminCommandHandler(configuration), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("b", "Возвестить пастве", true);

    public override string Command => Metadata.Command;

    public override async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (IsAdminMessage(message) == false)
        {
            return new("Вы не администратор.", new());
        }

        var length = Metadata.Command.Length + 1;

        if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length <= length)
        {
            return new(GetHelpMessage(), new());
        }

        var parts = message.Text.Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            return new(GetHelpMessage(), new());
        }

        var args = parts[1];
        var broadcastMessage = parts[2];

        Subscribes subscribes;

        try
        {
            subscribes = ParseSubscriptions(args);
        }
        catch (Exception exception)
        {
            return new(exception.Message);
        }

        var entitiesOffset = message.Text.IndexOf(']', StringComparison.Ordinal) + 2;

        var entities = message.Entities?
            .SkipWhile(x => x.Type == MessageEntityType.BotCommand)
            .Select(x => new MessageEntity
            {
                Length = x.Length,
                Offset = x.Offset - entitiesOffset,
                Type = x.Type,
                Url = x.Url,
                User = x.User,
                Language = x.Language,
                CustomEmojiId = x.CustomEmojiId,
            })
            .ToArray();

        // TODO: Обобщить с BroadcastConfirmationHandler

        var userCount = await userService.GetUserCountBySubscriptionAsync(subscribes, cancellationToken);
        var pendingId = broadcastPendingService.StorePendingBroadcast(broadcastMessage, subscribes, entities, message.From!.Id);
        var subscriptionInfo = GetSubscriptionDisplayInfo(subscribes);
        var preview = messagePreviewService.CreatePreview(broadcastMessage, entities);

        var previewHeader = $"""
                             📢 Подтверждение рассылки:

                             🎯 Подписки: {subscriptionInfo}
                             👥 Получателей: {userCount}

                             📋 Предварительный просмотр (как увидят пользователи):
                             ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                             """;

        var previewFooter = """
                            ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

                            ⚠️ Подтвердите отправку рассылки всем указанным пользователям.
                            """;

        var confirmationMessage = previewHeader + preview.PreviewText + previewFooter;
        var adjustedEntities = AdjustEntitiesForConfirmationMessage(preview.PreviewEntities, previewHeader.Length);
        var keyboard = keyboardFactory.CreateForBroadcastConfirmation(pendingId);

        return new(confirmationMessage, keyboard, adjustedEntities);
    }

    private static string GetHelpMessage()
    {
        var subscribes = SubscriptionExtensions.GetSubscriptionMetadata()
            .Values
            .Where(x => x.Subscription != Subscribes.All)
            .Select(x => $"▫️ {x.Subscription}");

        var message = $"""
                       📢 Справка по команде рассылки:

                       Использование:
                       /b [подписки_через_запятую] сообщение

                       Доступные подписки:
                       {string.Join(Environment.NewLine, subscribes)}

                       Доступные теги:
                       <first_name> - имя пользователя
                       <username> - ник пользователя
                       <alias> - псевдоним пользователя (если нет, то имя)

                       ❗При {Subscribes.None} отправится всем пользователям
                       ❗При отсутствии подписок отправится всем пользователям
                       """;

        return message;
    }

    private static Subscribes ParseSubscriptions(string args)
    {
        if (args.StartsWith('[') == false || args.EndsWith(']') == false)
        {
            throw new("not found [ or ]");
        }

        var subscriptionParam = args.Trim('[', ']');

        var result = Subscribes.None;

        if (string.IsNullOrWhiteSpace(subscriptionParam))
        {
            return result;
        }

        var parts = subscriptionParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (Enum.TryParse<Subscribes>(part, true, out var subscription))
            {
                result |= subscription;
            }
            else
            {
                throw new(part + " not parsed");
            }
        }

        return result;
    }

    private static string GetSubscriptionDisplayInfo(Subscribes subscribes)
    {
        if (subscribes == Subscribes.None)
        {
            return "Всем пользователям";
        }

        var metadata = SubscriptionExtensions.GetSubscriptionMetadata();

        var activeSubscriptions = metadata.Values
            .Where(x => x.Subscription != Subscribes.None
                        && x.Subscription != Subscribes.All
                        && subscribes.HasFlag(x.Subscription))
            .Select(x => $"{x.Icon} {x.DisplayName}")
            .ToList();

        return activeSubscriptions.Count > 0
            ? string.Join(", ", activeSubscriptions)
            : "Всем пользователям";
    }

    private static MessageEntity[]? AdjustEntitiesForConfirmationMessage(MessageEntity[]? entities, int offset)
    {
        if (entities == null || entities.Length == 0)
        {
            return null;
        }

        return entities
            .Select(x => new MessageEntity
            {
                Type = x.Type,
                Offset = x.Offset + offset,
                Length = x.Length,
                Url = x.Url,
                User = x.User,
                Language = x.Language,
                CustomEmojiId = x.CustomEmojiId,
            })
            .ToArray();
    }
}
