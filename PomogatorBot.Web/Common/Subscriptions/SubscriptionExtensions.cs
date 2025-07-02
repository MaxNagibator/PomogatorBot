using System.ComponentModel;
using System.Reflection;

namespace PomogatorBot.Web.Common.Subscriptions;

public static class SubscriptionExtensions
{
    private static readonly Lazy<Dictionary<Subscribes, SubscriptionMeta>> CachedMetadata =
        new(InitializeMetadata, LazyThreadSafetyMode.ExecutionAndPublication);

    public static Dictionary<Subscribes, SubscriptionMeta> SubscriptionMetadata => CachedMetadata.Value;

    private static Dictionary<Subscribes, SubscriptionMeta> InitializeMetadata()
    {
        var result = new Dictionary<Subscribes, SubscriptionMeta>();
        var type = typeof(Subscribes);

        foreach (Subscribes value in Enum.GetValues(type))
        {
            var member = type.GetMember(value.ToString())[0];
            var descriptionAttribute = member.GetCustomAttribute<DescriptionAttribute>();
            var metaAttribute = member.GetCustomAttribute<SubscriptionMetaAttribute>();

            if (metaAttribute == null)
            {
                throw new InvalidOperationException($"The type {type} is not marked with {nameof(SubscriptionMetaAttribute)}");
            }

            result[value] = new()
            {
                Subscription = value,
                DisplayName = metaAttribute.DisplayName,
                Description = descriptionAttribute?.Description ?? value.ToString(),
                Color = metaAttribute.Color,
                Icon = metaAttribute.Icon,
            };
        }

        return result;
    }
}
