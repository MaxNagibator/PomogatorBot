using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class MeCommandHandler(UserService userService) : IBotCommandHandler, ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("me", "Показать информацию о себе");

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
            return new($"Сначала выполните /{JoinCommandHandler.Metadata.Command}");
        }

        return new($"""
                    📋 Ваш профиль:
                    ID: {user.UserId}
                    Username: @{user.Username}
                    Имя: {user.FirstName}
                    Фамилия: {user.LastName}
                    Дата регистрации: {user.CreatedAt:dd.MM.yyyy}
                    """);
    }
}
