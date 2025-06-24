using PomogatorBot.Web.Constants;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using DatabaseUser = PomogatorBot.Web.Infrastructure.Entities.User;

namespace PomogatorBot.Web.CallbackQueries.Common;

public abstract class UserRequiredCallbackQueryHandler(UserService userService) : ICallbackQueryHandler
{
    protected UserService UserService { get; } = userService;

    public abstract bool CanHandle(string callbackData);

    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var user = await UserService.GetAsync(userId, cancellationToken);

        if (user == null)
        {
            return new(Messages.JoinBefore);
        }

        return await HandleUserCallbackAsync(callbackQuery, user, cancellationToken);
    }

    protected abstract Task<BotResponse> HandleUserCallbackAsync(CallbackQuery callbackQuery, DatabaseUser user, CancellationToken cancellationToken);
}
