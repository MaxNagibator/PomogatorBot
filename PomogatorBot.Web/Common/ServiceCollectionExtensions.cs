using System.Reflection;

namespace PomogatorBot.Web.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers<T>(this IServiceCollection services, Assembly assembly, Action<Type, IServiceCollection>? additionalRegistration = null)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false } && x.IsAssignableTo(typeof(T)));

        foreach (var type in handlerTypes)
        {
            services.AddScoped(typeof(T), type);
            additionalRegistration?.Invoke(type, services);
        }

        return services;
    }
}
