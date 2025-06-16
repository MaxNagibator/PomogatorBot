using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands.Common;

public interface IBotCommandHandler
{
    string Command { get; }
    Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken);
}
