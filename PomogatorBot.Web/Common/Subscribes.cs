namespace PomogatorBot.Web.Common;

[Flags]
public enum Subscribes
{
    [SubscriptionMeta("Нет подписок", "#808080", "🚫")]
    None = 0,

    [SubscriptionMeta("Образовательные трансляции", "#00bcd4", "☕")]
    EducationStreams = 1,

    [SubscriptionMeta("Игровые трансляции", "#ff6699", "🎮")]
    GameStreams = 1 << 1,

    //[SubscriptionMeta("Доброе утро", "#ffcc00", "🌅")]
    //DobroeUtro = 1 << 2,

    //[SubscriptionMeta("Spoki Noki", "#9c27b0", "🌙")]
    //SpokiNoki = 1 << 3,

    [SubscriptionMeta("Все подписки", "#4CAF50", "✅")]
    All = EducationStreams | GameStreams //| DobroeUtro | SpokiNoki,
}
