using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class BroadcastCommandHandler(IConfiguration configuration, IUserService userService) : IBotCommandHandler, ICommandMetadata
{
    private readonly string _adminUsername = configuration["Admin:Username"]
                                             ?? throw new InvalidOperationException("Имя пользователя администратора не настроено.");

    public static CommandMetadata Metadata { get; } = new("b", "Возвестить пастве");

    public string Command => Metadata.Command;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (IsAdminMessage(message) == false)
        {
            return new("Вы не администратор.", new());
        }

        var length = Metadata.Command.Length + 1;

        if (message.Text?.Length <= length)
        {
            return new(GetHelpMessage(), new());
        }

        var messageText = message.Text?[length..]?.Trim();

        if (string.IsNullOrEmpty(messageText))
        {
            return new(GetHelpMessage(), new());
        }

        var subscribes = Subscribes.None;
        var broadcastMessage = messageText;
        var closingBracketIndex = messageText.IndexOf(']');

        if (messageText.StartsWith('[') && closingBracketIndex != -1)
        {
            var subscriptionParam = messageText.Substring(1, closingBracketIndex - 1).Trim();
            broadcastMessage = messageText[(closingBracketIndex + 1)..].Trim();

            if (string.IsNullOrEmpty(broadcastMessage))
            {
                return new("Пожалуйста, укажите сообщение для рассылки после параметров подписок.", new());
            }

            subscribes = ParseSubscriptions(subscriptionParam);
        }

        var response = await userService.NotifyAsync(broadcastMessage, subscribes, cancellationToken);

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

                       ❗При {Subscribes.None} отправится всем пользователям
                       ❗При отсутствии подписок отправится всем пользователям
                       """;

        return message;
    }

    private static Subscribes ParseSubscriptions(string subscriptionParam)
    {
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
        }

        return result;
    }

    private bool IsAdminMessage(Message message)
    {
        return message.From != null
               && string.IsNullOrEmpty(message.From.Username) == false
               && message.From.Username.Equals(_adminUsername, StringComparison.OrdinalIgnoreCase);
    }
}
