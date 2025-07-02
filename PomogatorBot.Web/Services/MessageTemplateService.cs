using PomogatorBot.Web.Common.Constants;
using PomogatorBot.Web.Infrastructure.Entities;

namespace PomogatorBot.Web.Services;

public class MessageTemplateService
{
    public string ReplaceUserVariables(string message, PomogatorUser user)
    {
        return message
            .Replace(TemplateVariables.User.FirstName, user.FirstName, StringComparison.OrdinalIgnoreCase)
            .Replace(TemplateVariables.User.Username, user.Username, StringComparison.OrdinalIgnoreCase)
            .Replace(TemplateVariables.User.Alias, string.IsNullOrEmpty(user.Alias) ? user.FirstName : user.Alias, StringComparison.OrdinalIgnoreCase);
    }

    public string ReplacePreviewVariables(string message)
    {
        return message
            .Replace(TemplateVariables.User.FirstName, TemplateVariables.Preview.FirstName, StringComparison.OrdinalIgnoreCase)
            .Replace(TemplateVariables.User.Username, TemplateVariables.Preview.Username, StringComparison.OrdinalIgnoreCase)
            .Replace(TemplateVariables.User.Alias, TemplateVariables.Preview.Alias, StringComparison.OrdinalIgnoreCase);
    }
}
