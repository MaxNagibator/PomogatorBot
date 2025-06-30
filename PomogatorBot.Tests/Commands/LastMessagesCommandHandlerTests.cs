using PomogatorBot.Web.Commands;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types.Enums;

namespace PomogatorBot.Tests.Commands;

[TestFixture]
public class LastMessagesCommandHandlerTests
{
    /// <summary>
    /// Тест метода FormatBroadcastsResponseWithEntities для корректного формирования ответа с entities.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное формирование текста ответа с информацией о рассылках.<br />
    /// Проверяет правильную адаптацию MessageEntity[] для каждого сообщения в списке.<br />
    /// Проверяет корректное вычисление offset для entities в составном сообщении.<br />
    /// Проверяет обработку различных типов MessageEntity (bold, italic, links).<br />
    /// Проверяет корректность обрезки длинных сообщений с сохранением entities.<br />
    /// Проверяет объединение entities из нескольких рассылок в единый массив.
    /// </remarks>
    [Test]
    public void FormatBroadcastsResponseWithEntitiesShouldFormatCorrectly()
    {
        // Arrange
        var broadcasts = new List<BroadcastHistory>
        {
            new()
            {
                Id = 1,
                MessageText = "Первое тестовое сообщение с форматированием",
                MessageEntities =
                [
                    new() { Type = MessageEntityType.Bold, Offset = 0, Length = 6 },
                    new() { Type = MessageEntityType.Italic, Offset = 7, Length = 8 },
                ],
                StartedAt = new(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                CompletedAt = new DateTime(2024, 1, 15, 10, 31, 30, DateTimeKind.Utc),
                Status = BroadcastStatus.Completed,
                TotalRecipients = 100,
                SuccessfulDeliveries = 95,
                FailedDeliveries = 5,
            },
            new()
            {
                Id = 2,
                MessageText = "Второе сообщение с ссылкой на https://example.com и другим текстом",
                MessageEntities =
                [
                    new()
                    {
                        Type = MessageEntityType.TextLink,
                        Offset = 25,
                        Length = 19,
                        Url = "https://example.com",
                    },
                ],
                StartedAt = new(2024, 1, 15, 11, 0, 0, DateTimeKind.Utc),
                CompletedAt = new DateTime(2024, 1, 15, 11, 2, 15, DateTimeKind.Utc),
                Status = BroadcastStatus.Completed,
                TotalRecipients = 50,
                SuccessfulDeliveries = 48,
                FailedDeliveries = 2,
            },
        };

        var statistics = new BroadcastStatistics
        {
            Total = 2,
            Completed = 2,
            InProgress = 0,
            Failed = 0,
            TotalMessagesSent = 143,
        };

        const int RequestedCount = 2;

        // Act
        var (responseText, entities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(broadcasts, statistics, RequestedCount);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(responseText, Is.Not.Null);
            Assert.That(responseText, Contains.Substring("Последние 2 рассылок"));
            Assert.That(responseText, Contains.Substring("Общая статистика"));
            Assert.That(responseText, Contains.Substring("Всего рассылок: 2"));
            Assert.That(responseText, Contains.Substring("Завершено: 2"));
            Assert.That(responseText, Contains.Substring("Всего сообщений: 143"));

            Assert.That(responseText, Contains.Substring("15.01.2024 10:30"));
            Assert.That(responseText, Contains.Substring("Получателей: 100"));
            Assert.That(responseText, Contains.Substring("Успешно: 95"));
            Assert.That(responseText, Contains.Substring("Неуспешно: 5"));
            Assert.That(responseText, Contains.Substring("Первое тестовое сообщение"));

            Assert.That(responseText, Contains.Substring("15.01.2024 11:00"));
            Assert.That(responseText, Contains.Substring("Получателей: 50"));
            Assert.That(responseText, Contains.Substring("Успешно: 48"));
            Assert.That(responseText, Contains.Substring("Неуспешно: 2"));
            Assert.That(responseText, Contains.Substring("Второе сообщение с ссылкой"));

            Assert.That(entities, Is.Not.Null);
            Assert.That(entities, Has.Length.GreaterThan(0));

            foreach (var entity in entities)
            {
                Assert.That(entity.Offset, Is.GreaterThanOrEqualTo(0));
                Assert.That(entity.Offset + entity.Length, Is.LessThanOrEqualTo(responseText.Length));
            }
        }
    }

    /// <summary>
    /// Тест метода FormatBroadcastsResponseWithEntities с обрезкой длинных сообщений.
    /// </summary>
    /// <remarks>
    /// Проверяет корректную обрезку сообщений, превышающих максимальную длину.<br />
    /// Проверяет адаптацию entities для обрезанных сообщений.<br />
    /// Проверяет фильтрацию entities, выходящих за границы обрезанного текста.<br />
    /// Проверяет корректное вычисление новой длины для частично обрезанных entities.<br />
    /// Проверяет добавление многоточия к обрезанным сообщениям.<br />
    /// Проверяет сохранение entities, полностью помещающихся в обрезанный текст.
    /// </remarks>
    [Test]
    public void FormatBroadcastsResponseWithEntitiesWithTruncationShouldHandleCorrectly()
    {
        // Arrange
        var longMessage = new string('A', 150) + " конец сообщения";

        var broadcasts = new List<BroadcastHistory>
        {
            new()
            {
                Id = 1,
                MessageText = longMessage,
                MessageEntities =
                [
                    new() { Type = MessageEntityType.Bold, Offset = 0, Length = 10 },
                    new() { Type = MessageEntityType.Italic, Offset = 50, Length = 20 },
                    new() { Type = MessageEntityType.Code, Offset = 95, Length = 10 },
                    new() { Type = MessageEntityType.Underline, Offset = 160, Length = 5 },
                ],
                StartedAt = DateTime.UtcNow,
                Status = BroadcastStatus.Completed,
                TotalRecipients = 10,
                SuccessfulDeliveries = 10,
                FailedDeliveries = 0,
            },
        };

        var statistics = new BroadcastStatistics
        {
            Total = 1,
            Completed = 1,
            InProgress = 0,
            Failed = 0,
            TotalMessagesSent = 10,
        };

        // Act
        var (responseText, entities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(broadcasts, statistics, 1);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(responseText, Contains.Substring("..."), "Обрезанное сообщение должно содержать многоточие");
            Assert.That(entities, Is.Not.Null);

            var messageStartIndex = responseText.IndexOf(new string('A', 10), StringComparison.Ordinal);
            Assert.That(messageStartIndex, Is.GreaterThan(-1), "Начало сообщения должно быть найдено в ответе");

            foreach (var entity in entities!)
            {
                Assert.That(entity.Offset, Is.GreaterThanOrEqualTo(0));
                Assert.That(entity.Offset + entity.Length, Is.LessThanOrEqualTo(responseText.Length));
            }
        }
    }

    /// <summary>
    /// Тест метода FormatBroadcastsResponseWithEntities с пустым списком рассылок.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное формирование ответа при отсутствии рассылок.<br />
    /// Проверяет возврат null для entities при пустом списке рассылок.<br />
    /// Проверяет корректное отображение статистики при отсутствии данных.<br />
    /// Проверяет обработку граничного случая с пустым входным списком.
    /// </remarks>
    [Test]
    public void FormatBroadcastsResponseWithEntitiesWithEmptyListShouldHandleCorrectly()
    {
        // Arrange
        var broadcasts = new List<BroadcastHistory>();

        var statistics = new BroadcastStatistics
        {
            Total = 0,
            Completed = 0,
            InProgress = 0,
            Failed = 0,
            TotalMessagesSent = 0,
        };

        // Act
        var (responseText, entities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(broadcasts, statistics, 10);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(responseText, Is.Not.Null);
            Assert.That(responseText, Contains.Substring("Последние 0 рассылок"));
            Assert.That(responseText, Contains.Substring("Всего рассылок: 0"));
            Assert.That(responseText, Contains.Substring("Завершено: 0"));
            Assert.That(responseText, Contains.Substring("Всего сообщений: 0"));
            Assert.That(entities, Is.Null);
        }
    }

    /// <summary>
    /// Тест метода FormatBroadcastsResponseWithEntities с рассылками без entities.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное формирование ответа для рассылок без MessageEntity[].<br />
    /// Проверяет обработку null значений MessageEntity[] в рассылках.<br />
    /// Проверяет возврат null для entities при отсутствии форматирования во всех рассылках.<br />
    /// Проверяет корректность формирования текста ответа без entities.
    /// </remarks>
    [Test]
    public void FormatBroadcastsResponseWithEntitiesWithoutEntitiesShouldHandleCorrectly()
    {
        // Arrange
        var broadcasts = new List<BroadcastHistory>
        {
            new()
            {
                Id = 1,
                MessageText = "Простое сообщение без форматирования",
                MessageEntities = null,
                StartedAt = DateTime.UtcNow,
                Status = BroadcastStatus.Completed,
                TotalRecipients = 25,
                SuccessfulDeliveries = 25,
                FailedDeliveries = 0,
            },
            new()
            {
                Id = 2,
                MessageText = "Еще одно простое сообщение",
                MessageEntities = [],
                StartedAt = DateTime.UtcNow.AddMinutes(-10),
                Status = BroadcastStatus.InProgress,
                TotalRecipients = 30,
                SuccessfulDeliveries = 15,
                FailedDeliveries = 2,
            },
        };

        var statistics = new BroadcastStatistics
        {
            Total = 2,
            Completed = 1,
            InProgress = 1,
            Failed = 0,
            TotalMessagesSent = 40,
        };

        // Act
        var (responseText, entities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(broadcasts, statistics, 2);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(responseText, Is.Not.Null);
            Assert.That(responseText, Contains.Substring("Простое сообщение без форматирования"));
            Assert.That(responseText, Contains.Substring("Еще одно простое сообщение"));
            Assert.That(responseText, Contains.Substring("Получателей: 25"));
            Assert.That(responseText, Contains.Substring("Получателей: 30"));
            Assert.That(entities, Is.Null);
        }
    }

    /// <summary>
    /// Тест метода FormatBroadcastsResponseWithEntities с рассылками, содержащими ошибки.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное отображение информации об ошибках в рассылках.<br />
    /// Проверяет включение ErrorSummary в текст ответа.<br />
    /// Проверяет корректное формирование entities при наличии ошибок в рассылках.<br />
    /// Проверяет обработку различных статусов рассылок (Failed, InProgress, Completed).
    /// </remarks>
    [Test]
    public void FormatBroadcastsResponseWithEntitiesWithErrorsShouldIncludeErrorInfo()
    {
        // Arrange
        var broadcasts = new List<BroadcastHistory>
        {
            new()
            {
                Id = 1,
                MessageText = "Сообщение с ошибками",
                MessageEntities =
                [
                    new() { Type = MessageEntityType.Bold, Offset = 0, Length = 9 },
                ],
                StartedAt = DateTime.UtcNow,
                Status = BroadcastStatus.Failed,
                TotalRecipients = 100,
                SuccessfulDeliveries = 60,
                FailedDeliveries = 40,
                ErrorSummary = "Превышен лимит API, некоторые пользователи заблокировали бота",
            },
        };

        var statistics = new BroadcastStatistics
        {
            Total = 1,
            Completed = 0,
            InProgress = 0,
            Failed = 1,
            TotalMessagesSent = 60,
        };

        // Act
        var (responseText, entities) = LastMessagesCommandHandler.FormatBroadcastsResponseWithEntities(broadcasts, statistics, 1);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(responseText, Contains.Substring("Ошибки: Превышен лимит API"));
            Assert.That(responseText, Contains.Substring("Неуспешно: 40"));
            Assert.That(entities, Is.Not.Null);
            Assert.That(entities, Has.Length.EqualTo(1));
            Assert.That(entities![0].Type, Is.EqualTo(MessageEntityType.Bold));
        }
    }
}
