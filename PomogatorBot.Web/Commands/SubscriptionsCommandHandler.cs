using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Constants;
using PomogatorBot.Web.Features.Keyboard;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class SubscriptionsCommandHandler(
    UserService userService,
    KeyboardFactory keyboardFactory)
    : UserRequiredCommandHandler(userService), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("subscriptions", "Управление подписками");

    public override string Command => Metadata.Command;

    protected override Task<BotResponse> HandleUserCommandAsync(Message message, PomogatorUser user, CancellationToken cancellationToken)
    {
        var response = new BotResponse($"{Emoji.Settings} Управление подписками:", keyboardFactory.CreateForSubscriptions(user.Subscriptions));
        return Task.FromResult(response);
    }
}
