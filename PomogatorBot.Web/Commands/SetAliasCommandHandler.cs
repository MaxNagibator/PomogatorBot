using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class SetAliasCommandHandler(
    IConfiguration configuration,
    UserService userService,
    ILogger<SetAliasCommandHandler> logger)
    : AdminRequiredCommandHandler(configuration), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("setalias", "Установить псевдоним для пользователя", true);

    public override string Command => Metadata.Command;

    protected override async Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var length = Metadata.Command.Length + 1;

        if (message.Text?.Length <= length)
        {
            return new(GetHelpMessage(), new());
        }

        var messageText = message.Text?[length..]?.Trim();

        if (string.IsNullOrEmpty(messageText))
        {
            return new(GetHelpMessage(), new());
        }

        var parts = messageText.Split(' ', 2);

        if (parts.Length < 2)
        {
            return new("❗ Необходимо указать ID пользователя и псевдоним.", new());
        }

        if (long.TryParse(parts[0], out var userId) == false)
        {
            return new("❌ Некорректный ID пользователя. Используйте числовое значение.", new());
        }

        var alias = parts[1].Trim();

        if (string.IsNullOrWhiteSpace(alias))
        {
            return new("❗ Псевдоним не может быть пустым.", new());
        }

        var success = await userService.SetAliasAsync(userId, alias, cancellationToken);

        if (success == false)
        {
            return new($"❌ Пользователь с ID {userId} не найден.", new());
        }

        logger.LogInformation("Установлен псевдоним для пользователя {UserId}: {Alias}", userId, alias);
        return new($"✅ Псевдоним '{alias}' успешно установлен для пользователя {userId}.", new());
    }

    private static string GetHelpMessage()
    {
        const string Message = """
                               🏷️ Справка по команде установки псевдонима:

                               Использование:
                               /setalias ID_пользователя псевдоним

                               📝 Пример:
                               /setalias 123456789 Василий
                               """;

        return Message;
    }
}
