using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Commands;
using PomogatorBot.Web.Common.Constants;
using PomogatorBot.Web.Common.Keyboard;

namespace PomogatorBot.Web.CallbackQueries;

public class LastMessagesCallbackHandler(
    BroadcastHistoryService broadcastHistoryService,
    KeyboardFactory keyboardFactory)
    : ICallbackQueryHandler
{
    public const string ShowPrefix = "lastmsg_show_";
    public const string RefreshAction = "lastmsg_refresh";

    private const int DefaultCount = 1;

    public bool CanHandle(string callbackData)
    {
        return callbackData.StartsWith(ShowPrefix, StringComparison.OrdinalIgnoreCase)
               || callbackData.Equals(RefreshAction, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        // TODO: Подумать над проверкой на администратора

        var callbackData = callbackQuery.Data!;

        return callbackData switch
        {
            _ when callbackData.StartsWith(ShowPrefix, StringComparison.OrdinalIgnoreCase) => await HandleShowMessages(callbackData, cancellationToken),
            RefreshAction => await HandleRefresh(cancellationToken),
            _ => new($"{Emoji.Error} Неизвестная команда."),
        };
    }

    private async Task<BotResponse> HandleShowMessages(string callbackData, CancellationToken cancellationToken)
    {
        if (CallbackDataParser.TryParseWithPrefix(callbackData, ShowPrefix, out var countStr) == false
            || int.TryParse(countStr, out var count) == false)
        {
            return new($"{Emoji.Error} Неверный формат команды.");
        }

        var lastBroadcasts = await broadcastHistoryService.GetLastsAsync(count, cancellationToken);
        var statistics = await broadcastHistoryService.GetStatisticsAsync(cancellationToken);

        if (lastBroadcasts.Count == 0)
        {
            var emptyResponse = $"{Emoji.Info} История рассылок пуста.";
            var emptyKeyboard = keyboardFactory.CreateForLastMessages();
            return new(emptyResponse, emptyKeyboard);
        }

        var (responseText, responseEntities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(lastBroadcasts, statistics, count);
        var keyboard = keyboardFactory.CreateForLastMessages();

        return new(responseText, keyboard, responseEntities);
    }

    private async Task<BotResponse> HandleRefresh(CancellationToken cancellationToken)
    {
        var lastBroadcasts = await broadcastHistoryService.GetLastsAsync(DefaultCount, cancellationToken);
        var statistics = await broadcastHistoryService.GetStatisticsAsync(cancellationToken);

        if (lastBroadcasts.Count == 0)
        {
            var emptyResponse = $"{Emoji.Refresh} Обновлено. История рассылок пуста.";
            var emptyKeyboard = keyboardFactory.CreateForLastMessages();
            return new(emptyResponse, emptyKeyboard);
        }

        var (broadcastsText, broadcastsEntities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(lastBroadcasts, statistics, DefaultCount);
        var refreshPrefix = $"{Emoji.Refresh} Обновлено.\n\n";
        var responseText = refreshPrefix + broadcastsText;

        var responseEntities = MessageEntityHelper.OffsetEntities(broadcastsEntities, refreshPrefix.Length);

        var keyboard = keyboardFactory.CreateForLastMessages();

        return new(responseText, keyboard, responseEntities);
    }
}
