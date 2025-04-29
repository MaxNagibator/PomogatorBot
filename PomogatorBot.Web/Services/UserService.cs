using Microsoft.EntityFrameworkCore;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Infrastructure.Entities;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace PomogatorBot.Web.Services;

public interface IUserService
{
    ValueTask<User?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task SaveAsync(User entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<NotifyResponse> NotifyAsync(string message, Subscribes subscribes, CancellationToken cancellationToken = default);
}

public class UserService(
    ApplicationDbContext context,
    ITelegramBotClient botClient,
    ILogger<UserService> logger) : IUserService
{
    public ValueTask<User?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return context.Users.FindAsync([id], cancellationToken);
    }

    public async Task SaveAsync(User entity, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(entity.UserId, cancellationToken);

        if (user == null)
        {
            await context.Users.AddAsync(entity, cancellationToken);
        }
        else
        {
            context.Users.Update(user);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(id, cancellationToken);

        if (user == null)
        {
            return false;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(x => x.UserId == id, cancellationToken);
    }

    public async Task<NotifyResponse> NotifyAsync(string message, Subscribes subscribes, CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .Where(x => (x.Subscriptions & subscribes) == subscribes)
            .ToListAsync(cancellationToken);

        var successfulSends = 0;

        foreach (var user in users)
        {
            var messageText = message
                .Replace("<first_name>", user.FirstName)
                .Replace("<username>", user.Username);

            try
            {
                await botClient.SendMessage(user.UserId,
                    messageText,
                    Telegram.Bot.Types.Enums.ParseMode.None,
                    cancellationToken: cancellationToken);

                successfulSends++;
            }
            catch (ApiRequestException exception) when (exception.ErrorCode == 403 && exception.Message.Contains("bot was blocked by the user"))
            {
                logger.LogInformation("Пользователь {UserId} заблокировал бота. Удаляем учетную запись", user.UserId);
                await DeleteAsync(user.UserId, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error sending message to user {UserId}", user.UserId);
            }
        }

        return new(users.Count, successfulSends, users.Count - successfulSends);
    }
}
