using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Web.Common;

public record BotResponse(
    string Message,
    InlineKeyboardMarkup? KeyboardMarkup = null,
    MessageEntity[]? Entities = null,
    Action<long, int>? OnMessageSent = null);
