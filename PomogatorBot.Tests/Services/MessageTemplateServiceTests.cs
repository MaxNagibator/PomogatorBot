using PomogatorBot.Web.Infrastructure.Entities;
using PomogatorBot.Web.Services;

namespace PomogatorBot.Tests.Services;

// При написании тестов пострадал один кит
[TestFixture]
public class MessageTemplateServiceTests
{
    [SetUp]
    public void SetUp()
    {
        _service = new();
    }

    private MessageTemplateService _service;

    [TestCase("Привет, <first_name>!", "Иван", "ivan", "", "Привет, Иван!")]
    [TestCase("Пользователь <username> вошел в систему", "Иван", "ivan", "", "Пользователь ivan вошел в систему")]
    [TestCase("Добро пожаловать, <alias>!", "Иван", "ivan", "Админ", "Добро пожаловать, Админ!")]
    [TestCase("Привет, <first_name>! Ваш логин: <username>, псевдоним: <alias>", "Иван", "ivan", "Админ", "Привет, Иван! Ваш логин: ivan, псевдоним: Админ")]
    [Category("UserVariableReplacement")]
    public void ReplaceUserVariablesWithValidUserDataShouldReplaceCorrectly(string message, string firstName, string username, string alias, string expected)
    {
        // Arrange
        var user = new User
        {
            FirstName = firstName,
            Username = username,
            Alias = alias,
        };

        // Act
        var result = _service.ReplaceUserVariables(message, user);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ReplaceUserVariablesWithEmptyAliasShouldFallbackToFirstName(string? alias)
    {
        // Arrange
        var message = "Добро пожаловать, <alias>!";

        var user = new User
        {
            FirstName = "Иван",
            Username = "ivan",
            Alias = alias,
        };

        // Act
        var result = _service.ReplaceUserVariables(message, user);

        // Assert
        Assert.That(result, Is.EqualTo("Добро пожаловать, Иван!"));
    }

    [TestCase("Привет, <FIRST_NAME>! Логин: <Username>, псевдоним: <Alias>", "Привет, Иван! Логин: ivan, псевдоним: Админ")]
    [TestCase("Привет, <first_name>! Логин: <username>, псевдоним: <alias>", "Привет, Иван! Логин: ivan, псевдоним: Админ")]
    [TestCase("Привет, <First_Name>! Логин: <UserName>, псевдоним: <ALIAS>", "Привет, Иван! Логин: ivan, псевдоним: Админ")]
    public void ReplaceUserVariablesWithDifferentCasingShouldBeCaseInsensitive(string message, string expected)
    {
        // Arrange
        var user = new User
        {
            FirstName = "Иван",
            Username = "ivan",
            Alias = "Админ",
        };

        // Act
        var result = _service.ReplaceUserVariables(message, user);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Привет, <first_name>! Ваш логин: <username>, псевдоним: <alias>", "Привет, Иван! Ваш логин: @admin, псевдоним: Админ")]
    [TestCase("Привет, <FIRST_NAME>! Логин: <USERNAME>, псевдоним: <ALIAS>", "Привет, Иван! Логин: @admin, псевдоним: Админ")]
    [TestCase("Привет, <First_Name>! Логин: <UserName>, псевдоним: <Alias>", "Привет, Иван! Логин: @admin, псевдоним: Админ")]
    public void ReplacePreviewVariablesWithVariousMessagesShouldReplaceWithPreviewData(string message, string expected)
    {
        // Act
        var result = _service.ReplacePreviewVariables(message);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("Обычное сообщение без переменных")]
    [TestCase("")]
    [TestCase("Сообщение с <неизвестной_переменной>")]
    public void ReplaceUserVariablesWithMessagesWithoutKnownVariablesShouldReturnUnchanged(string message)
    {
        // Arrange
        var user = new User
        {
            FirstName = "Иван",
            Username = "ivan",
            Alias = "Админ",
        };

        // Act
        var result = _service.ReplaceUserVariables(message, user);

        // Assert
        Assert.That(result, Is.EqualTo(message));
    }

    [TestCase("Обычное сообщение без переменных")]
    [TestCase("")]
    [TestCase("Сообщение с <неизвестной_переменной>")]
    public void ReplacePreviewVariablesWithMessagesWithoutKnownVariablesShouldReturnUnchanged(string message)
    {
        // Act
        var result = _service.ReplacePreviewVariables(message);

        // Assert
        Assert.That(result, Is.EqualTo(message));
    }

    [TestCaseSource(nameof(ComplexUserVariableTestData))]
    public void ReplaceUserVariablesWithComplexScenariosShouldHandleCorrectly(UserVariableTestData testData)
    {
        // Act
        var result = _service.ReplaceUserVariables(testData.Message, testData.User);

        // Assert
        Assert.That(result, Is.EqualTo(testData.Expected));
    }

    private static IEnumerable<UserVariableTestData> ComplexUserVariableTestData()
    {
        yield return new()
        {
            Message = "Многократное использование: <first_name>, <first_name>, <first_name>",
            User = new()
            {
                FirstName = "Тест",
                Username = "test",
                Alias = "",
            },
            Expected = "Многократное использование: Тест, Тест, Тест",
        };

        yield return new()
        {
            Message = "Смешанные переменные: <alias> (<first_name>) - @<username>",
            User = new()
            {
                FirstName = "Иван",
                Username = "ivan_admin",
                Alias = "Главный Админ",
            },
            Expected = "Смешанные переменные: Главный Админ (Иван) - @ivan_admin",
        };

        yield return new()
        {
            Message = "Переменные в начале <first_name> и в конце <username>",
            User = new()
            {
                FirstName = "Анна",
                Username = "anna",
                Alias = "",
            },
            Expected = "Переменные в начале Анна и в конце anna",
        };
    }

    public class UserVariableTestData
    {
        public required string Message { get; init; }
        public required User User { get; init; }
        public required string Expected { get; init; }
    }
}
