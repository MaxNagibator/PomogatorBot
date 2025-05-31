using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PomogatorBot.Web.Commands;

public class BroadcastCommandHandler(IConfiguration configuration, UserService userService) : BotAdminCommandHandler(configuration), ICommandMetadata
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

        var response = await userService.NotifyAsync(broadcastMessage, subscribes, entities, cancellationToken);

        return new($"""
                    Рассылка завершена. 
                    Успешно - {response.SuccessfulSends}
                    С ошибкой - {response.FailedSends}
                    Всего - {response.TotalUsers}
                    """, new());
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
}
