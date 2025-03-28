﻿using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Common;

public record BotResponse(string Message, InlineKeyboardMarkup? KeyboardMarkup = null);
