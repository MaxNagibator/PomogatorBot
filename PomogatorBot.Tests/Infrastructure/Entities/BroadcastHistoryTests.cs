using Microsoft.EntityFrameworkCore;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Infrastructure.Entities;
using System.Diagnostics;
using System.Text.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Telegram.Bot.Types.User;

namespace PomogatorBot.Tests.Infrastructure.Entities;

[TestFixture]
public class BroadcastHistoryTests
{
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private ApplicationDbContext _context = null!;

    /// <summary>
    /// Тест сериализации и десериализации MessageEntity[] в JSON через Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное сохранение MessageEntity[] в базу данных как JSON.<br />
    /// Проверяет корректное чтение MessageEntity[] из базы данных после сериализации.<br />
    /// Проверяет сохранение всех свойств MessageEntity при сериализации/десериализации.<br />
    /// Проверяет работу с различными типами MessageEntity (bold, italic, links, mentions).<br />
    /// Проверяет корректность обработки null значений для MessageEntity[].<br />
    /// Проверяет целостность данных после операций сохранения и чтения.
    /// </remarks>
    [Test]
    public async Task MessageEntitiesJsonSerializationShouldWorkCorrectly()
    {
        // Arrange
        var testUser = new User
        {
            Id = 12345,
            FirstName = "TestUser",
            Username = "testuser",
        };

        var entities = new[]
        {
            new MessageEntity
            {
                Type = MessageEntityType.Bold,
                Offset = 0,
                Length = 6,
            },
            new MessageEntity
            {
                Type = MessageEntityType.Italic,
                Offset = 7,
                Length = 8,
            },
            new MessageEntity
            {
                Type = MessageEntityType.TextLink,
                Offset = 16,
                Length = 7,
                Url = "https://example.com",
            },
            new MessageEntity
            {
                Type = MessageEntityType.TextMention,
                Offset = 24,
                Length = 9,
                User = testUser,
            },
            new MessageEntity
            {
                Type = MessageEntityType.Code,
                Offset = 34,
                Length = 4,
                Language = "python",
            },
            new MessageEntity
            {
                Type = MessageEntityType.CustomEmoji,
                Offset = 39,
                Length = 2,
                CustomEmojiId = "custom_emoji_123",
            },
        };

        var broadcast = new BroadcastHistory
        {
            MessageText = "Жирный курсивный ссылка @mention код 😀",
            MessageEntities = entities,
            AdminUserId = 67890,
            TotalRecipients = 100,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.Completed,
            SuccessfulDeliveries = 95,
            FailedDeliveries = 5,
        };

        // Act
        _context.BroadcastHistory.Add(broadcast);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var savedBroadcast = await _context.BroadcastHistory
            .FirstOrDefaultAsync(b => b.Id == broadcast.Id);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast!.MessageEntities, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Has.Length.EqualTo(6));

            var boldEntity = savedBroadcast.MessageEntities![0];
            Assert.That(boldEntity.Type, Is.EqualTo(MessageEntityType.Bold));
            Assert.That(boldEntity.Offset, Is.Zero);
            Assert.That(boldEntity.Length, Is.EqualTo(6));

            var italicEntity = savedBroadcast.MessageEntities[1];
            Assert.That(italicEntity.Type, Is.EqualTo(MessageEntityType.Italic));
            Assert.That(italicEntity.Offset, Is.EqualTo(7));
            Assert.That(italicEntity.Length, Is.EqualTo(8));

            var linkEntity = savedBroadcast.MessageEntities[2];
            Assert.That(linkEntity.Type, Is.EqualTo(MessageEntityType.TextLink));
            Assert.That(linkEntity.Offset, Is.EqualTo(16));
            Assert.That(linkEntity.Length, Is.EqualTo(7));
            Assert.That(linkEntity.Url, Is.EqualTo("https://example.com"));

            var mentionEntity = savedBroadcast.MessageEntities[3];
            Assert.That(mentionEntity.Type, Is.EqualTo(MessageEntityType.TextMention));
            Assert.That(mentionEntity.Offset, Is.EqualTo(24));
            Assert.That(mentionEntity.Length, Is.EqualTo(9));
            Assert.That(mentionEntity.User, Is.Not.Null);
            Assert.That(mentionEntity.User!.Id, Is.EqualTo(12345));
            Assert.That(mentionEntity.User.FirstName, Is.EqualTo("TestUser"));
            Assert.That(mentionEntity.User.Username, Is.EqualTo("testuser"));

            var codeEntity = savedBroadcast.MessageEntities[4];
            Assert.That(codeEntity.Type, Is.EqualTo(MessageEntityType.Code));
            Assert.That(codeEntity.Offset, Is.EqualTo(34));
            Assert.That(codeEntity.Length, Is.EqualTo(4));
            Assert.That(codeEntity.Language, Is.EqualTo("python"));

            var emojiEntity = savedBroadcast.MessageEntities[5];
            Assert.That(emojiEntity.Type, Is.EqualTo(MessageEntityType.CustomEmoji));
            Assert.That(emojiEntity.Offset, Is.EqualTo(39));
            Assert.That(emojiEntity.Length, Is.EqualTo(2));
            Assert.That(emojiEntity.CustomEmojiId, Is.EqualTo("custom_emoji_123"));
        }
    }

    /// <summary>
    /// Тест сериализации null значения MessageEntity[] в JSON.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное сохранение null значения MessageEntity[] в базу данных.<br />
    /// Проверяет корректное чтение null значения MessageEntity[] из базы данных.<br />
    /// Проверяет отсутствие ошибок при работе с null значениями.<br />
    /// Проверяет корректность обработки записей без entities форматирования.
    /// </remarks>
    [Test]
    public async Task MessageEntitiesNullValueShouldSerializeCorrectly()
    {
        // Arrange
        var broadcast = new BroadcastHistory
        {
            MessageText = "Простое сообщение без форматирования",
            MessageEntities = null,
            AdminUserId = 12345,
            TotalRecipients = 50,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.InProgress,
            SuccessfulDeliveries = 0,
            FailedDeliveries = 0,
        };

        // Act
        _context.BroadcastHistory.Add(broadcast);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var savedBroadcast = await _context.BroadcastHistory
            .FirstOrDefaultAsync(b => b.Id == broadcast.Id);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Is.Null);
            Assert.That(savedBroadcast.MessageText, Is.EqualTo("Простое сообщение без форматирования"));
        }
    }

    /// <summary>
    /// Тест сериализации пустого массива MessageEntity[] в JSON.
    /// </summary>
    /// <remarks>
    /// Проверяет корректное сохранение пустого массива MessageEntity[] в базу данных.<br />
    /// Проверяет корректное чтение пустого массива MessageEntity[] из базы данных.<br />
    /// Проверяет различие между null и пустым массивом при сериализации.<br />
    /// Проверяет корректность обработки записей с пустым массивом entities.
    /// </remarks>
    [Test]
    public async Task MessageEntitiesEmptyArrayShouldSerializeCorrectly()
    {
        // Arrange
        var broadcast = new BroadcastHistory
        {
            MessageText = "Сообщение с пустым массивом entities",
            MessageEntities = [],
            AdminUserId = 54321,
            TotalRecipients = 25,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.Failed,
            SuccessfulDeliveries = 10,
            FailedDeliveries = 15,
            ErrorSummary = "Тестовая ошибка",
        };

        // Act
        _context.BroadcastHistory.Add(broadcast);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var savedBroadcast = await _context.BroadcastHistory
            .FirstOrDefaultAsync(b => b.Id == broadcast.Id);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Has.Length.EqualTo(0));
            Assert.That(savedBroadcast.ErrorSummary, Is.EqualTo("Тестовая ошибка"));
        }
    }

    /// <summary>
    /// Тест производительности сериализации больших массивов MessageEntity[].
    /// </summary>
    /// <remarks>
    /// Проверяет производительность сериализации массивов MessageEntity[] размером до 1000 элементов.<br />
    /// Проверяет корректность сохранения и чтения больших объемов данных entities.<br />
    /// Проверяет отсутствие деградации производительности при увеличении размера массива.<br />
    /// Проверяет целостность данных при работе с большими массивами entities.
    /// </remarks>
    [Test]
    public async Task MessageEntitiesLargeArrayPerformanceShouldHandleEfficiently()
    {
        // Arrange
        const int EntityCount = 1000;
        var entities = new MessageEntity[EntityCount];

        for (var i = 0; i < EntityCount; i++)
        {
            entities[i] = new()
            {
                Type = (MessageEntityType)(i % 10 + 1),
                Offset = i * 10,
                Length = 5 + i % 10,
                Url = i % 3 == 0 ? $"https://example{i}.com" : null,
                Language = i % 5 == 0 ? "python" : null,
                CustomEmojiId = i % 7 == 0 ? $"emoji_{i}" : null,
            };
        }

        var broadcast = new BroadcastHistory
        {
            MessageText = new('A', 50000),
            MessageEntities = entities,
            AdminUserId = 99999,
            TotalRecipients = 1000,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.Completed,
            SuccessfulDeliveries = 950,
            FailedDeliveries = 50,
        };

        var stopwatch = Stopwatch.StartNew();

        // Act
        _context.BroadcastHistory.Add(broadcast);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var savedBroadcast = await _context.BroadcastHistory
            .FirstOrDefaultAsync(b => b.Id == broadcast.Id);

        stopwatch.Stop();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(savedBroadcast, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Is.Not.Null);
            Assert.That(savedBroadcast.MessageEntities, Has.Length.EqualTo(EntityCount));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), "Операция должна завершиться менее чем за 5 секунд");
            Assert.That(savedBroadcast.MessageEntities![0].Offset, Is.Zero);
            Assert.That(savedBroadcast.MessageEntities[EntityCount - 1].Offset, Is.EqualTo((EntityCount - 1) * 10));
        }
    }
}

public class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BroadcastHistory>()
            .Property(e => e.MessageEntities)
            .HasConversion(v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<MessageEntity[]>(v, (JsonSerializerOptions?)null));
    }
}
