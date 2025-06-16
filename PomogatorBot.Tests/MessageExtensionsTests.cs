using PomogatorBot.Web.Common;
using Telegram.Bot.Types;

namespace PomogatorBot.Tests;

[TestFixture]
public class MessageExtensionsTests
{
    [Test]
    public void ValidateUserWithValidUser()
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
            Assert.That(result, Is.Null, "ValidateUser should return null for valid user");
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId), "User ID should be extracted correctly");
        }
    }

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
        Assert.That(result, Is.Not.Null, "ValidateUser should return error response for null From");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Message, Is.EqualTo("Ошибка идентификации пользователя"), "Error message should match expected text");
            Assert.That(userId, Is.Zero, "User ID should be 0 when validation fails");
        }
    }

    [Test]
    public void ValidateUserWithZeroUserId()
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
            Assert.That(result, Is.Null, "ValidateUser should return null for valid user ID 0");
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId), "User ID 0 should be extracted correctly");
        }
    }

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
            Assert.That(result, Is.Null, "ValidateUser should return null for valid large user ID");
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId), "Large user ID should be extracted correctly");
        }
    }

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
            Assert.That(result, Is.Null, "ValidateUser should return null even for bot users");
            Assert.That(actualUserId, Is.EqualTo(ExpectedUserId), "Bot user ID should be extracted correctly");
        }
    }

    [Test]
    public void ValidateUserErrorResponseHasCorrectProperties()
    {
        // Arrange
        var message = new Message { From = null };

        // Act
        var result = message.ValidateUser(out _);

        // Assert
        Assert.That(result, Is.Not.Null, "Result should not be null");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.KeyboardMarkup, Is.Null, "Error response should not have keyboard markup");
            Assert.That(result.Entities, Is.Null, "Error response should not have entities");
            Assert.That(result.Message, Is.Not.Null.And.Not.Empty, "Error message should not be null or empty");
        }
    }
}
