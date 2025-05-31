using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class LeaveCommandHandler(UserService userService) : IBotCommandHandler, ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("leave", "Покинуть систему");

    public string Command => Metadata.Command;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var userId = message.From?.Id;

        if (userId == null)
        {
            return new("Ошибка идентификации пользователя");
        }

        var user = await userService.GetAsync(userId.Value, cancellationToken);

        if (user == null)
        {
            return new("Вы не зарегистрированы");
        }

        await userService.DeleteAsync(userId.Value, cancellationToken);
        return new($"До свидания, {user.FirstName}! Мы будем скучать 😢");
    }
}
