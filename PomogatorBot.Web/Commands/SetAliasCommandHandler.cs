using Microsoft.Extensions.Options;
using PomogatorBot.Web.Common.Configuration;
using PomogatorBot.Web.Common.Constants;

namespace PomogatorBot.Web.Commands;

public class SetAliasCommandHandler(
    IOptions<AdminConfiguration> adminOptions,
    UserService userService,
    ILogger<SetAliasCommandHandler> logger)
    : AdminRequiredCommandHandler(adminOptions), ICommandMetadata
{
    private const string HelpMessage =
        $"""
         {Emoji.Tag} Справка по команде установки псевдонима:

         Использование:
         /setalias ID_пользователя псевдоним

         {Emoji.Memo} Пример:
         /setalias 123456789 Василий
         """;

    public static CommandMetadata Metadata { get; } = new("setalias", "Установить псевдоним для пользователя", true);

    public override string Command => Metadata.Command;

    protected override async Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var length = Metadata.Command.Length + 1;

        if (message.Text?.Length <= length)
        {
            return new(HelpMessage, new());
        }

        var messageText = message.Text?[length..]?.Trim();

        if (string.IsNullOrEmpty(messageText))
        {
            return new(HelpMessage, new());
        }

        var parts = messageText.Split(' ', 2);

        if (parts.Length < 2)
        {
            return new($"{Emoji.Important} Необходимо указать ID пользователя и псевдоним.", new());
        }

        if (long.TryParse(parts[0], out var userId) == false)
        {
            return new($"{Emoji.Error} Некорректный ID пользователя. Используйте числовое значение.", new());
        }

        var alias = parts[1].Trim();

        if (string.IsNullOrWhiteSpace(alias))
        {
            return new($"{Emoji.Important} Псевдоним не может быть пустым.", new());
        }

        var success = await userService.SetAliasAsync(userId, alias, cancellationToken);

        if (success == false)
        {
            return new($"{Emoji.Error} Пользователь с ID {userId} не найден.", new());
        }

        logger.LogInformation("Установлен псевдоним для пользователя {UserId}: {Alias}", userId, alias);
        return new($"{Emoji.Success} Псевдоним '{alias}' успешно установлен для пользователя {userId}.", new());
    }
}
