using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Constants;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Commands;

public class JoinCommandHandler(
    UserService userService,
    ILogger<JoinCommandHandler> logger)
    : IBotCommandHandler, ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("join", "Присоединиться к системе");

    public string Command => Metadata.Command;

    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
    {
        var user = message.From;

        if (user == null)
        {
            return new($"{Emoji.Error} Ошибка идентификации пользователя");
        }

        var existingUser = await userService.GetAsync(user.Id, cancellationToken);

        if (existingUser != null)
        {
            return new($"{Emoji.Success} Вы уже зарегистрированы!");
        }

        var newUser = new PomogatorUser
        {
            UserId = user.Id,
            Username = user.Username ?? "Аноним",
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = DateTime.UtcNow,
            Subscriptions = 0,
        };

        await userService.SaveAsync(newUser, cancellationToken);
        logger.LogInformation("Новый пользователь присоединился: {UserId}", user.Id);

        return new($"Добро пожаловать, {newUser.FirstName}! {Emoji.Party}");
    }
}
