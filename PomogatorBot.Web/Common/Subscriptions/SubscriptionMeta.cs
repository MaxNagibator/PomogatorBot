namespace PomogatorBot.Web.Common.Subscriptions;

public class SubscriptionMeta
{
    public required Subscribes Subscription { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public required string Icon { get; init; }
}
