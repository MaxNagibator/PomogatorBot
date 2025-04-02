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
            return new("Пожалуйста, укажите сообщение для рассылки.", new());
        }

        var broadcastMessage = message.Text?[length..]?.Trim();

        if (string.IsNullOrEmpty(broadcastMessage))
        {
            return new("Пожалуйста, укажите сообщение для рассылки.", new());
        }

        var response = await userService.NotifyAsync(broadcastMessage, Subscribes.None, cancellationToken);

        return new($"Рассылка завершена. "
                   + $"Успешно отправлено {response.SuccessfulSends} пользователям. "
                   + $"Ошибки при отправке {response.FailedSends} пользователям. "
                   + $"Всего пользователей: {response.TotalUsers}", new());
    }

    private bool IsAdminMessage(Message message)
    {
        return message.From != null
               && string.IsNullOrEmpty(message.From.Username) == false
               && message.From.Username.Equals(_adminUsername, StringComparison.OrdinalIgnoreCase);
    }
}
