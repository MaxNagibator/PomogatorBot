using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands.Common;

public abstract class AdminRequiredCommandHandler(IConfiguration configuration)
    : AdminCommandHandler(configuration), IBotCommandHandler
{
    public sealed override async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (IsAdminMessage(message) == false)
        {
            return new("Доступ запрещен. Команда доступна только администраторам.");
        }

        return await HandleAdminCommandAsync(message, cancellationToken);
    }

    protected abstract Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken);
}
