using PomogatorBot.Web.Constants;
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

    /// <summary>
    /// Метод ReplaceUserVariables корректно заменяет переменные пользователя в шаблоне сообщения.
    /// </summary>
    /// <remarks>
    /// Проверяет замену переменной first_name на имя пользователя.<br />
    /// Проверяет замену переменной username на логин пользователя.<br />
    /// Проверяет замену переменной alias на псевдоним пользователя.<br />
    /// Проверяет комбинированную замену нескольких переменных в одном сообщении.
    /// </remarks>
    [TestCase($"Привет, {TemplateVariables.User.FirstName}!", "Иван", "ivan", "", "Привет, Иван!")]
    [TestCase($"Пользователь {TemplateVariables.User.Username} вошел в систему", "Иван", "ivan", "", "Пользователь ivan вошел в систему")]
    [TestCase($"Добро пожаловать, {TemplateVariables.User.Alias}!", "Иван", "ivan", "Админ", "Добро пожаловать, Админ!")]
    [TestCase($"Привет, {TemplateVariables.User.FirstName}! Ваш логин: {TemplateVariables.User.Username}, псевдоним: {TemplateVariables.User.Alias}", "Иван", "ivan", "Админ", "Привет, Иван! Ваш логин: ivan, псевдоним: Админ")]
    [Category("UserVariableReplacement")]
    public void ReplaceUserVariablesWithValidUserDataShouldReplaceCorrectly(string message, string firstName, string username, string alias, string expected)
    {
        // Arrange
        var user = new PomogatorUser
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

    /// <summary>
    /// Метод ReplaceUserVariables использует имя пользователя как fallback для пустого псевдонима.
    /// </summary>
    /// <remarks>
    /// Проверяет замену переменной alias на имя пользователя при пустом псевдониме.<br />
    /// Проверяет замену переменной alias на имя пользователя при null псевдониме.<br />
    /// Проверяет логику fallback для отсутствующих данных пользователя.
    /// </remarks>
    [TestCase("")]
    [TestCase(null)]
    public void ReplaceUserVariablesWithEmptyAliasShouldFallbackToFirstName(string? alias)
    {
        // Arrange
        var message = $"Добро пожаловать, {TemplateVariables.User.Alias}!";

        var user = new PomogatorUser
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

    /// <summary>
    /// Метод ReplaceUserVariables нечувствителен к регистру переменных.
    /// </summary>
    /// <remarks>
    /// Проверяет замену переменных в верхнем регистре.<br />
    /// Проверяет замену переменных в нижнем регистре.<br />
    /// Проверяет замену переменных в смешанном регистре.<br />
    /// Проверяет корректность обработки различных вариантов написания переменных.
    /// </remarks>
    [TestCase("Привет, <FIRST_NAME>! Логин: <Username>, псевдоним: <Alias>", "Привет, Иван! Логин: ivan, псевдоним: Админ")]
    [TestCase($"Привет, {TemplateVariables.User.FirstName}! Логин: {TemplateVariables.User.Username}, псевдоним: {TemplateVariables.User.Alias}", "Привет, Иван! Логин: ivan, псевдоним: Админ")]
    [TestCase("Привет, <First_Name>! Логин: <UserName>, псевдоним: <ALIAS>", "Привет, Иван! Логин: ivan, псевдоним: Админ")]
    public void ReplaceUserVariablesWithDifferentCasingShouldBeCaseInsensitive(string message, string expected)
    {
        // Arrange
        var user = new PomogatorUser
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

    /// <summary>
    /// Метод ReplacePreviewVariables заменяет переменные на предварительные данные для предпросмотра.
    /// </summary>
    /// <remarks>
    /// Проверяет замену переменных на стандартные данные предпросмотра.<br />
    /// Проверяет нечувствительность к регистру при замене переменных предпросмотра.<br />
    /// Проверяет корректность формирования предварительного просмотра сообщений.
    /// </remarks>
    [TestCase($"Привет, {TemplateVariables.User.FirstName}! Ваш логин: {TemplateVariables.User.Username}, псевдоним: {TemplateVariables.User.Alias}", "Привет, Иван! Ваш логин: @admin, псевдоним: Админ")]
    [TestCase("Привет, <FIRST_NAME>! Логин: <USERNAME>, псевдоним: <ALIAS>", "Привет, Иван! Логин: @admin, псевдоним: Админ")]
    [TestCase("Привет, <First_Name>! Логин: <UserName>, псевдоним: <Alias>", "Привет, Иван! Логин: @admin, псевдоним: Админ")]
    public void ReplacePreviewVariablesWithVariousMessagesShouldReplaceWithPreviewData(string message, string expected)
    {
        // Act
        var result = _service.ReplacePreviewVariables(message);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Метод ReplaceUserVariables возвращает исходное сообщение без изменений при отсутствии известных переменных.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку сообщений без переменных.<br />
    /// Проверяет обработку пустых сообщений.<br />
    /// Проверяет обработку сообщений с неизвестными переменными.<br />
    /// Проверяет стабильность работы метода при различных входных данных.
    /// </remarks>
    [TestCase("Обычное сообщение без переменных")]
    [TestCase("")]
    [TestCase("Сообщение с <неизвестной_переменной>")]
    public void ReplaceUserVariablesWithMessagesWithoutKnownVariablesShouldReturnUnchanged(string message)
    {
        // Arrange
        var user = new PomogatorUser
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

    /// <summary>
    /// Метод ReplacePreviewVariables возвращает исходное сообщение без изменений при отсутствии известных переменных.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку сообщений без переменных в режиме предпросмотра.<br />
    /// Проверяет обработку пустых сообщений в режиме предпросмотра.<br />
    /// Проверяет обработку сообщений с неизвестными переменными в режиме предпросмотра.<br />
    /// Проверяет стабильность работы метода предпросмотра при различных входных данных.
    /// </remarks>
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

    /// <summary>
    /// Метод ReplaceUserVariables корректно обрабатывает сложные сценарии замены переменных.
    /// </summary>
    /// <remarks>
    /// Проверяет многократное использование одной переменной в сообщении.<br />
    /// Проверяет смешанное использование различных переменных.<br />
    /// Проверяет размещение переменных в начале и конце сообщения.<br />
    /// Проверяет комплексные сценарии замены с различными комбинациями данных пользователя.
    /// </remarks>
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
            Message = $"Многократное использование: {TemplateVariables.User.FirstName}, {TemplateVariables.User.FirstName}, {TemplateVariables.User.FirstName}",
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
            Message = $"Смешанные переменные: {TemplateVariables.User.Alias} ({TemplateVariables.User.FirstName}) - @{TemplateVariables.User.Username}",
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
            Message = $"Переменные в начале {TemplateVariables.User.FirstName} и в конце {TemplateVariables.User.Username}",
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
        public required PomogatorUser User { get; init; }
        public required string Expected { get; init; }
    }
}
