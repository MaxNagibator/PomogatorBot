using PomogatorBot.Web.Utils;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PomogatorBot.Tests.Utils;

[TestFixture]
public class MessageEntityHelperTests
{
    /// <summary>
    /// Метод OffsetEntities корректно смещает offset всех entities на указанное количество символов.
    /// </summary>
    /// <remarks>
    /// Проверяет смещение offset для различных типов entities.<br />
    /// Проверяет обработку положительных и отрицательных смещений.<br />
    /// Проверяет фильтрацию entities с некорректными offset после смещения.<br />
    /// Проверяет сохранение всех дополнительных свойств entities при копировании.<br />
    /// Проверяет обработку null и пустых массивов entities.<br />
    /// Проверяет корректность создания нового массива entities.
    /// </remarks>
    [TestCase(5, 3, 8)]
    [TestCase(10, -3, 7)]
    [TestCase(0, 5, 5)]
    [TestCase(15, 0, 15)]
    public void OffsetEntitiesWithValidOffsetShouldAdjustCorrectly(int originalOffset, int offsetAdjustment, int expectedOffset)
    {
        // Arrange
        var entities = new[]
        {
            new MessageEntity
            {
                Type = MessageEntityType.Bold,
                Offset = originalOffset,
                Length = 5,
                Url = "https://example.com",
                Language = "ru",
            },
        };

        // Act
        var result = MessageEntityHelper.OffsetEntities(entities, offsetAdjustment);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result[0].Offset, Is.EqualTo(expectedOffset));
            Assert.That(result[0].Length, Is.EqualTo(5));
            Assert.That(result[0].Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(result[0].Url, Is.EqualTo("https://example.com"));
            Assert.That(result[0].Language, Is.EqualTo("ru"));
        }
    }

    /// <summary>
    /// Метод OffsetEntities фильтрует entities с отрицательными offset после смещения.
    /// </summary>
    /// <remarks>
    /// Проверяет исключение entities, которые получают отрицательный offset после смещения.<br />
    /// Проверяет корректную обработку смешанных случаев с валидными и невалидными entities.<br />
    /// Проверяет возврат null при отсутствии валидных entities после фильтрации.<br />
    /// Проверяет сохранение только валидных entities в результирующем массиве.
    /// </remarks>
    [Test]
    public void OffsetEntitiesWithNegativeResultShouldFilterInvalidEntities()
    {
        // Arrange
        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 2, Length = 3 },
            new MessageEntity { Type = MessageEntityType.Italic, Offset = 8, Length = 4 },
            new MessageEntity { Type = MessageEntityType.Code, Offset = 1, Length = 2 },
        };

        // Act
        var result = MessageEntityHelper.OffsetEntities(entities, -5);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(1));
            Assert.That(result![0].Type, Is.EqualTo(MessageEntityType.Italic));
            Assert.That(result[0].Offset, Is.EqualTo(3));
            Assert.That(result[0].Length, Is.EqualTo(4));
        }
    }

    /// <summary>
    /// Метод OffsetEntities корректно обрабатывает граничные случаи с null и пустыми массивами.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null для null входного массива.<br />
    /// Проверяет возврат исходного массива для пустого входного массива.<br />
    /// Проверяет возврат исходного массива при нулевом смещении.<br />
    /// Проверяет возврат null при полной фильтрации всех entities.
    /// </remarks>
    [TestCase(null, 5)]
    public void OffsetEntitiesWithEdgeCasesShouldHandleCorrectly(MessageEntity[]? entities, int offset)
    {
        // Act
        var result = MessageEntityHelper.OffsetEntities(entities, offset);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод OffsetEntities корректно обрабатывает пустой массив entities.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат исходного массива для пустого входного массива.<br />
    /// Проверяет корректную обработку пустого массива без создания копии.<br />
    /// Проверяет оптимизацию для случая отсутствия entities для обработки.
    /// </remarks>
    [Test]
    public void OffsetEntitiesWithEmptyArrayShouldReturnSameArray()
    {
        // Arrange
        var emptyEntities = Array.Empty<MessageEntity>();

        // Act
        var result = MessageEntityHelper.OffsetEntities(emptyEntities, 5);

        // Assert
        Assert.That(result, Is.SameAs(emptyEntities));
    }

    /// <summary>
    /// Метод OffsetEntities возвращает исходный массив при нулевом смещении.
    /// </summary>
    /// <remarks>
    /// Проверяет оптимизацию для случая нулевого смещения.<br />
    /// Проверяет возврат точно того же объекта массива без создания копии.<br />
    /// Проверяет сохранение всех свойств entities без изменений.
    /// </remarks>
    [Test]
    public void OffsetEntitiesWithZeroOffsetShouldReturnOriginalArray()
    {
        // Arrange
        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 5, Length = 3 },
        };

        // Act
        var result = MessageEntityHelper.OffsetEntities(entities, 0);

        // Assert
        Assert.That(result, Is.SameAs(entities));
    }

    /// <summary>
    /// Метод OffsetEntities возвращает null когда все entities фильтруются.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null при полной фильтрации всех entities.<br />
    /// Проверяет корректную обработку случая, когда все entities получают отрицательный offset.<br />
    /// Проверяет оптимизацию памяти при отсутствии валидных результатов.
    /// </remarks>
    [Test]
    public void OffsetEntitiesWithAllEntitiesFilteredShouldReturnNull()
    {
        // Arrange
        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 1, Length = 2 },
            new MessageEntity { Type = MessageEntityType.Italic, Offset = 2, Length = 3 },
        };

        // Act
        var result = MessageEntityHelper.OffsetEntities(entities, -10);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод AdaptEntitiesForTruncatedMessage корректно адаптирует entities для обрезанного сообщения.
    /// </summary>
    /// <remarks>
    /// Проверяет адаптацию entities при обрезке сообщения с различными границами.<br />
    /// Проверяет корректное применение дополнительного смещения к результирующим entities.<br />
    /// Проверяет обработку entities, которые полностью помещаются в обрезанный текст.<br />
    /// Проверяет обработку entities, которые частично пересекаются с границей обрезки.<br />
    /// Проверяет фильтрацию entities, которые полностью выходят за границы обрезанного текста.<br />
    /// Проверяет корректное вычисление новой длины для пересекающихся entities.
    /// </remarks>
    [Test]
    public void AdaptEntitiesForTruncatedMessageWithValidDataShouldAdaptCorrectly()
    {
        // Arrange
        const string OriginalMessage = "Это тестовое сообщение для проверки адаптации entities";
        const string TruncatedMessage = "Это тестовое сообщение";

        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 4, Length = 8 }, // "тестовое" - полностью в обрезанном
            new MessageEntity { Type = MessageEntityType.Italic, Offset = 13, Length = 15 }, // "сообщение для п" - частично обрезано
            new MessageEntity { Type = MessageEntityType.Code, Offset = 30, Length = 10 }, // полностью за границей
        };

        const int AdditionalOffset = 5;

        // Act
        var result = MessageEntityHelper.AdaptEntitiesForTruncatedMessage(entities, OriginalMessage, TruncatedMessage, AdditionalOffset);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(2));

            Assert.That(result![0].Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(result[0].Offset, Is.EqualTo(9)); // 4 + 5
            Assert.That(result[0].Length, Is.EqualTo(8));

            Assert.That(result[1].Type, Is.EqualTo(MessageEntityType.Italic));
            Assert.That(result[1].Offset, Is.EqualTo(18)); // 13 + 5
            Assert.That(result[1].Length, Is.EqualTo(9)); // обрезано до границы
        }
    }

    /// <summary>
    /// Метод AdaptEntitiesForTruncatedMessage корректно обрабатывает граничные случаи.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null для null входного массива entities.<br />
    /// Проверяет возврат null для пустого входного массива entities.<br />
    /// Проверяет возврат null для пустого обрезанного сообщения.<br />
    /// Проверяет возврат null для null обрезанного сообщения.<br />
    /// Проверяет корректную обработку случаев с невалидными входными данными.
    /// </remarks>
    [TestCase(null, "original", "truncated")]
    public void AdaptEntitiesForTruncatedMessageWithEdgeCasesShouldHandleCorrectly(
        MessageEntity[]? entities,
        string originalMessage,
        string truncatedMessage)
    {
        // Act
        var result = MessageEntityHelper.AdaptEntitiesForTruncatedMessage(entities, originalMessage, truncatedMessage);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод AdaptEntitiesForTruncatedMessage корректно обрабатывает пустой массив entities.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null для пустого входного массива entities.<br />
    /// Проверяет корректную обработку случая отсутствия entities для адаптации.<br />
    /// Проверяет оптимизацию для случая пустого массива entities.
    /// </remarks>
    [Test]
    public void AdaptEntitiesForTruncatedMessageWithEmptyArrayShouldReturnNull()
    {
        // Arrange
        var emptyEntities = Array.Empty<MessageEntity>();

        // Act
        var result = MessageEntityHelper.AdaptEntitiesForTruncatedMessage(emptyEntities, "original", "truncated");

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод AdaptEntitiesForTruncatedMessage возвращает null для пустого обрезанного сообщения.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null при пустом обрезанном сообщении.<br />
    /// Проверяет корректную обработку случая полной обрезки сообщения.<br />
    /// Проверяет оптимизацию для случая отсутствия текста для адаптации.
    /// </remarks>
    [Test]
    public void AdaptEntitiesForTruncatedMessageWithEmptyTruncatedMessageShouldReturnNull()
    {
        // Arrange
        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 0, Length = 5 },
        };

        // Act
        var result = MessageEntityHelper.AdaptEntitiesForTruncatedMessage(entities, "original", "");

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод ValidateEntities корректно валидирует entities относительно текста сообщения.
    /// </summary>
    /// <remarks>
    /// Проверяет валидацию entities с корректными offset и length.<br />
    /// Проверяет фильтрацию entities с отрицательными offset.<br />
    /// Проверяет фильтрацию entities с нулевой или отрицательной length.<br />
    /// Проверяет фильтрацию entities, выходящих за границы текста сообщения.<br />
    /// Проверяет сохранение только валидных entities в результирующем массиве.<br />
    /// Проверяет корректную обработку смешанных случаев с валидными и невалидными entities.
    /// </remarks>
    [Test]
    public void ValidateEntitiesWithMixedValidityShouldFilterCorrectly()
    {
        // Arrange
        const string MessageText = "Тестовое сообщение для валидации";

        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 0, Length = 8 },
            new MessageEntity { Type = MessageEntityType.Italic, Offset = -1, Length = 5 },
            new MessageEntity { Type = MessageEntityType.Code, Offset = 9, Length = 0 },
            new MessageEntity { Type = MessageEntityType.Underline, Offset = 9, Length = 9 },
            new MessageEntity { Type = MessageEntityType.Strikethrough, Offset = 25, Length = 15 },
            new MessageEntity { Type = MessageEntityType.Spoiler, Offset = 50, Length = 5 },
        };

        // Act
        var result = MessageEntityHelper.ValidateEntities(entities, MessageText);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(2));
            Assert.That(result![0].Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(result[0].Offset, Is.Zero);
            Assert.That(result[0].Length, Is.EqualTo(8));
            Assert.That(result[1].Type, Is.EqualTo(MessageEntityType.Underline));
            Assert.That(result[1].Offset, Is.EqualTo(9));
            Assert.That(result[1].Length, Is.EqualTo(9));
        }
    }

    /// <summary>
    /// Метод ValidateEntities корректно обрабатывает граничные случаи.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null для null входного массива entities.<br />
    /// Проверяет возврат null для пустого входного массива entities.<br />
    /// Проверяет возврат null для пустого текста сообщения.<br />
    /// Проверяет возврат null для null текста сообщения.<br />
    /// Проверяет корректную обработку случаев с невалидными входными данными.
    /// </remarks>
    [TestCase(null, "message")]
    public void ValidateEntitiesWithEdgeCasesShouldHandleCorrectly(
        MessageEntity[]? entities,
        string messageText)
    {
        // Act
        var result = MessageEntityHelper.ValidateEntities(entities, messageText);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод ValidateEntities корректно обрабатывает пустой массив entities.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null для пустого входного массива entities.<br />
    /// Проверяет корректную обработку случая отсутствия entities для валидации.<br />
    /// Проверяет оптимизацию для случая пустого массива entities.
    /// </remarks>
    [Test]
    public void ValidateEntitiesWithEmptyArrayShouldReturnNull()
    {
        // Arrange
        var emptyEntities = Array.Empty<MessageEntity>();

        // Act
        var result = MessageEntityHelper.ValidateEntities(emptyEntities, "test message");

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод ValidateEntities возвращает null для пустого или null текста сообщения.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null при пустом тексте сообщения.<br />
    /// Проверяет возврат null при null тексте сообщения.<br />
    /// Проверяет корректную обработку случаев отсутствия текста для валидации.
    /// </remarks>
    [TestCase("")]
    [TestCase(null)]
    public void ValidateEntitiesWithEmptyOrNullMessageTextShouldReturnNull(string? messageText)
    {
        // Arrange
        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 0, Length = 5 },
        };

        // Act
        var result = MessageEntityHelper.ValidateEntities(entities, messageText!);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод ValidateEntities возвращает null когда все entities невалидны.
    /// </summary>
    /// <remarks>
    /// Проверяет возврат null при полной фильтрации всех entities.<br />
    /// Проверяет корректную обработку случая, когда все entities имеют некорректные параметры.<br />
    /// Проверяет оптимизацию памяти при отсутствии валидных результатов.
    /// </remarks>
    [Test]
    public void ValidateEntitiesWithAllInvalidEntitiesShouldReturnNull()
    {
        // Arrange
        const string MessageText = "Короткий текст";

        var entities = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = -1, Length = 5 },
            new MessageEntity { Type = MessageEntityType.Italic, Offset = 5, Length = 0 },
            new MessageEntity { Type = MessageEntityType.Code, Offset = 20, Length = 5 },
        };

        // Act
        var result = MessageEntityHelper.ValidateEntities(entities, MessageText);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Метод CreateCopy корректно создает копию MessageEntity с новыми значениями offset и length.
    /// </summary>
    /// <remarks>
    /// Проверяет создание копии с обновленными offset и length.<br />
    /// Проверяет сохранение всех дополнительных свойств исходной entity.<br />
    /// Проверяет корректное копирование Type, Url, User, Language, CustomEmojiId.<br />
    /// Проверяет создание нового объекта, а не ссылки на исходный.<br />
    /// Проверяет работу с различными типами MessageEntity.<br />
    /// Проверяет корректность установки новых значений offset и length.
    /// </remarks>
    [Test]
    public void CreateCopyWithAllPropertiesShouldCopyCorrectly()
    {
        // Arrange
        var originalUser = new User
        {
            Id = 12345,
            FirstName = "Test",
            Username = "testuser",
        };

        var original = new MessageEntity
        {
            Type = MessageEntityType.TextLink,
            Offset = 10,
            Length = 15,
            Url = "https://example.com",
            User = originalUser,
            Language = "python",
            CustomEmojiId = "custom_emoji_123",
        };

        const int NewOffset = 25;
        const int NewLength = 8;

        // Act
        var copy = MessageEntityHelper.CreateCopy(original, NewOffset, NewLength);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy, Is.Not.SameAs(original));
            Assert.That(copy.Type, Is.EqualTo(MessageEntityType.TextLink));
            Assert.That(copy.Offset, Is.EqualTo(NewOffset));
            Assert.That(copy.Length, Is.EqualTo(NewLength));
            Assert.That(copy.Url, Is.EqualTo("https://example.com"));
            Assert.That(copy.User, Is.SameAs(originalUser));
            Assert.That(copy.Language, Is.EqualTo("python"));
            Assert.That(copy.CustomEmojiId, Is.EqualTo("custom_emoji_123"));
        }
    }

    /// <summary>
    /// Метод CreateCopy корректно обрабатывает entities с минимальными свойствами.
    /// </summary>
    /// <remarks>
    /// Проверяет создание копии для entity только с обязательными свойствами.<br />
    /// Проверяет корректную обработку null значений дополнительных свойств.<br />
    /// Проверяет сохранение null значений в копии.<br />
    /// Проверяет работу с базовыми типами MessageEntity без дополнительных данных.
    /// </remarks>
    [Test]
    public void CreateCopyWithMinimalPropertiesShouldCopyCorrectly()
    {
        // Arrange
        var original = new MessageEntity
        {
            Type = MessageEntityType.Bold,
            Offset = 5,
            Length = 10,
        };

        const int NewOffset = 15;
        const int NewLength = 3;

        // Act
        var copy = MessageEntityHelper.CreateCopy(original, NewOffset, NewLength);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(copy, Is.Not.SameAs(original));
            Assert.That(copy.Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(copy.Offset, Is.EqualTo(NewOffset));
            Assert.That(copy.Length, Is.EqualTo(NewLength));
            Assert.That(copy.Url, Is.Null);
            Assert.That(copy.User, Is.Null);
            Assert.That(copy.Language, Is.Null);
            Assert.That(copy.CustomEmojiId, Is.Null);
        }
    }
}
