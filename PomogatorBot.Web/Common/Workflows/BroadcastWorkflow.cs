using PomogatorBot.Web.Common.Keyboard;

namespace PomogatorBot.Web.Common.Workflows;

public static class BroadcastWorkflow
{
    public const string Name = "broadcast";

    public static WorkflowDefinition Create(KeyboardFactory keyboardFactory)
    {
        return new WorkflowDefinition
            {
                Name = Name,
                OnComplete = async (context, serviceProvider, cancellationToken) =>
                {
                    // TODO: Подумать
                    var broadcastPendingService = serviceProvider.GetRequiredService<BroadcastPendingService>();
                    var notificationService = serviceProvider.GetRequiredService<BroadcastNotificationService>();
                    var broadcastPreviewService = serviceProvider.GetRequiredService<BroadcastPreviewService>();

                    var message = context.TryGetData<string>("message")!;
                    var entities = context.TryGetData<MessageEntity[]>("entities");
                    var subscribes = context.TryGetData<Subscribes>("subscribes");
                    var adminUserId = context.TryGetData<long>("adminUserId");

                    var preview = await broadcastPreviewService.BuildPreviewAsync(message,
                        subscribes,
                        entities,
                        () => keyboardFactory.CreateForBroadcastConfirmation(string.Empty),
                        adminUserId,
                        cancellationToken);

                    var pendingId = broadcastPendingService.Store(message, subscribes, entities, adminUserId);

                    var keyboard = keyboardFactory.CreateForBroadcastConfirmation(pendingId);

                    return new(preview.Message, keyboard, preview.Entities, (chatId, messageId) =>
                    {
                        notificationService.Add(pendingId,
                            adminUserId,
                            chatId,
                            messageId,
                            DateTime.UtcNow,
                            DateTime.UtcNow.AddHours(1),
                            preview.Message,
                            preview.Entities,
                            keyboard);
                    });
                },
            }
            .AddStep(new MessageInputStep())
            .AddStep(new CategorySelectionStep(keyboardFactory));
    }
}
