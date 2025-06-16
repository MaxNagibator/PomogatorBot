using PomogatorBot.Web.Infrastructure.Entities;

namespace PomogatorBot.Web.Services;

public class MessageTemplateService
{
    public string ReplaceUserVariables(string message, User user)
    {
        return message
            .Replace("<first_name>", user.FirstName, StringComparison.OrdinalIgnoreCase)
            .Replace("<username>", user.Username, StringComparison.OrdinalIgnoreCase)
            .Replace("<alias>", string.IsNullOrEmpty(user.Alias) ? user.FirstName : user.Alias, StringComparison.OrdinalIgnoreCase);
    }

    public string ReplacePreviewVariables(string message)
    {
        // TODO: Переиспользовать ReplaceUserVariables
        return message
            .Replace("<first_name>", "Иван", StringComparison.OrdinalIgnoreCase)
            .Replace("<username>", "@admin", StringComparison.OrdinalIgnoreCase)
            .Replace("<alias>", "Админ", StringComparison.OrdinalIgnoreCase);
    }
}
