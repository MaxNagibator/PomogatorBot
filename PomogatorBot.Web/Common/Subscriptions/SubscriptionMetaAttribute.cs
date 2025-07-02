namespace PomogatorBot.Web.Common.Subscriptions;

[AttributeUsage(AttributeTargets.Field)]
public sealed class SubscriptionMetaAttribute(string displayName, string color, string icon) : Attribute
{
    public string DisplayName { get; } = displayName;
    public string Color { get; } = color;
    public string Icon { get; } = icon;
}
