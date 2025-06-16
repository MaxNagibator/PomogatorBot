using PomogatorBot.Web.Common;

namespace PomogatorBot.Tests.Common;

[TestFixture]
public class CallbackDataParserTests
{
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

    [TestCase("", "value")]
    [TestCase("prefix", "")]
    public void CreateWithPrefixWithEmptyInputsShouldThrowException(string prefix, string value)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => CallbackDataParser.CreateWithPrefix(prefix, value));
    }

    [Test]
    public void CreateWithPrefixWithNullPrefixShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CallbackDataParser.CreateWithPrefix(null, "value"));
    }

    [Test]
    public void CreateWithPrefixWithNullValueShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CallbackDataParser.CreateWithPrefix("prefix", null));
    }

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

    [Test]
    [Category("EdgeCases")]
    public void TryParseWithMultiplePrefixesWithNullCallbackDataShouldReturnFalse()
    {
        // Arrange
        var prefixes = new Dictionary<string, string> { { "prefix_", "action" } };

        // Act
        var result = CallbackDataParser.TryParseWithMultiplePrefixes(null, prefixes, out var action, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(action, Is.EqualTo(string.Empty));
            Assert.That(value, Is.EqualTo(string.Empty));
        }
    }

    [Test]
    [Category("EdgeCases")]
    public void TryParseWithMultiplePrefixesWithNullPrefixesShouldReturnFalse()
    {
        // Act
        var result = CallbackDataParser.TryParseWithMultiplePrefixes("callback_data", null, out var action, out var value);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.False);
            Assert.That(action, Is.EqualTo(string.Empty));
            Assert.That(value, Is.EqualTo(string.Empty));
        }
    }

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
