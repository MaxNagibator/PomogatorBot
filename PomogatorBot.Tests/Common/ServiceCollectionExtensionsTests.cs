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

    /// <summary>
    /// Метод AddHandlers регистрирует все обработчики указанного типа из сборки.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddHandlers находит и регистрирует все конкретные реализации интерфейса.<br />
    /// Проверяет, что количество зарегистрированных обработчиков соответствует ожидаемому.<br />
    /// Проверяет, что все ожидаемые типы обработчиков присутствуют в контейнере DI.<br />
    /// Проверяет корректность автоматической регистрации сервисов по интерфейсу.
    /// </remarks>
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

    /// <summary>
    /// Метод AddHandlers не регистрирует абстрактные классы.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddHandlers исключает абстрактные классы из регистрации.<br />
    /// Проверяет фильтрацию типов при автоматической регистрации сервисов.<br />
    /// Проверяет, что в контейнере DI отсутствуют экземпляры абстрактных классов.<br />
    /// Проверяет корректность логики выбора типов для регистрации.
    /// </remarks>
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

    /// <summary>
    /// Метод AddHandlers вызывает дополнительную регистрацию для каждого обработчика.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddHandlers вызывает callback функцию для каждого найденного обработчика.<br />
    /// Проверяет, что количество вызовов дополнительной регистрации соответствует количеству обработчиков.<br />
    /// Проверяет, что все типы обработчиков передаются в callback функцию.<br />
    /// Проверяет корректность работы механизма дополнительной настройки сервисов.
    /// </remarks>
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

    /// <summary>
    /// Метод AddHandlers регистрирует обработчики с временем жизни Scoped.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddHandlers использует ServiceLifetime.Scoped для регистрации сервисов.<br />
    /// Проверяет корректность настройки времени жизни сервисов в контейнере DI.<br />
    /// Проверяет, что дескриптор сервиса создается с правильными параметрами.<br />
    /// Проверяет соответствие стратегии управления жизненным циклом объектов.
    /// </remarks>
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

    /// <summary>
    /// Метод AddHandlers с null дополнительной регистрацией не выбрасывает исключение.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddHandlers корректно обрабатывает отсутствие callback функции.<br />
    /// Проверяет устойчивость метода к null значениям необязательных параметров.<br />
    /// Проверяет, что базовая функциональность работает без дополнительной настройки.<br />
    /// Проверяет корректность обработки граничных случаев при регистрации сервисов.
    /// </remarks>
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
