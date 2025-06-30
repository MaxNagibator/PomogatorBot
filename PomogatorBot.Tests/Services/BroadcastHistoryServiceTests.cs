using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PomogatorBot.Tests.Infrastructure.Entities;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;

namespace PomogatorBot.Tests.Services;

[TestFixture]
public class BroadcastHistoryServiceTests
{
    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new(options);
        _logger = new NullLogger<BroadcastHistoryService>();
        _service = new(_context, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    private TestApplicationDbContext _context = null!;
    private BroadcastHistoryService _service = null!;
    private ILogger<BroadcastHistoryService> _logger = null!;

    /// <summary>
    /// Тест успешного начала рассылки.
    /// </summary>
    /// <remarks>
    /// Проверяет:
    /// • Создание записи рассылки в базе данных
    /// • Корректное заполнение всех полей
    /// • Установка статуса InProgress
    /// • Инициализация счетчиков нулевыми значениями
    /// </remarks>
    [Test]
    public async Task StartBroadcastAsyncValidParametersCreatesRecordCorrectly()
    {
        // Arrange
        const string MessageText = "Тестовое сообщение для рассылки";
        const long AdminUserId = 12345;
        const int TotalRecipients = 100;

        // Act
        var result = await _service.StartAsync(MessageText, AdminUserId, TotalRecipients);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Id, Is.GreaterThan(0), "ID рассылки должен быть присвоен");
            Assert.That(result.MessageText, Is.EqualTo(MessageText), "Текст сообщения должен совпадать");
            Assert.That(result.AdminUserId, Is.EqualTo(AdminUserId), "ID администратора должен совпадать");
            Assert.That(result.TotalRecipients, Is.EqualTo(TotalRecipients), "Количество получателей должно совпадать");
            Assert.That(result.Status, Is.EqualTo(BroadcastStatus.InProgress), "Статус должен быть InProgress");
            Assert.That(result.SuccessfulDeliveries, Is.Zero, "Успешные доставки должны быть 0");
            Assert.That(result.FailedDeliveries, Is.Zero, "Неуспешные доставки должны быть 0");
            Assert.That(result.StartedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)), "Время начала должно быть текущим");
            Assert.That(result.CompletedAt, Is.Null, "Время завершения должно быть null");
        }

        var savedBroadcast = await _context.BroadcastHistory.FindAsync(result.Id);
        Assert.That(savedBroadcast, Is.Not.Null, "Рассылка должна быть сохранена в базе данных");
    }

    /// <summary>
    /// Тест обновления прогресса рассылки.
    /// </summary>
    /// <remarks>
    /// Проверяет:
    /// • Корректное обновление счетчиков успешных и неуспешных доставок
    /// • Сохранение изменений в базе данных
    /// • Обработка несуществующего ID рассылки
    /// </remarks>
    [Test]
    public async Task UpdateBroadcastProgressAsyncExistingBroadcastUpdatesCountersCorrectly()
    {
        // Arrange
        var broadcast = new BroadcastHistory
        {
            MessageText = "Тест",
            TotalRecipients = 50,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.InProgress,
            SuccessfulDeliveries = 0,
            FailedDeliveries = 0,
        };

        _context.BroadcastHistory.Add(broadcast);
        await _context.SaveChangesAsync();

        const int SuccessCount = 30;
        const int FailedCount = 5;

        // Act
        await _service.UpdateProgressAsync(broadcast.Id, SuccessCount, FailedCount);

        // Assert
        var updatedBroadcast = await _context.BroadcastHistory.FindAsync(broadcast.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedBroadcast!.SuccessfulDeliveries, Is.EqualTo(SuccessCount), "Счетчик успешных доставок должен обновиться");
            Assert.That(updatedBroadcast.FailedDeliveries, Is.EqualTo(FailedCount), "Счетчик неуспешных доставок должен обновиться");
        }
    }

    /// <summary>
    /// Тест завершения рассылки.
    /// </summary>
    /// <remarks>
    /// Проверяет:
    /// • Установку финальных счетчиков
    /// • Установку времени завершения
    /// • Корректное определение статуса (Completed/Failed)
    /// • Сохранение краткого описания ошибок
    /// </remarks>
    [TestCase(50, 50, 0, BroadcastStatus.Completed)]
    [TestCase(50, 45, 5, BroadcastStatus.Completed)]
    [TestCase(50, 30, 10, BroadcastStatus.Failed)]
    public async Task CompleteBroadcastAsyncDifferentScenariosSetsCorrectStatus(
        int totalRecipients,
        int successCount,
        int failedCount,
        BroadcastStatus expectedStatus)
    {
        // Arrange
        var broadcast = new BroadcastHistory
        {
            MessageText = "Тест",
            TotalRecipients = totalRecipients,
            StartedAt = DateTime.UtcNow,
            Status = BroadcastStatus.InProgress,
            SuccessfulDeliveries = 0,
            FailedDeliveries = 0,
        };

        _context.BroadcastHistory.Add(broadcast);
        await _context.SaveChangesAsync();

        const string ErrorSummary = "Некоторые ошибки доставки";

        // Act
        await _service.CompleteAsync(broadcast.Id, successCount, failedCount, ErrorSummary);

        // Assert
        var completedBroadcast = await _context.BroadcastHistory.FindAsync(broadcast.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(completedBroadcast!.SuccessfulDeliveries, Is.EqualTo(successCount), "Финальный счетчик успешных доставок");
            Assert.That(completedBroadcast.FailedDeliveries, Is.EqualTo(failedCount), "Финальный счетчик неуспешных доставок");
            Assert.That(completedBroadcast.Status, Is.EqualTo(expectedStatus), "Статус рассылки должен соответствовать результату");
            Assert.That(completedBroadcast.CompletedAt, Is.Not.Null, "Время завершения должно быть установлено");
            Assert.That(completedBroadcast.CompletedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)), "Время завершения должно быть текущим");
            Assert.That(completedBroadcast.ErrorSummary, Is.EqualTo(ErrorSummary), "Описание ошибок должно сохраниться");
        }
    }

    /// <summary>
    /// Тест получения последних рассылок.
    /// </summary>
    /// <remarks>
    /// Проверяет:
    /// • Корректную сортировку по времени начала (новые сверху)
    /// • Ограничение количества возвращаемых записей
    /// • Валидацию параметра count
    /// </remarks>
    [TestCase(5, 5, TestName = "GetLastBroadcasts_RequestFive_ReturnsFive")]
    [TestCase(10, 7, TestName = "GetLastBroadcasts_RequestTenButOnlySeven_ReturnsSeven")]
    [TestCase(0, 1, TestName = "GetLastBroadcasts_RequestZero_ReturnsOne")]
    [TestCase(-5, 1, TestName = "GetLastBroadcasts_RequestNegative_ReturnsOne")]
    [TestCase(150, 7, TestName = "GetLastBroadcasts_RequestOverLimit_ReturnsAll")]
    public async Task GetLastBroadcastsAsyncDifferentCountsReturnsCorrectAmount(int requestedCount, int expectedCount)
    {
        var broadcasts = new List<BroadcastHistory>();

        for (var i = 0; i < 7; i++)
        {
            broadcasts.Add(new()
            {
                MessageText = $"Сообщение {i + 1}",
                TotalRecipients = 10,
                StartedAt = DateTime.UtcNow.AddMinutes(-i),
                Status = BroadcastStatus.Completed,
                SuccessfulDeliveries = 10,
                FailedDeliveries = 0,
            });
        }

        _context.BroadcastHistory.AddRange(broadcasts);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetLastsAsync(requestedCount);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(expectedCount), "Количество возвращенных рассылок должно соответствовать ожидаемому");

            if (result.Count > 1)
            {
                for (var i = 0; i < result.Count - 1; i++)
                {
                    Assert.That(result[i].StartedAt, Is.GreaterThanOrEqualTo(result[i + 1].StartedAt),
                        $"Рассылка {i} должна быть новее рассылки {i + 1}");
                }
            }
        }
    }

    /// <summary>
    /// Тест получения статистики рассылок.
    /// </summary>
    /// <remarks>
    /// Проверяет:
    /// • Корректный подсчет рассылок по статусам
    /// • Суммирование общего количества отправленных сообщений
    /// • Суммирование успешных и неуспешных доставок
    /// • Обработку пустой базы данных
    /// </remarks>
    [Test]
    public async Task GetBroadcastStatisticsAsyncWithVariousStatusesReturnsCorrectCounts()
    {
        // Arrange
        var broadcasts = new[]
        {
            new BroadcastHistory { MessageText = "1", TotalRecipients = 100, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 95, FailedDeliveries = 5, StartedAt = DateTime.UtcNow },
            new BroadcastHistory { MessageText = "2", TotalRecipients = 50, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 50, FailedDeliveries = 0, StartedAt = DateTime.UtcNow },
            new BroadcastHistory { MessageText = "3", TotalRecipients = 75, Status = BroadcastStatus.Failed, SuccessfulDeliveries = 30, FailedDeliveries = 45, StartedAt = DateTime.UtcNow },
            new BroadcastHistory { MessageText = "4", TotalRecipients = 200, Status = BroadcastStatus.InProgress, SuccessfulDeliveries = 150, FailedDeliveries = 10, StartedAt = DateTime.UtcNow },
        };

        _context.BroadcastHistory.AddRange(broadcasts);
        await _context.SaveChangesAsync();

        // Act
        var statistics = await _service.GetStatisticsAsync();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(statistics.Total, Is.EqualTo(4), "Общее количество рассылок");
            Assert.That(statistics.Completed, Is.EqualTo(2), "Количество завершенных рассылок");
            Assert.That(statistics.Failed, Is.EqualTo(1), "Количество неуспешных рассылок");
            Assert.That(statistics.InProgress, Is.EqualTo(1), "Количество рассылок в процессе");
            Assert.That(statistics.TotalMessagesSent, Is.EqualTo(95 + 5 + 50 + 0 + 30 + 45 + 150 + 10), "Общее количество отправленных сообщений");
            Assert.That(statistics.TotalSuccessfulDeliveries, Is.EqualTo(95 + 50 + 30 + 150), "Общее количество успешных доставок");
            Assert.That(statistics.TotalFailedDeliveries, Is.EqualTo(5 + 0 + 45 + 10), "Общее количество неуспешных доставок");
        }
    }

    /// <summary>
    /// Тест очистки истории рассылок.
    /// </summary>
    /// <remarks>
    /// Проверяет:
    /// • Удаление всех записей при вызове без параметров
    /// • Удаление только старых записей при указании количества дней
    /// • Возврат корректного количества удаленных записей
    /// • Сохранение новых записей при частичной очистке
    /// </remarks>
    [Test]
    public async Task ClearHistoryAsyncWithoutParametersRemovesAllRecords()
    {
        // Arrange
        var broadcasts = new[]
        {
            new BroadcastHistory { MessageText = "Старая 1", TotalRecipients = 10, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 10, FailedDeliveries = 0, StartedAt = DateTime.UtcNow.AddDays(-10) },
            new BroadcastHistory { MessageText = "Старая 2", TotalRecipients = 20, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 20, FailedDeliveries = 0, StartedAt = DateTime.UtcNow.AddDays(-5) },
            new BroadcastHistory { MessageText = "Новая", TotalRecipients = 30, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 30, FailedDeliveries = 0, StartedAt = DateTime.UtcNow },
        };

        _context.BroadcastHistory.AddRange(broadcasts);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _service.ClearHistoryAsync();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deletedCount, Is.EqualTo(3), "Должны быть удалены все 3 записи");
        }

        var remainingCount = await _context.BroadcastHistory.CountAsync();
        Assert.That(remainingCount, Is.Zero, "В базе не должно остаться записей");
    }

    /// <summary>
    /// Тест частичной очистки истории рассылок.
    /// </summary>
    [Test]
    public async Task ClearHistoryAsyncWithDaysParameterRemovesOnlyOldRecords()
    {
        // Arrange
        var broadcasts = new[]
        {
            new BroadcastHistory { MessageText = "Очень старая", TotalRecipients = 10, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 10, FailedDeliveries = 0, StartedAt = DateTime.UtcNow.AddDays(-10) },
            new BroadcastHistory { MessageText = "Старая", TotalRecipients = 20, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 20, FailedDeliveries = 0, StartedAt = DateTime.UtcNow.AddDays(-5) },
            new BroadcastHistory { MessageText = "Новая", TotalRecipients = 30, Status = BroadcastStatus.Completed, SuccessfulDeliveries = 30, FailedDeliveries = 0, StartedAt = DateTime.UtcNow.AddDays(-1) },
        };

        _context.BroadcastHistory.AddRange(broadcasts);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _service.ClearHistoryAsync(7);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deletedCount, Is.EqualTo(1), "Должна быть удалена только 1 очень старая запись");
        }

        var remainingBroadcasts = await _context.BroadcastHistory.ToListAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(remainingBroadcasts, Has.Count.EqualTo(2), "Должны остаться 2 записи");
            Assert.That(remainingBroadcasts.Any(b => b.MessageText == "Старая"), Is.True, "Должна остаться 'Старая' запись");
            Assert.That(remainingBroadcasts.Any(b => b.MessageText == "Новая"), Is.True, "Должна остаться 'Новая' запись");
        }
    }
}
