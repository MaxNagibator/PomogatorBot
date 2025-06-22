using Microsoft.Extensions.Configuration;
using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Common;
using Telegram.Bot.Types;

namespace PomogatorBot.Tests.Commands.Common;

[TestFixture]
public class AdminRequiredCommandHandlerTests
{
    [SetUp]
    public void SetUp()
    {
        var configurationBuilder = new ConfigurationBuilder();

        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Admin:Username", "admin_user" },
        }!);

        _configuration = configurationBuilder.Build();

        _handler = new(_configuration);
    }

    private TestAdminRequiredCommandHandler _handler;
    private IConfiguration _configuration;

    /// <summary>
    /// Метод HandleAsync корректно валидирует права администратора для различных имен пользователей.
    /// </summary>
    /// <remarks>
    /// Проверяет доступ для точного совпадения имени администратора.<br />
    /// Проверяет нечувствительность к регистру при проверке имени администратора.<br />
    /// Проверяет запрет доступа для обычных пользователей.<br />
    /// Проверяет запрет доступа для пустого имени пользователя.<br />
    /// Проверяет корректность сообщений об успехе и отказе в доступе.
    /// </remarks>
    /// <param name="username">Имя пользователя для тестирования валидации прав администратора</param>
    /// <param name="shouldAllowAccess">
    /// Ожидаемый результат проверки доступа: true если доступ должен быть разрешен, false если
    /// запрещен
    /// </param>
    [TestCase("admin_user", true)]
    [TestCase("ADMIN_USER", true)]
    [TestCase("Admin_User", true)]
    [TestCase("regular_user", false)]
    [TestCase("", false)]
    public async Task HandleAsyncWithDifferentUsernamesShouldValidateAdminCorrectly(string username, bool shouldAllowAccess)
    {
        // Arrange
        var message = new Message
        {
            From = new()
            {
                Id = 123,
                Username = username,
            },
        };

        // Act
        var result = await _handler.HandleAsync(message, CancellationToken.None);

        // Assert
        Assert.That(result.Message, shouldAllowAccess ? Is.EqualTo("Admin command executed") : Is.EqualTo("Доступ запрещен. Команда доступна только администраторам."));
    }

    /// <summary>
    /// Метод HandleAsync запрещает доступ при отсутствии данных пользователя.
    /// </summary>
    /// <remarks>
    /// Проверяет запрет доступа при null значении From в сообщении.<br />
    /// Проверяет корректность сообщения об отказе в доступе.<br />
    /// Проверяет обработку граничного случая с отсутствующими данными отправителя.
    /// </remarks>
    [Test]
    public async Task HandleAsyncWithNullUserShouldDenyAccess()
    {
        // Arrange
        var message = new Message
        {
            From = null,
        };

        // Act
        var result = await _handler.HandleAsync(message, CancellationToken.None);

        // Assert
        Assert.That(result.Message, Is.EqualTo("Доступ запрещен. Команда доступна только администраторам."));
    }

    /// <summary>
    /// Метод HandleAsync запрещает доступ при null имени пользователя.
    /// </summary>
    /// <remarks>
    /// Проверяет запрет доступа при null значении Username.<br />
    /// Проверяет корректность сообщения об отказе в доступе.<br />
    /// Проверяет обработку граничного случая с отсутствующим именем пользователя.
    /// </remarks>
    [Test]
    public async Task HandleAsyncWithNullUsernameShouldDenyAccess()
    {
        // Arrange
        var message = new Message
        {
            From = new()
            {
                Id = 123,
                Username = null,
            },
        };

        // Act
        var result = await _handler.HandleAsync(message, CancellationToken.None);

        // Assert
        Assert.That(result.Message, Is.EqualTo("Доступ запрещен. Команда доступна только администраторам."));
    }

    /// <summary>
    /// Метод HandleAsync запрещает доступ при пустом имени пользователя.
    /// </summary>
    /// <remarks>
    /// Проверяет запрет доступа при пустой строке Username.<br />
    /// Проверяет корректность сообщения об отказе в доступе.<br />
    /// Проверяет обработку граничного случая с пустым именем пользователя.
    /// </remarks>
    [Test]
    public async Task HandleAsyncWithEmptyUsernameShouldDenyAccess()
    {
        // Arrange
        var message = new Message
        {
            From = new()
            {
                Id = 123,
                Username = "",
            },
        };

        // Act
        var result = await _handler.HandleAsync(message, CancellationToken.None);

        // Assert
        Assert.That(result.Message, Is.EqualTo("Доступ запрещен. Команда доступна только администраторам."));
    }

    /// <summary>
    /// Конструктор выбрасывает исключение при отсутствии настройки имени администратора.
    /// </summary>
    /// <remarks>
    /// Проверяет выброс InvalidOperationException при отсутствии конфигурации Admin:Username.<br />
    /// Проверяет корректность сообщения об ошибке конфигурации.<br />
    /// Проверяет валидацию обязательных настроек при инициализации.
    /// </remarks>
    [Test]
    public void ConstructorWithMissingAdminUsernameShouldThrowException()
    {
        // Arrange
        var emptyConfiguration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _ = new TestAdminRequiredCommandHandler(emptyConfiguration));
        Assert.That(exception.Message, Is.EqualTo("Имя пользователя администратора не настроено."));
    }

    /// <summary>
    /// Метод HandleAsync вызывает HandleAdminCommandAsync для валидного администратора.
    /// </summary>
    /// <remarks>
    /// Проверяет успешное выполнение команды администратора.<br />
    /// Проверяет вызов метода HandleAdminCommandAsync для валидного администратора.<br />
    /// Проверяет корректность ответа при успешном выполнении команды.<br />
    /// Проверяет правильность работы механизма делегирования команд администратора.
    /// </remarks>
    [Test]
    public async Task HandleAsyncWithValidAdminShouldCallHandleAdminCommandAsync()
    {
        // Arrange
        var message = new Message
        {
            From = new()
            {
                Id = 123,
                Username = "admin_user",
            },
        };

        // Act
        var result = await _handler.HandleAsync(message, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Message, Is.EqualTo("Admin command executed"));
            Assert.That(_handler.HandleAdminCommandAsyncCalled, Is.True);
        }
    }

    /// <summary>
    /// Метод HandleAsync не вызывает HandleAdminCommandAsync для невалидного пользователя.
    /// </summary>
    /// <remarks>
    /// Проверяет отказ в выполнении команды для обычного пользователя.<br />
    /// Проверяет отсутствие вызова метода HandleAdminCommandAsync для невалидного пользователя.<br />
    /// Проверяет корректность сообщения об отказе в доступе.<br />
    /// Проверяет правильность работы механизма защиты административных команд.
    /// </remarks>
    [Test]
    public async Task HandleAsyncWithInvalidUserShouldNotCallHandleAdminCommandAsync()
    {
        // Arrange
        var message = new Message
        {
            From = new()
            {
                Id = 123,
                Username = "regular_user",
            },
        };

        // Act
        var result = await _handler.HandleAsync(message, CancellationToken.None);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Message, Is.EqualTo("Доступ запрещен. Команда доступна только администраторам."));
            Assert.That(_handler.HandleAdminCommandAsyncCalled, Is.False);
        }
    }
}

public class TestAdminRequiredCommandHandler(IConfiguration configuration) : AdminRequiredCommandHandler(configuration)
{
    public override string Command => "test";
    public bool HandleAdminCommandAsyncCalled { get; private set; }

    protected override Task<BotResponse> HandleAdminCommandAsync(Message message, CancellationToken cancellationToken)
    {
        HandleAdminCommandAsyncCalled = true;
        return Task.FromResult(new BotResponse("Admin command executed"));
    }
}
