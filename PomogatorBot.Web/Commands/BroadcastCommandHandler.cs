using Microsoft.Extensions.Options;
using PomogatorBot.Web.Common.Configuration;
using PomogatorBot.Web.Common.Constants;
using PomogatorBot.Web.Common.Keyboard;
using PomogatorBot.Web.Common.Workflows;
using Telegram.Bot.Types.Enums;

namespace PomogatorBot.Web.Commands;

public class BroadcastCommandHandler(
    IOptions<AdminConfiguration> adminOptions,
    KeyboardFactory keyboardFactory,
    BroadcastPendingService broadcastPendingService,
    BroadcastNotificationService notificationService,
    BroadcastPreviewService broadcastPreviewService,
    WorkflowService workflowService)
    : AdminRequiredCommandHandler(adminOptions), ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("b", "Возвестить пастве", true);

    public override string Command => Metadata.Command;

    protected override async Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
    {
        var length = Metadata.Command.Length + 1;

        if (string.IsNullOrWhiteSpace(message.Text) || message.Text.Length <= length)
        {
            return await workflowService.StartWorkflowAsync(message.From!.Id, BroadcastWorkflow.Name, cancellationToken);
        }

        var parts = message.Text.Split(" ", 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            return new(GetHelpMessage(), new());
        }

        var args = parts[1];
        var broadcastMessage = parts[2];

        Subscribes subscribes;

        try
        {
            subscribes = ParseSubscriptions(args);
        }
        catch (Exception exception)
        {
            return new(exception.Message);
        }

        var entitiesOffset = message.Text.IndexOf(']', StringComparison.Ordinal) + 2;

        var filteredEntities = message.Entities?
            .SkipWhile(x => x.Type == MessageEntityType.BotCommand)
            .ToArray();

        var entities = MessageEntityHelper.OffsetEntities(filteredEntities, -entitiesOffset);

        var preview = await broadcastPreviewService.BuildPreviewAsync(broadcastMessage,
            subscribes,
            entities,
            () => keyboardFactory.CreateForBroadcastConfirmation(string.Empty),
            message.From!.Id,
            cancellationToken);

        var pendingId = broadcastPendingService.Store(broadcastMessage, subscribes, entities, message.From!.Id);

        var keyboard = keyboardFactory.CreateForBroadcastConfirmation(pendingId);

        return new(preview.Message, keyboard, preview.Entities, OnMessageSent);

        void OnMessageSent(long chatId, int messageId)
        {
            notificationService.Add(pendingId,
                message.From!.Id,
                chatId,
                messageId,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                preview.Message,
                preview.Entities,
                keyboard);
        }
    }

    private static string GetHelpMessage()
    {
        var subscribes = SubscriptionExtensions.SubscriptionMetadata
            .Values
            .Where(x => x.Subscription != Subscribes.All)
            .Select(x => $"{Emoji.Bullet} {x.Subscription}");

        var message = $"""
                       📢 Справка по команде рассылки:

                       Использование:
                       /b [подписки_через_запятую] сообщение

                       Доступные подписки:
                       {string.Join(Environment.NewLine, subscribes)}

                       Доступные теги:
                       {TemplateVariables.User.FirstName} - имя пользователя
                       {TemplateVariables.User.Username}- ник пользователя
                       {TemplateVariables.User.Alias} - псевдоним пользователя (если нет, то имя)

                       {Emoji.Important}При {Subscribes.None} отправится всем пользователям
                       {Emoji.Important}При отсутствии подписок отправится всем пользователям
                       """;

        return message;
    }

    private static Subscribes ParseSubscriptions(string args)
    {
        if (!args.StartsWith('[') || !args.EndsWith(']'))
        {
            throw new ArgumentException($"{Emoji.Error} Не найдены скобки [ или ]. Используйте формат: [подписки]");
        }

        var subscriptionParam = args.Trim('[', ']');

        var result = Subscribes.None;

        if (string.IsNullOrWhiteSpace(subscriptionParam))
        {
            return result;
        }

        var parts = subscriptionParam.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var part in parts)
        {
            if (Enum.TryParse<Subscribes>(part, true, out var subscription))
            {
                result |= subscription;
            }
            else
            {
                throw new ArgumentException($"{Emoji.Error} Неизвестная подписка: '{part}'. Проверьте доступные подписки в справке.");
            }
        }

        return result;
    }
}
