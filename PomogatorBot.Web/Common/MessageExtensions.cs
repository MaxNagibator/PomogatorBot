using PomogatorBot.Web.Constants;
using Telegram.Bot.Types;

namespace PomogatorBot.Web.Common;

public static class MessageExtensions
{
    // TODO: Сомнительно. Подумать
    public static BotResponse? ValidateUser(this Message message, out long userId)
    {
        userId = 0;

        if (message.From?.Id == null)
        {
            return new($"{Emoji.Error} Ошибка идентификации пользователя");
        }

        userId = message.From.Id;
        return null;
    }
}
