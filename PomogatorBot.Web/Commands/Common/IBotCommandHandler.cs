using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands.Common;

public interface IBotCommandHandler
{
    string Command { get; }
    Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken);
}

public abstract class BotAdminCommandHandler(IConfiguration configuration) : IBotCommandHandler
{
    private readonly string _adminUsername = configuration["Admin:Username"]
                                             ?? throw new InvalidOperationException("Имя пользователя администратора не настроено.");

    public abstract string Command { get; }

    public abstract Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken);

    protected bool IsAdminMessage(Message message)
    {
        return message.From != null
               && string.IsNullOrEmpty(message.From.Username) == false
               && message.From.Username.Equals(_adminUsername, StringComparison.OrdinalIgnoreCase);
    }
}
