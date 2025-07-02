using Microsoft.Extensions.Options;
using PomogatorBot.Web.Common.Configuration;
using PomogatorBot.Web.Common.Constants;

namespace PomogatorBot.Web.Commands;

public class UsersCommandHandler(
    IOptions<AdminConfiguration> adminOptions,
    UserService userService)
    : AdminRequiredCommandHandler(adminOptions), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("users", "Показать список всех пользователей", true);

    public override string Command => Metadata.Command;

    protected override async Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var users = await userService.GetAllUsersAsync(cancellationToken);

        if (users.Count == 0)
        {
            return new($"{Emoji.Users} Нет зарегистрированных пользователей.");
        }

        var userRows = users.Select(user =>
        {
            var aliasInfo = string.IsNullOrEmpty(user.Alias) ? string.Empty : $" | Псевдоним: {user.Alias}";
            var fullName = $"{user.FirstName} {user.LastName ?? string.Empty}".Trim();
            return $"{Emoji.User} ID: {user.UserId} | @{user.Username} | {fullName}{aliasInfo}";
        });

        var usersList = string.Join("\n", userRows);

        var responseText =
            $"""
             {Emoji.List} Список пользователей ({users.Count}):

             {usersList}

             {Emoji.Bulb} Используйте /{SetAliasCommandHandler.Metadata.Command} ID псевдоним для установки псевдонима
             """;

        return new(responseText);
    }
}
