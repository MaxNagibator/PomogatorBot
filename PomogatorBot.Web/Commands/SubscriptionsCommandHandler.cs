using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using DatabaseUser = PomogatorBot.Web.Infrastructure.Entities.User;

namespace PomogatorBot.Web.Commands;

public class SubscriptionsCommandHandler(
    UserService userService,
    KeyboardFactory keyboardFactory)
    : UserRequiredCommandHandler(userService), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("subscriptions", "Управление подписками");

    public override string Command => Metadata.Command;

    protected override Task<BotResponse> HandleUserCommandAsync(Message message, DatabaseUser user, CancellationToken cancellationToken)
    {
        var response = new BotResponse("Управление подписками:", keyboardFactory.CreateForSubscriptions(user.Subscriptions));
        return Task.FromResult(response);
    }
}
