using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PomogatorBot.Tests.Infrastructure.Entities;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Telegram.Bot.Types.User;

namespace PomogatorBot.Tests.Services;

[TestFixture]
public class BroadcastHistoryServiceMessageEntityTests
{
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
        var logger = new NullLogger<BroadcastHistoryService>();
        _service = new(_context, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private ApplicationDbContext _context = null!;
    private BroadcastHistoryService _service = null!;

    /// <summary>
    /// Тест метода StartAsync с параметром MessageEntity[] для корректного сохранения entities в базу данных.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное сохранение MessageEntity[] при создании новой рассылки.<br />
    /// Проверяет сохранение всех свойств MessageEntity в базе данных.<br />
    /// Проверяет корректность инициализации всех полей BroadcastHistory.<br />
    /// Проверяет работу с различными типами MessageEntity (bold, italic, links, mentions).<br />
    /// Проверяет возврат корректного объекта BroadcastHistory с заполненными entities.<br />
    /// Проверяет целостность данных после сохранения в базу данных.
    /// </remarks>
    [Test]
    public async Task StartAsyncWithMessageEntitiesShouldSaveCorrectly()
    {
        // Arrange
        const string MessageText = "Тестовое сообщение с форматированием";
        const long AdminUserId = 12345;
        const int TotalRecipients = 100;

        var testUser = new User
        {
            Id = 67890,
            FirstName = "TestUser",
            Username = "testuser",
        };

        var entities = new[]
        {
            new MessageEntity
            {
                Type = MessageEntityType.Bold,
                Offset = 0,
                Length = 8,
            },
            new MessageEntity
            {
                Type = MessageEntityType.Italic,
                Offset = 9,
                Length = 9,
            },
            new MessageEntity
            {
                Type = MessageEntityType.TextLink,
                Offset = 21,
                Length = 15,
                Url = "https://example.com",
            },
            new MessageEntity
            {
                Type = MessageEntityType.TextMention,
                Offset = 19,
                Length = 1,
                User = testUser,
            },
        };

        // Act
        var result = await _service.StartAsync(MessageText, AdminUserId, TotalRecipients, entities);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.MessageText, Is.EqualTo(MessageText));
            Assert.That(result.AdminUserId, Is.EqualTo(AdminUserId));
            Assert.That(result.TotalRecipients, Is.EqualTo(TotalRecipients));
            Assert.That(result.Status, Is.EqualTo(BroadcastStatus.InProgress));
            Assert.That(result.SuccessfulDeliveries, Is.Zero);
            Assert.That(result.FailedDeliveries, Is.Zero);
            Assert.That(result.MessageEntities, Is.Not.Null);
            Assert.That(result.MessageEntities, Has.Length.EqualTo(4));

            var savedBroadcast = await _context.BroadcastHistory.FindAsync(result.Id);
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast!.MessageEntities, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Has.Length.EqualTo(4));

            var boldEntity = savedBroadcast.MessageEntities![0];
            Assert.That(boldEntity.Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(boldEntity.Offset, Is.Zero);
            Assert.That(boldEntity.Length, Is.EqualTo(8));

            var linkEntity = savedBroadcast.MessageEntities[2];
            Assert.That(linkEntity.Type, Is.EqualTo(MessageEntityType.TextLink));
            Assert.That(linkEntity.Url, Is.EqualTo("https://example.com"));

            var mentionEntity = savedBroadcast.MessageEntities[3];
            Assert.That(mentionEntity.Type, Is.EqualTo(MessageEntityType.TextMention));
            Assert.That(mentionEntity.User, Is.Not.Null);
            Assert.That(mentionEntity.User!.Id, Is.EqualTo(67890));
        }
    }

    /// <summary>
    /// Тест метода StartAsync с null MessageEntity[] для корректной обработки отсутствия entities.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное сохранение null значения MessageEntity[] в базе данных.<br />
    /// Проверяет отсутствие ошибок при работе с null entities.<br />
    /// Проверяет корректность инициализации остальных полей BroadcastHistory.<br />
    /// Проверяет совместимость с существующим кодом, не использующим entities.
    /// </remarks>
    [Test]
    public async Task StartAsyncWithNullMessageEntitiesShouldSaveCorrectly()
    {
        // Arrange
        const string MessageText = "Простое сообщение без форматирования";
        const long AdminUserId = 54321;
        const int TotalRecipients = 50;

        // Act
        var result = await _service.StartAsync(MessageText, AdminUserId, TotalRecipients);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.MessageText, Is.EqualTo(MessageText));
            Assert.That(result.MessageEntities, Is.Null);

            var savedBroadcast = await _context.BroadcastHistory.FindAsync(result.Id);
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast!.MessageEntities, Is.Null);
        }
    }

    /// <summary>
    /// Тест метода StartAsync с пустым массивом MessageEntity[] для корректной обработки.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное сохранение пустого массива MessageEntity[] в базе данных.<br />
    /// Проверяет различие между null и пустым массивом при сохранении.<br />
    /// Проверяет отсутствие ошибок при работе с пустым массивом entities.<br />
    /// Проверяет корректность обработки случаев без entities форматирования.
    /// </remarks>
    [Test]
    public async Task StartAsyncWithEmptyMessageEntitiesShouldSaveCorrectly()
    {
        // Arrange
        const string MessageText = "Сообщение с пустым массивом entities";
        const long AdminUserId = 98765;
        const int TotalRecipients = 75;

        var emptyEntities = Array.Empty<MessageEntity>();

        // Act
        var result = await _service.StartAsync(MessageText, AdminUserId, TotalRecipients, emptyEntities);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.MessageEntities, Is.Not.Null);
            Assert.That(result.MessageEntities, Has.Length.EqualTo(0));

            var savedBroadcast = await _context.BroadcastHistory.FindAsync(result.Id);
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast!.MessageEntities, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Has.Length.EqualTo(0));
        }
    }

    /// <summary>
    /// Тест метода GetLastsAsync для корректного чтения MessageEntity[] из базы данных.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное чтение MessageEntity[] при получении истории рассылок.<br />
    /// Проверяет сохранение всех свойств MessageEntity после чтения из базы данных.<br />
    /// Проверяет корректность десериализации JSON в объекты MessageEntity.<br />
    /// Проверяет работу с множественными записями, содержащими entities.<br />
    /// Проверяет целостность данных при операциях чтения из базы данных.
    /// </remarks>
    [Test]
    public async Task GetLastsAsyncShouldReadMessageEntitiesCorrectly()
    {
        // Arrange
        var entities1 = new[]
        {
            new MessageEntity { Type = MessageEntityType.Bold, Offset = 0, Length = 5 },
            new MessageEntity { Type = MessageEntityType.Italic, Offset = 6, Length = 7 },
        };

        var entities2 = new[]
        {
            new MessageEntity
            {
                Type = MessageEntityType.TextLink,
                Offset = 0,
                Length = 10,
                Url = "https://test.com",
            },
        };

        await _service.StartAsync("Первое сообщение", 111, 10, entities1);
        await _service.StartAsync("Второе сообщение", 222, 20, entities2);
        await _service.StartAsync("Третье сообщение", 333, 30);

        // Act
        var result = await _service.GetLastsAsync(3);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(3));

            var firstBroadcast = result[0];
            Assert.That(firstBroadcast.MessageText, Is.EqualTo("Третье сообщение"));
            Assert.That(firstBroadcast.MessageEntities, Is.Null);

            var secondBroadcast = result[1];
            Assert.That(secondBroadcast.MessageText, Is.EqualTo("Второе сообщение"));
            Assert.That(secondBroadcast.MessageEntities, Is.Not.Null);
            Assert.That(secondBroadcast.MessageEntities, Has.Length.EqualTo(1));
            Assert.That(secondBroadcast.MessageEntities![0].Type, Is.EqualTo(MessageEntityType.TextLink));
            Assert.That(secondBroadcast.MessageEntities[0].Url, Is.EqualTo("https://test.com"));

            var thirdBroadcast = result[2];
            Assert.That(thirdBroadcast.MessageText, Is.EqualTo("Первое сообщение"));
            Assert.That(thirdBroadcast.MessageEntities, Is.Not.Null);
            Assert.That(thirdBroadcast.MessageEntities, Has.Length.EqualTo(2));
            Assert.That(thirdBroadcast.MessageEntities![0].Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(thirdBroadcast.MessageEntities[1].Type, Is.EqualTo(MessageEntityType.Italic));
        }
    }
}
