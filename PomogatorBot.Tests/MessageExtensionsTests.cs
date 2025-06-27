using PomogatorBot.Web.Common;
using Telegram.Bot.Types;

namespace PomogatorBot.Tests;

[TestFixture]
public class MessageExtensionsTests
{
    /// <summary>
    /// Метод ValidateUser возвращает null для валидного пользователя и корректно извлекает ID пользователя.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает null для валидного пользователя.<br />
    /// Проверяет, что ID пользователя корректно извлекается из сообщения.<br />
    /// Проверяет работу с обычным пользователем с валидными данными.
    /// </remarks>
    [Test]
    public void ValidateUserWithValidUserReturnsNullAndExtractsUserId()
    {
        // Arrange
        const long ExpectedUserId = 12345L;

        var message = new Message
        {
            From = new()
            {
                Id = ExpectedUserId,
                FirstName = "Test",
                IsBot = false,
            },
        };

        // Act
        var result = message.ValidateUser(out var actualUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId));
        }
    }

    /// <summary>
    /// Метод ValidateUser возвращает ошибку и нулевой ID пользователя при отсутствии данных отправителя.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает объект ошибки при null значении From.<br />
    /// Проверяет, что сообщение об ошибке соответствует ожидаемому тексту.<br />
    /// Проверяет, что ID пользователя устанавливается в 0 при ошибке валидации.
    /// </remarks>
    [Test]
    public void ValidateUserWithNullFromReturnsErrorResponseAndZeroUserId()
    {
        // Arrange
        var message = new Message
        {
            From = null,
        };

        // Act
        var result = message.ValidateUser(out var userId);

        // Assert
        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Message, Is.EqualTo("❌ Ошибка идентификации пользователя"));
            Assert.That(userId, Is.Zero);
        }
    }

    /// <summary>
    /// Метод ValidateUser корректно обрабатывает пользователя с нулевым ID.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает null для пользователя с ID равным 0.<br />
    /// Проверяет, что нулевой ID пользователя корректно извлекается.<br />
    /// Проверяет обработку граничного случая с минимальным значением ID.
    /// </remarks>
    [Test]
    public void ValidateUserWithZeroUserIdReturnsNullAndExtractsUserId()
    {
        // Arrange
        const long ExpectedUserId = 0L;

        var message = new Message
        {
            From = new()
            {
                Id = ExpectedUserId,
                FirstName = "Test",
                IsBot = false,
            },
        };

        // Act
        var result = message.ValidateUser(out var actualUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId));
        }
    }

    /// <summary>
    /// Метод ValidateUser корректно обрабатывает пользователя с большим ID.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает null для пользователя с большим ID.<br />
    /// Проверяет, что большой ID пользователя корректно извлекается.<br />
    /// Проверяет обработку граничного случая с максимальными значениями ID.
    /// </remarks>
    [Test]
    public void ValidateUserWithLargeUserIdReturnsNullAndExtractsUserId()
    {
        // Arrange
        const long ExpectedUserId = 999999999999L;

        var message = new Message
        {
            From = new()
            {
                Id = ExpectedUserId,
                FirstName = "Test",
                IsBot = false,
            },
        };

        // Act
        var result = message.ValidateUser(out var actualUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId));
        }
    }

    /// <summary>
    /// Метод ValidateUser корректно обрабатывает пользователя-бота.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает null даже для пользователей-ботов.<br />
    /// Проверяет, что ID пользователя-бота корректно извлекается.<br />
    /// Проверяет обработку специального случая с ботами.
    /// </remarks>
    [Test]
    public void ValidateUserWithBotUserReturnsNullAndExtractsUserId()
    {
        // Arrange
        const long ExpectedUserId = 54321L;

        var message = new Message
        {
            From = new()
            {
                Id = ExpectedUserId,
                FirstName = "TestBot",
                IsBot = true,
            },
        };

        // Act
        var result = message.ValidateUser(out var actualUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId));
        }
    }

    /// <summary>
    /// Ответ об ошибке ValidateUser содержит корректные свойства.
    /// </summary>
    /// <remarks>
    /// Проверяет, что объект ошибки не равен null.<br />
    /// Проверяет, что ответ об ошибке не содержит разметку клавиатуры.<br />
    /// Проверяет, что ответ об ошибке не содержит сущности.<br />
    /// Проверяет, что сообщение об ошибке не пустое и не равно null.
    /// </remarks>
    [Test]
    public void ValidateUserErrorResponseHasCorrectProperties()
    {
        // Arrange
        var message = new Message { From = null };

        // Act
        var result = message.ValidateUser(out _);

        // Assert
        Assert.That(result, Is.Not.Null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.KeyboardMarkup, Is.Null);
            Assert.That(result.Entities, Is.Null);
            Assert.That(result.Message, Is.Not.Null.And.Not.Empty);
        }
    }

    /// <summary>
    /// Метод ValidateUser корректно обрабатывает отрицательный ID пользователя.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает null для отрицательного ID.<br />
    /// Проверяет, что отрицательный ID пользователя корректно извлекается.<br />
    /// Проверяет обработку граничного случая с отрицательными значениями ID.
    /// </remarks>
    [Test]
    public void ValidateUserWithNegativeUserIdReturnsNullAndExtractsUserId()
    {
        // Arrange
        const long ExpectedUserId = -12345L;

        var message = new Message
        {
            From = new()
            {
                Id = ExpectedUserId,
                FirstName = "Test",
                IsBot = false,
            },
        };

        // Act
        var result = message.ValidateUser(out var actualUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId));
        }
    }

    /// <summary>
    /// Метод ValidateUser корректно обрабатывает пользователя без имени.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод ValidateUser возвращает null для пользователя без FirstName.<br />
    /// Проверяет, что ID пользователя корректно извлекается даже при отсутствии имени.<br />
    /// Проверяет обработку случая с минимальными данными пользователя.
    /// </remarks>
    [Test]
    public void ValidateUserWithEmptyFirstNameReturnsNullAndExtractsUserId()
    {
        // Arrange
        const long ExpectedUserId = 98765L;

        var message = new Message
        {
            From = new()
            {
                Id = ExpectedUserId,
                FirstName = "",
                IsBot = false,
            },
        };

        // Act
        var result = message.ValidateUser(out var actualUserId);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Null);
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId));
        }
    }
}
