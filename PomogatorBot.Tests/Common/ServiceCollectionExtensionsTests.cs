using Microsoft.Extensions.DependencyInjection;
using PomogatorBot.Web.Common;
using System.Reflection;

namespace PomogatorBot.Tests.Common;

// Тесты не очень, но жалко было выкинуть старания китёнка
[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [SetUp]
    public void SetUp()
    {
        _services = new ServiceCollection();
    }

    private IServiceCollection _services;

    [Test]
    public void AddHandlersShouldRegisterAllHandlersOfSpecifiedType()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        _services.AddHandlers<ITestHandler>(assembly);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<ITestHandler>();

        using (Assert.EnterMultipleScope())
        {
            var testHandlers = handlers as ITestHandler[] ?? handlers.ToArray();
            Assert.That(testHandlers, Has.Length.EqualTo(2));
            Assert.That(testHandlers.Any(h => h.GetType() == typeof(TestHandlerA)), Is.True);
            Assert.That(testHandlers.Any(h => h.GetType() == typeof(TestHandlerB)), Is.True);
        }
    }

    [Test]
    public void AddHandlersShouldNotRegisterAbstractClasses()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        _services.AddHandlers<ITestHandler>(assembly);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<ITestHandler>();

        Assert.That(handlers.Any(h => h.GetType() == typeof(AbstractTestHandler)), Is.False);
    }

    [Test]
    public void AddHandlersShouldCallAdditionalRegistrationForEachHandler()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var additionalRegistrationCalls = new List<Type>();

        // Act
        _services.AddHandlers<ITestHandler>(assembly, (type, _) =>
        {
            additionalRegistrationCalls.Add(type);
        });

        // Assert
        Assert.That(additionalRegistrationCalls, Has.Count.EqualTo(2));
        Assert.That(additionalRegistrationCalls, Does.Contain(typeof(TestHandlerA)));
        Assert.That(additionalRegistrationCalls, Does.Contain(typeof(TestHandlerB)));
    }

    [Test]
    public void AddHandlersShouldRegisterHandlersAsScoped()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act
        _services.AddHandlers<ITestHandler>(assembly);

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(ITestHandler));
        Assert.That(serviceDescriptor, Is.Not.Null);
        Assert.That(serviceDescriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
    }

    [Test]
    public void AddHandlersWithNullAdditionalRegistrationShouldNotThrow()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        // Act & Assert
        Assert.DoesNotThrow(() => _services.AddHandlers<ITestHandler>(assembly));
    }
}

public interface ITestHandler
{
    void Handle();
}

public class TestHandlerA : ITestHandler
{
    public void Handle() { }
}

public class TestHandlerB : ITestHandler
{
    public void Handle() { }
}

public abstract class AbstractTestHandler : ITestHandler
{
    public abstract void Handle();
}
