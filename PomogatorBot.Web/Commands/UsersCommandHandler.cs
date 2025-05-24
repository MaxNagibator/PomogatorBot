using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class UsersCommandHandler(
    IConfiguration configuration,
    IUserService userService) : IBotCommandHandler, ICommandMetadata
{
    private readonly string _adminUsername = configuration["Admin:Username"]
                                             ?? throw new InvalidOperationException("Имя пользователя администратора не настроено.");

    public static CommandMetadata Metadata { get; } = new("users", "Показать список всех пользователей");

    public string Command => Metadata.Command;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        if (IsAdminMessage(message) == false)
        {
            return new("Вы не администратор.");
        }

        var users = await userService.GetAllUsersAsync(cancellationToken);

        if (users.Count == 0)
        {
            return new("Нет зарегистрированных пользователей.");
        }

        var userRows = users.Select(user =>
        {
            var aliasInfo = string.IsNullOrEmpty(user.Alias) ? string.Empty : $" | Псевдоним: {user.Alias}";
            return $"👤 ID: {user.UserId} | @{user.Username} | {user.FirstName} {user.LastName}{aliasInfo}";
        });

        var usersList = string.Join("\n", userRows);

        var responseText = $"""
                            📋 Список пользователей ({users.Count}):

                            {usersList}

                            Используйте /{SetAliasCommandHandler.Metadata.Command} ID псевдоним для установки псевдонима
                            """;

        return new(responseText);
    }

    private bool IsAdminMessage(Message message)
    {
        return message.From != null
               && string.IsNullOrEmpty(message.From.Username) == false
               && message.From.Username.Equals(_adminUsername, StringComparison.OrdinalIgnoreCase);
    }
}
