﻿using PomogatorBot.Web.Common.Constants;

namespace PomogatorBot.Web.Commands;

public class DefaultCommandHandler(UserService userService) : IBotCommandHandler
{
    public string Command => string.Empty;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var validationError = message.ValidateUser(out var userId);

        if (validationError != null)
        {
            return validationError;
        }

        var exists = await userService.ExistsAsync(userId, cancellationToken);

        return exists
            ? new($"{Emoji.Question} Не понимаю команду. Используйте /{HelpCommandHandler.Metadata.Command} для списка команд")
            : new BotResponse($"{Emoji.Rocket} Для начала работы выполните /{JoinCommandHandler.Metadata.Command}");
    }
}
