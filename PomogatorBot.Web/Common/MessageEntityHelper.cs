namespace PomogatorBot.Web.Common;

/// <summary>
/// Утилитарный класс для работы с MessageEntity
/// </summary>
/// <remarks>
/// Предоставляет методы для корректной обработки сущностей форматирования сообщений Telegram.
/// </remarks>
public static class MessageEntityHelper
{
    /// <summary>
    /// Смещает offset всех entities на указанное количество символов
    /// </summary>
    /// <param name="entities">Массив сущностей для смещения</param>
    /// <param name="offset">Количество символов для смещения (может быть отрицательным)</param>
    /// <returns>Новый массив сущностей со смещенными offset или null, если исходный массив null/пустой</returns>
    /// <remarks>
    /// Используется при добавлении префиксов к сообщениям или объединении нескольких текстов.
    /// Автоматически фильтрует entities с некорректными offset после смещения.
    /// </remarks>
    public static MessageEntity[]? OffsetEntities(MessageEntity[]? entities, int offset)
    {
        if (entities == null || entities.Length == 0 || offset == 0)
        {
            return entities;
        }

        var adjustedEntities = new List<MessageEntity>();

        foreach (var entity in entities)
        {
            var newOffset = entity.Offset + offset;

            if (newOffset < 0)
            {
                continue;
            }

            adjustedEntities.Add(CreateCopy(entity, newOffset, entity.Length));
        }

        return adjustedEntities.Count > 0 ? [.. adjustedEntities] : null;
    }

    /// <summary>
    /// Адаптирует entities для обрезанного сообщения, корректно обрабатывая границы
    /// </summary>
    /// <param name="entities">Исходные сущности</param>
    /// <param name="originalMessage">Исходный текст сообщения</param>
    /// <param name="truncatedMessage">Обрезанный текст сообщения</param>
    /// <param name="additionalOffset">Дополнительное смещение для применения к результирующим entities</param>
    /// <returns>Адаптированные entities или null, если нет подходящих</returns>
    /// <remarks>
    /// Обрабатывает случаи, когда сообщение обрезается и некоторые entities могут:
    /// - Полностью выходить за границы обрезанного текста
    /// - Частично пересекаться с границей обрезки
    /// - Полностью помещаться в обрезанный текст
    /// Автоматически корректирует длину entities, которые пересекают границу обрезки.
    /// </remarks>
    public static MessageEntity[]? AdaptEntitiesForTruncatedMessage(
        MessageEntity[]? entities,
        string originalMessage,
        string truncatedMessage,
        int additionalOffset = 0)
    {
        if (entities == null || entities.Length == 0)
        {
            return null;
        }

        if (string.IsNullOrEmpty(truncatedMessage))
        {
            return null;
        }

        var adaptedEntities = new List<MessageEntity>();

        foreach (var entity in entities)
        {
            if (entity.Offset >= truncatedMessage.Length)
            {
                continue;
            }

            var maxLength = truncatedMessage.Length - entity.Offset;
            var adjustedLength = Math.Min(entity.Length, maxLength);

            if (adjustedLength <= 0)
            {
                continue;
            }

            var originalMaxLength = originalMessage.Length - entity.Offset;

            if (originalMaxLength <= 0)
            {
                continue;
            }

            adaptedEntities.Add(CreateCopy(entity, entity.Offset + additionalOffset, adjustedLength));
        }

        return adaptedEntities.Count > 0 ? [.. adaptedEntities] : null;
    }

    /// <summary>
    /// Валидирует entities относительно текста сообщения
    /// </summary>
    /// <param name="entities">Сущности для валидации</param>
    /// <param name="messageText">Текст сообщения</param>
    /// <returns>Массив валидных entities или null, если нет валидных</returns>
    /// <remarks>
    /// Проверяет, что все entities:
    /// - Имеют корректные offset и length
    /// - Не выходят за границы текста сообщения
    /// - Имеют положительную длину
    /// Используется для очистки entities после различных трансформаций текста.
    /// </remarks>
    public static MessageEntity[]? ValidateEntities(MessageEntity[]? entities, string messageText)
    {
        if (entities == null || entities.Length == 0 || string.IsNullOrEmpty(messageText))
        {
            return null;
        }

        var validEntities = new List<MessageEntity>();

        foreach (var entity in entities)
        {
            if (entity.Offset < 0 || entity.Length <= 0 || entity.Offset >= messageText.Length || entity.Offset + entity.Length > messageText.Length)
            {
                continue;
            }

            validEntities.Add(entity);
        }

        return validEntities.Count > 0 ? [.. validEntities] : null;
    }

    /// <summary>
    /// Создает копию MessageEntity с новыми значениями offset и length
    /// </summary>
    /// <param name="original">Исходная сущность</param>
    /// <param name="newOffset">Новое значение offset</param>
    /// <param name="newLength">Новое значение length</param>
    /// <returns>Новая сущность с обновленными значениями</returns>
    /// <remarks>
    /// Упрощает создание копий MessageEntity с сохранением всех дополнительных свойств.
    /// Используется для избежания дублирования кода при создании новых entities.
    /// </remarks>
    public static MessageEntity CreateCopy(MessageEntity original, int newOffset, int newLength)
    {
        return new()
        {
            Type = original.Type,
            Offset = newOffset,
            Length = newLength,
            Url = original.Url,
            User = original.User,
            Language = original.Language,
            CustomEmojiId = original.CustomEmojiId,
        };
    }
}
