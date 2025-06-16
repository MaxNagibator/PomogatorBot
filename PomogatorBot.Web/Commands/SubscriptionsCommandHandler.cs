using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class SubscriptionsCommandHandler(
    UserService userService,
    KeyboardFactory keyboardFactory) : IBotCommandHandler, ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("subscriptions", "Управление подписками");

    public string Command => Metadata.Command;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var validationError = message.ValidateUser(out var userId);

        if (validationError != null)
        {
            return validationError;
        }

        var user = await userService.GetAsync(userId, cancellationToken);

        if (user == null)
        {
            return new(Messages.JoinBefore);
        }

        return new("Управление подписками:", keyboardFactory.CreateForSubscriptions(user.Subscriptions));
    }
}
