namespace PomogatorBot.Web.Services;

public class MessagePreviewService(MessageTemplateService messageTemplateService)
{
    public MessagePreviewResult CreatePreview(string message, MessageEntity[]? entities)
    {
        var previewText = ApplyTemplateVariables(message);
        var adjustedEntities = AdjustEntitiesForPreview(message, previewText, entities);

        return new()
        {
            PreviewText = previewText,
            PreviewEntities = adjustedEntities,
            OriginalMessage = message,
            OriginalEntities = entities,
        };
    }

    private string ApplyTemplateVariables(string message)
    {
        return messageTemplateService.ReplacePreviewVariables(message);
    }

    private MessageEntity[]? AdjustEntitiesForPreview(string originalMessage, string previewMessage, MessageEntity[]? originalEntities)
    {
        if (originalEntities == null || originalEntities.Length == 0)
        {
            return originalEntities;
        }

        var adjustedEntities = new List<MessageEntity>();

        foreach (var entity in originalEntities)
        {
            var adjustedEntity = AdjustEntityOffset(originalMessage, previewMessage, entity);

            if (adjustedEntity != null)
            {
                adjustedEntities.Add(adjustedEntity);
            }
        }

        var result = adjustedEntities.ToArray();
        return MessageEntityHelper.ValidateEntities(result, previewMessage);
    }

    private MessageEntity? AdjustEntityOffset(string originalMessage, string previewMessage, MessageEntity entity)
    {
        var originalOffset = entity.Offset;
        var originalLength = entity.Length;

        if (originalOffset >= originalMessage.Length)
        {
            return null;
        }

        var beforeEntity = originalMessage[..originalOffset];
        var entityText = originalMessage.Substring(originalOffset, Math.Min(originalLength, originalMessage.Length - originalOffset));

        var previewBeforeEntity = ApplyTemplateVariables(beforeEntity);
        var previewEntityText = ApplyTemplateVariables(entityText);

        var newOffset = previewBeforeEntity.Length;
        var newLength = previewEntityText.Length;

        if (newOffset >= previewMessage.Length || newLength <= 0)
        {
            return null;
        }

        return MessageEntityHelper.CreateCopy(entity, newOffset, newLength);
    }
}
