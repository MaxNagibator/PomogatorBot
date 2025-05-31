using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class DefaultCommandHandler(UserService userService) : IBotCommandHandler
{
    public string Command => string.Empty;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var userId = message.From?.Id;

        if (userId == null)
        {
            return new("Ошибка идентификации пользователя");
        }

        var exists = await userService.ExistsAsync(userId.Value, cancellationToken);

        return exists
            ? new($"Не понимаю команду. Используйте /{HelpCommandHandler.Metadata.Command} для списка команд")
            : new BotResponse($"Для начала работы выполните /{JoinCommandHandler.Metadata.Command}");
    }
}
