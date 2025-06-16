using System.Reflection;

namespace PomogatorBot.Web.Commands.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBotCommandHandlers(this IServiceCollection services, Assembly assembly)
    {
        return services.AddHandlers<IBotCommandHandler>(assembly, RegisterCommandMetadata);
    }

    private static void RegisterCommandMetadata(Type handlerType, IServiceCollection services)
    {
        var metadataInterface = handlerType.GetInterfaces()
            .FirstOrDefault(x => x == typeof(ICommandMetadata));

        if (metadataInterface == null)
        {
            return;
        }

        var property = handlerType.GetProperty(nameof(ICommandMetadata.Metadata), BindingFlags.Public | BindingFlags.Static);

        if (property?.GetValue(null) is CommandMetadata metadata)
        {
            services.AddSingleton(metadata);
        }
    }
}
