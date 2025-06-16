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

    [Test]
    public void ConstructorWithMissingAdminUsernameShouldThrowException()
    {
        // Arrange
        var emptyConfiguration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _ = new TestAdminRequiredCommandHandler(emptyConfiguration));
        Assert.That(exception.Message, Is.EqualTo("Имя пользователя администратора не настроено."));
    }

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
