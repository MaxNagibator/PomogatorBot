﻿using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.CallbackQueries;

public class NavigationHandler(IUserService userService) : ICallbackQueryHandler
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
            MenuBack => user == null ? Messages.JoinBefore : "Главное меню:",
            MenuMain => "Добро пожаловать в главное меню!",
            _ => "Неподдерживаемая команда навигации",
        };

        return new(message);
    }
}
