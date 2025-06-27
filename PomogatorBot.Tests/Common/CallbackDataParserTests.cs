using PomogatorBot.Web.Common;

namespace PomogatorBot.Tests.Common;

[TestFixture]
public class CallbackDataParserTests
{
    /// <summary>
    /// Метод TryParseWithPrefix корректно парсит callback данные с различными входными параметрами.
    /// </summary>
    /// <remarks>
    /// Проверяет парсинг callback данных с точным совпадением префикса.<br />
    /// Проверяет нечувствительность к регистру при парсинге префикса.<br />
    /// Проверяет обработку callback данных с несовпадающим префиксом.<br />
    /// Проверяет обработку пустых callback данных.<br />
    /// Проверяет парсинг callback данных, состоящих только из префикса.<br />
    /// Проверяет корректность извлечения значения после префикса.
    /// </remarks>
    /// <param name="callbackData">Callback данные для парсинга</param>
    /// <param name="prefix">Префикс для поиска в callback данных</param>
    /// <param name="expectedResult">Ожидаемый результат парсинга: true если префикс найден, false если не найден</param>
    /// <param name="expectedValue">Ожидаемое значение, извлеченное после префикса</param>
    [TestCase("prefix_value", "prefix_", true, "value")]
    [TestCase("PREFIX_value", "prefix_", true, "value")]
    [TestCase("prefix_VALUE", "PREFIX_", true, "VALUE")]
    [TestCase("different_value", "prefix_", false, "")]
    [TestCase("", "prefix_", false, "")]
    [TestCase("prefix_", "prefix_", true, "")]
    public void TryParseWithPrefixWithVariousInputsShouldParseCorrectly(string callbackData, string prefix, bool expectedResult, string expectedValue)
    {
        // Act
        var result = CallbackDataParser.TryParseWithPrefix(callbackData, prefix, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(expectedResult));
            Assert.That(value, Is.EqualTo(expectedValue));
        }
    }

    /// <summary>
    /// Метод TryParseWithPrefix возвращает false при пустом префиксе.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку пустого префикса при парсинге callback данных.<br />
    /// Проверяет, что метод возвращает false при некорректном префиксе.<br />
    /// Проверяет, что выходное значение устанавливается в пустую строку при ошибке парсинга.
    /// </remarks>
    /// <param name="callbackData">Callback данные для тестирования обработки пустого префикса</param>
    /// <param name="prefix">Пустой префикс для тестирования граничного случая</param>
    [TestCase("callback", "")]
    public void TryParseWithPrefixWithEmptyPrefixShouldReturnFalse(string callbackData, string prefix)
    {
        // Act
        var result = CallbackDataParser.TryParseWithPrefix(callbackData, prefix, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(value, Is.EqualTo(string.Empty));
        }
    }

    /// <summary>
    /// Метод CreateWithPrefix корректно объединяет префикс и значение для валидных входных данных.
    /// </summary>
    /// <remarks>
    /// Проверяет объединение простого префикса со значением.<br />
    /// Проверяет объединение префикса с подчеркиванием и значения.<br />
    /// Проверяет объединение сложного префикса с числовым значением.<br />
    /// Проверяет корректность формирования callback данных из компонентов.
    /// </remarks>
    /// <param name="prefix">Префикс для объединения с значением</param>
    /// <param name="value">Значение для объединения с префиксом</param>
    /// <param name="expected">Ожидаемый результат объединения префикса и значения</param>
    [TestCase("prefix", "value", "prefixvalue")]
    [TestCase("toggle_", "All", "toggle_All")]
    [TestCase("broadcast_confirm_", "12345", "broadcast_confirm_12345")]
    public void CreateWithPrefixWithValidInputsShouldCombineCorrectly(string prefix, string value, string expected)
    {
        // Act
        var result = CallbackDataParser.CreateWithPrefix(prefix, value);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    /// <summary>
    /// Метод CreateWithPrefix выбрасывает ArgumentException при пустых входных данных.
    /// </summary>
    /// <remarks>
    /// Проверяет выброс исключения при пустом префиксе с валидным значением.<br />
    /// Проверяет выброс исключения при валидном префиксе с пустым значением.<br />
    /// Проверяет валидацию обязательных параметров метода создания callback данных.
    /// </remarks>
    /// <param name="prefix">Префикс для тестирования валидации (может быть пустым)</param>
    /// <param name="value">Значение для тестирования валидации (может быть пустым)</param>
    [TestCase("", "value")]
    [TestCase("prefix", "")]
    public void CreateWithPrefixWithEmptyInputsShouldThrowException(string prefix, string value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataParser.CreateWithPrefix(prefix, value));
    }

    /// <summary>
    /// Метод CreateWithPrefix выбрасывает ArgumentNullException при null префиксе.
    /// </summary>
    /// <remarks>
    /// Проверяет выброс ArgumentNullException при передаче null в качестве префикса.<br />
    /// Проверяет валидацию обязательного параметра префикса.<br />
    /// Проверяет корректность обработки null значений в методе создания callback данных.
    /// </remarks>
    [Test]
    public void CreateWithPrefixWithNullPrefixShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CallbackDataParser.CreateWithPrefix(null!, "value"));
    }

    /// <summary>
    /// Метод CreateWithPrefix выбрасывает ArgumentNullException при null значении.
    /// </summary>
    /// <remarks>
    /// Проверяет выброс ArgumentNullException при передаче null в качестве значения.<br />
    /// Проверяет валидацию обязательного параметра значения.<br />
    /// Проверяет корректность обработки null значений в методе создания callback данных.
    /// </remarks>
    [Test]
    public void CreateWithPrefixWithNullValueShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CallbackDataParser.CreateWithPrefix("prefix", null!));
    }

    /// <summary>
    /// Метод TryParseWithMultiplePrefixes корректно парсит callback данные с множественными префиксами.
    /// </summary>
    /// <remarks>
    /// Проверяет парсинг callback данных с различными префиксами подтверждения.<br />
    /// Проверяет парсинг callback данных с префиксами отмены.<br />
    /// Проверяет парсинг callback данных с префиксами показа подписчиков.<br />
    /// Проверяет обработку неизвестных callback данных.<br />
    /// Проверяет обработку пустых callback данных.<br />
    /// Проверяет корректность извлечения действия и значения из сложных callback данных.
    /// </remarks>
    /// <param name="testCase">Тестовый случай, содержащий callback данные, префиксы и ожидаемые результаты парсинга</param>
    [TestCaseSource(nameof(MultiplePrefixTestData))]
    public void TryParseWithMultiplePrefixesWithVariousInputsShouldParseCorrectly(MultiplePrefixTestCase testCase)
    {
        // Act
        var result = CallbackDataParser.TryParseWithMultiplePrefixes(testCase.CallbackData, testCase.Prefixes, out var action, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo(testCase.ExpectedResult));
            Assert.That(action, Is.EqualTo(testCase.ExpectedAction));
            Assert.That(value, Is.EqualTo(testCase.ExpectedValue));
        }
    }

    private static IEnumerable<MultiplePrefixTestCase> MultiplePrefixTestData()
    {
        var prefixes = new Dictionary<string, string>
        {
            { "broadcast_confirm_", "confirm" },
            { "broadcast_cancel_", "cancel" },
            { "broadcast_show_subs_", "show_subs" },
        };

        yield return new()
        {
            CallbackData = "broadcast_confirm_12345",
            Prefixes = prefixes,
            ExpectedResult = true,
            ExpectedAction = "confirm",
            ExpectedValue = "12345",
        };

        yield return new()
        {
            CallbackData = "broadcast_cancel_67890",
            Prefixes = prefixes,
            ExpectedResult = true,
            ExpectedAction = "cancel",
            ExpectedValue = "67890",
        };

        yield return new()
        {
            CallbackData = "broadcast_show_subs_abc",
            Prefixes = prefixes,
            ExpectedResult = true,
            ExpectedAction = "show_subs",
            ExpectedValue = "abc",
        };

        yield return new()
        {
            CallbackData = "unknown_action_123",
            Prefixes = prefixes,
            ExpectedResult = false,
            ExpectedAction = "",
            ExpectedValue = "",
        };

        yield return new()
        {
            CallbackData = "",
            Prefixes = prefixes,
            ExpectedResult = false,
            ExpectedAction = "",
            ExpectedValue = "",
        };
    }

    /// <summary>
    /// Метод TryParseWithMultiplePrefixes возвращает false при пустых callback данных.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку пустых callback данных при парсинге с множественными префиксами.<br />
    /// Проверяет, что метод возвращает false при некорректных входных данных.<br />
    /// Проверяет, что выходные параметры устанавливаются в пустые строки при ошибке парсинга.<br />
    /// Проверяет устойчивость метода к граничным случаям входных данных.
    /// </remarks>
    /// <param name="callbackData">Пустые callback данные для тестирования граничного случая</param>
    [TestCase("")]
    [Category("EdgeCases")]
    public void TryParseWithMultiplePrefixesWithEmptyCallbackDataShouldReturnFalse(string callbackData)
    {
        // Arrange
        var prefixes = new Dictionary<string, string> { { "prefix_", "action" } };

        // Act
        var result = CallbackDataParser.TryParseWithMultiplePrefixes(callbackData, prefixes, out var action, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(action, Is.EqualTo(string.Empty));
            Assert.That(value, Is.EqualTo(string.Empty));
        }
    }

    /// <summary>
    /// Метод TryParseWithMultiplePrefixes возвращает false при null callback данных.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку null callback данных при парсинге с множественными префиксами.<br />
    /// Проверяет устойчивость метода к null значениям входных параметров.<br />
    /// Проверяет, что выходные параметры устанавливаются в пустые строки при null входных данных.<br />
    /// Проверяет корректность обработки граничного случая с отсутствующими данными.
    /// </remarks>
    [Test]
    [Category("EdgeCases")]
    public void TryParseWithMultiplePrefixesWithNullCallbackDataShouldReturnFalse()
    {
        // Arrange
        var prefixes = new Dictionary<string, string> { { "prefix_", "action" } };

        // Act
        var result = CallbackDataParser.TryParseWithMultiplePrefixes(null!, prefixes, out var action, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(action, Is.EqualTo(string.Empty));
            Assert.That(value, Is.EqualTo(string.Empty));
        }
    }

    /// <summary>
    /// Метод TryParseWithMultiplePrefixes возвращает false при пустом словаре префиксов.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку пустого словаря префиксов при парсинге callback данных.<br />
    /// Проверяет поведение метода при отсутствии настроенных префиксов.<br />
    /// Проверяет, что выходные параметры устанавливаются в пустые строки при пустой конфигурации.<br />
    /// Проверяет корректность обработки граничного случая с пустой конфигурацией префиксов.
    /// </remarks>
    [Test]
    [Category("EdgeCases")]
    public void TryParseWithMultiplePrefixesWithEmptyPrefixesShouldReturnFalse()
    {
        // Arrange
        var emptyPrefixes = new Dictionary<string, string>();

        // Act
        var result = CallbackDataParser.TryParseWithMultiplePrefixes("callback_data", emptyPrefixes, out var action, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(action, Is.EqualTo(string.Empty));
            Assert.That(value, Is.EqualTo(string.Empty));
        }
    }

    public class MultiplePrefixTestCase
    {
        public required string CallbackData { get; init; }
        public required IReadOnlyDictionary<string, string> Prefixes { get; init; }
        public required bool ExpectedResult { get; init; }
        public required string ExpectedAction { get; init; }
        public required string ExpectedValue { get; init; }
    }
}
