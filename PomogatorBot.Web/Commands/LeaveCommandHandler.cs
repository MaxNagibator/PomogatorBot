using PomogatorBot.Web.Common.Constants;
using PomogatorBot.Web.Infrastructure.Entities;

namespace PomogatorBot.Web.Commands;

public class LeaveCommandHandler(UserService userService) : UserRequiredCommandHandler(userService), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("leave", "Покинуть систему");

    public override string Command => Metadata.Command;

    protected override async Task<BotResponse> HandleUserCommandAsync(Message message, PomogatorUser user, CancellationToken cancellationToken)
    {
        await UserService.DeleteAsync(user.UserId, cancellationToken);
        return new($"До свидания, {user.FirstName}! Мы будем скучать {Emoji.Sad}");
    }
}
