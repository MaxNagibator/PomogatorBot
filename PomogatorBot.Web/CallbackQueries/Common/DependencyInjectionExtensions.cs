using System.Reflection;

namespace PomogatorBot.Web.CallbackQueries.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBotCallbackQueryHandlers(this IServiceCollection services, Assembly assembly)
    {
        return services.AddHandlers<ICallbackQueryHandler>(assembly);
    }
}
