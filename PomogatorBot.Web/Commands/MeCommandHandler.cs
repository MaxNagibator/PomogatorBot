using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using DatabaseUser = PomogatorBot.Web.Infrastructure.Entities.User;

namespace PomogatorBot.Web.Commands;

public class MeCommandHandler(UserService userService) : UserRequiredCommandHandler(userService), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("me", "Показать информацию о себе");

    public override string Command => Metadata.Command;

    protected override Task<BotResponse> HandleUserCommandAsync(Message message, DatabaseUser user, CancellationToken cancellationToken)
    {
        var response = new BotResponse($"""
                                        📋 Ваш профиль:
                                        ID: {user.UserId}
                                        Username: @{user.Username}
                                        Имя: {user.FirstName}
                                        Фамилия: {user.LastName}
                                        Дата регистрации: {user.CreatedAt:dd.MM.yyyy}
                                        """);

        return Task.FromResult(response);
    }
}
