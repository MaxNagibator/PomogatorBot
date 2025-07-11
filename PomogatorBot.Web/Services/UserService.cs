using Microsoft.EntityFrameworkCore;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Infrastructure.Entities;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace PomogatorBot.Web.Services;

public class UserService(
    ApplicationDbContext context,
    ITelegramBotClient botClient,
    MessageTemplateService messageTemplateService,
    BroadcastHistoryService broadcastHistoryService,
    ILogger<UserService> logger)
{
    private static readonly LinkPreviewOptions LinkPreviewOptions = new()
    {
        IsDisabled = true,
    };

    public ValueTask<PomogatorUser?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return context.Users.FindAsync([id], cancellationToken);
    }

    public async Task SaveAsync(PomogatorUser entity, CancellationToken cancellationToken = default)
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

    public Task<List<PomogatorUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return context.Users
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetAliasAsync(long id, string alias, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(id, cancellationToken);

        if (user == null)
        {
            return false;
        }

        user.Alias = alias;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> GetCountBySubscriptionAsync(Subscribes subscribes, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .Where(x => (x.Subscriptions & subscribes) == subscribes)
            .CountAsync(cancellationToken);
    }

    public Task<NotifyResponse> NotifyAsync(
        string message,
        Subscribes subscribes,
        MessageEntity[]? entities = null,
        long? adminId = null,
        CancellationToken cancellationToken = default)
    {
        return NotifyInternalAsync(message, subscribes, entities, adminId, null, cancellationToken);
    }

    public Task<NotifyResponse> NotifyWithProgressAsync(
        string message,
        Subscribes subscribes,
        MessageEntity[]? entities = null,
        long? adminId = null,
        Func<int, int, Task>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        return NotifyInternalAsync(message, subscribes, entities, adminId, progressCallback, cancellationToken);
    }

    private async Task<NotifyResponse> NotifyInternalAsync(
        string message,
        Subscribes subscribes,
        MessageEntity[]? entities,
        long? adminId,
        Func<int, int, Task>? progressCallback,
        CancellationToken cancellationToken)
    {
        var queryable = context.Users
            .Where(x => (x.Subscriptions & subscribes) == subscribes);

        if (adminId.HasValue)
        {
            queryable = queryable.Where(x => x.UserId != adminId.Value);
        }

        var users = await queryable
            .ToListAsync(cancellationToken);

        var totalUsers = users.Count;

        var broadcast = await broadcastHistoryService.StartAsync(message, adminId, totalUsers, entities, cancellationToken);

        var successfulSends = 0;
        var failedSends = 0;
        var errorMessages = new List<string>();

        foreach (var user in users)
        {
            var messageText = messageTemplateService.ReplaceUserVariables(message, user);

            try
            {
                await botClient.SendMessage(user.UserId,
                    messageText,
                    linkPreviewOptions: LinkPreviewOptions,
                    replyMarkup: null,
                    entities: entities,
                    cancellationToken: cancellationToken);

                successfulSends++;
            } // TODO: Шляпа
            catch (ApiRequestException exception) when (exception.ErrorCode == 403)
            {
                logger.LogInformation(exception, "Пользователь {UserId} заблокировал бота (ErrorCode: 403). Удаляем учетную запись", user.UserId);

                failedSends++;
                errorMessages.Add($"Пользователь {user.UserId} ({user.Username}) заблокировал бота");

                await DeleteAsync(user.UserId, cancellationToken);
            }
            catch (ApiRequestException exception)
            {
                logger.LogError(exception, "Ошибка API при отправке сообщения пользователю {UserId}: {ErrorCode}", user.UserId, exception.ErrorCode);

                failedSends++;
                errorMessages.Add($"Ошибка API {exception.ErrorCode} для пользователя {user.UserId} ({user.Username})");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Неизвестная ошибка при отправке сообщения пользователю {UserId}", user.UserId);

                failedSends++;
                errorMessages.Add($"Неизвестная ошибка для пользователя {user.UserId} ({user.Username}): {exception.Message}");
            }

            var progressUpdateInterval = progressCallback != null ? 5 : 10;

            if ((successfulSends + failedSends) % progressUpdateInterval != 0 && successfulSends + failedSends != totalUsers)
            {
                continue;
            }

            await broadcastHistoryService.UpdateProgressAsync(broadcast.Id, successfulSends, failedSends, cancellationToken);

            if (progressCallback == null)
            {
                continue;
            }

            try
            {
                await progressCallback(successfulSends, failedSends);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Ошибка в callback прогресса во время рассылки {BroadcastId}", broadcast.Id);
            }
        }

        var errors = string.Join("; ", errorMessages.Take(5)) + (errorMessages.Count > 5 ? "..." : "");

        var errorSummary = errorMessages.Count > 0
            ? errors
            : null;

        await broadcastHistoryService.CompleteAsync(broadcast.Id, successfulSends, failedSends, errorSummary, cancellationToken);

        return new(totalUsers, successfulSends, failedSends);
    }
}
