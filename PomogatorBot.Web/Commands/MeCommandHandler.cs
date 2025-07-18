﻿using PomogatorBot.Web.Common.Constants;
using PomogatorBot.Web.Infrastructure.Entities;

namespace PomogatorBot.Web.Commands;

public class MeCommandHandler(UserService userService) : UserRequiredCommandHandler(userService), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("me", "Показать информацию о себе");

    public override string Command => Metadata.Command;

    protected override Task<BotResponse> HandleUserCommandAsync(Message message, PomogatorUser user, CancellationToken cancellationToken)
    {
        var responseText =
            $"""
             {Emoji.List} Ваш профиль:
             ID: {user.UserId}
             Username: @{user.Username}
             Имя: {user.FirstName}
             Фамилия: {user.LastName ?? "Не указана"}
             Дата регистрации: {user.CreatedAt:dd.MM.yyyy}
             """;

        var response = new BotResponse(responseText);
        return Task.FromResult(response);
    }
}
