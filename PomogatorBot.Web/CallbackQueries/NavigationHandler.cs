using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Constants;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.CallbackQueries;

public class NavigationHandler(UserService userService) : ICallbackQueryHandler
{
    public const string MenuBack = "menu_back";
    private const string MenuMain = "menu_main";

    public bool CanHandle(string callbackData)
    {
        return callbackData is MenuBack or MenuMain;
    }

    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;
        var user = await userService.GetAsync(userId, cancellationToken);

        var message = callbackQuery.Data switch
        {
            MenuBack => user == null ? Messages.JoinBefore : $"{Emoji.Home} Главное меню:",
            MenuMain => $"{Emoji.Party} Добро пожаловать в главное меню!",
            _ => $"{Emoji.Error} Неподдерживаемая команда навигации",
        };

        return new(message);
    }
}
