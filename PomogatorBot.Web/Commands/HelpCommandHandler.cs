using Microsoft.Extensions.Options;
using PomogatorBot.Web.Common.Configuration;
using PomogatorBot.Web.Common.Constants;

namespace PomogatorBot.Web.Commands;

public class HelpCommandHandler(
    IOptions<AdminConfiguration> adminOptions,
    IEnumerable<CommandMetadata> commands)
    : AdminCommandHandler(adminOptions), ICommandMetadata
{
    private readonly IEnumerable<CommandMetadata> _commands = commands
        .Where(x => string.IsNullOrEmpty(x.Description) == false)
        .OrderBy(x => x.Command);

    public static CommandMetadata Metadata { get; } = new("help", "Показать справку");

    public override string Command => Metadata.Command;

    public override Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var userIsAdmin = IsAdminMessage(message);

        var availableCommands = _commands
            .Where(x => x.RequiresAdmin == false || userIsAdmin)
            .Select(x => $"/{x.Command} - {x.Description}");

        var helpText = string.Join("\n", availableCommands);

        if (string.IsNullOrEmpty(helpText))
        {
            helpText = $"{Emoji.Error} Нет доступных команд.";
        }

        return Task.FromResult(new BotResponse(helpText));
    }
}
