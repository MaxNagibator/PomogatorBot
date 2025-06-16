using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using DatabaseUser = PomogatorBot.Web.Infrastructure.Entities.User;

namespace PomogatorBot.Web.Commands.Common;

public abstract class UserRequiredCommandHandler(UserService userService) : IBotCommandHandler
{
    public abstract string Command { get; }

    protected UserService UserService { get; } = userService;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var validationError = message.ValidateUser(out var userId);

        if (validationError != null)
        {
            return validationError;
        }

        var user = await UserService.GetAsync(userId, cancellationToken);

        if (user == null)
        {
            return new(Messages.JoinBefore);
        }

        return await HandleUserCommandAsync(message, user, cancellationToken);
    }

    protected abstract Task<BotResponse> HandleUserCommandAsync(Message message, DatabaseUser user, CancellationToken cancellationToken);
}
