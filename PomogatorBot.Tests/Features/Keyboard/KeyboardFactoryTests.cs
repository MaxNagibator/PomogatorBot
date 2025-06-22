using PomogatorBot.Web.Features.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Tests.Features.Keyboard;

[TestFixture]
public class KeyboardFactoryTests
{
    /// <summary>
    /// Метод CreateCallbackButton с иконкой и текстом создает корректную кнопку.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод CreateCallbackButton объединяет иконку и текст.<br />
    /// Проверяет, что кнопка содержит правильные callback данные.<br />
    /// Проверяет корректность формирования текста кнопки с иконкой.
    /// </remarks>
    [Test]
    public void CreateCallbackButtonWithIconAndTextShouldCreateCorrectButton()
    {
        // Arrange
        const string Icon = "📌";
        const string Text = "Мой профиль";
        const string CallbackData = "me";

        // Act
        var button = KeyboardFactory.CreateCallbackButton(Icon, Text, CallbackData);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo("📌 Мой профиль"));
            Assert.That(button.CallbackData, Is.EqualTo("me"));
        }
    }

    /// <summary>
    /// Метод CreateCallbackButton только с текстом создает корректную кнопку.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод CreateCallbackButton создает кнопку с переданным текстом.<br />
    /// Проверяет, что кнопка содержит правильные callback данные.<br />
    /// Проверяет корректность создания простой текстовой кнопки.
    /// </remarks>
    [Test]
    public void CreateCallbackButtonWithTextOnlyShouldCreateCorrectButton()
    {
        // Arrange
        const string Text = "Помощь";
        const string CallbackData = "help";

        // Act
        var button = KeyboardFactory.CreateCallbackButton(Text, CallbackData);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo("Помощь"));
            Assert.That(button.CallbackData, Is.EqualTo("help"));
        }
    }

    /// <summary>
    /// Метод CreateButtonRow с двумя кнопками создает строку с двумя кнопками.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод CreateButtonRow создает массив с двумя элементами.<br />
    /// Проверяет, что текст каждой кнопки соответствует ожидаемому.<br />
    /// Проверяет корректность создания строки с несколькими кнопками.
    /// </remarks>
    [Test]
    public void CreateButtonRowWithTwoButtonsShouldCreateRowWithTwoButtons()
    {
        // Arrange
        var leftButton = InlineKeyboardButton.WithCallbackData("Left", "left");
        var rightButton = InlineKeyboardButton.WithCallbackData("Right", "right");

        // Act
        var row = KeyboardFactory.CreateButtonRow(leftButton, rightButton);

        // Assert
        Assert.That(row, Has.Length.EqualTo(2));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(row[0].Text, Is.EqualTo("Left"));
            Assert.That(row[1].Text, Is.EqualTo("Right"));
        }
    }

    /// <summary>
    /// Метод CreateButtonRow с одной кнопкой создает строку с одной кнопкой.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод CreateButtonRow создает массив с одним элементом.<br />
    /// Проверяет, что текст кнопки соответствует ожидаемому.<br />
    /// Проверяет корректность создания строки с единственной кнопкой.
    /// </remarks>
    [Test]
    public void CreateButtonRowWithSingleButtonShouldCreateRowWithOneButton()
    {
        // Arrange
        var button = InlineKeyboardButton.WithCallbackData("Single", "single");

        // Act
        var row = KeyboardFactory.CreateButtonRow(button);

        // Assert
        Assert.That(row, Has.Length.EqualTo(1));
        Assert.That(row[0].Text, Is.EqualTo("Single"));
    }

    /// <summary>
    /// Метод CreateConfirmationRow создает корректную строку подтверждения с кнопками подтверждения и отмены.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод CreateConfirmationRow создает строку с двумя кнопками.<br />
    /// Проверяет, что первая кнопка имеет текст и callback данные подтверждения.<br />
    /// Проверяет, что вторая кнопка имеет текст и callback данные отмены.<br />
    /// Проверяет корректность создания стандартной строки подтверждения действия.
    /// </remarks>
    [Test]
    public void CreateConfirmationRowWithConfirmAndCancelShouldCreateCorrectRow()
    {
        // Arrange
        const string ConfirmText = "✅ Подтвердить";
        const string ConfirmCallback = "confirm_123";
        const string CancelText = "❌ Отменить";
        const string CancelCallback = "cancel_123";

        // Act
        var row = KeyboardFactory.CreateConfirmationRow(ConfirmText, ConfirmCallback, CancelText, CancelCallback);

        // Assert
        Assert.That(row, Has.Length.EqualTo(2));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(row[0].Text, Is.EqualTo("✅ Подтвердить"));
            Assert.That(row[0].CallbackData, Is.EqualTo("confirm_123"));
            Assert.That(row[1].Text, Is.EqualTo("❌ Отменить"));
            Assert.That(row[1].CallbackData, Is.EqualTo("cancel_123"));
        }
    }

    /// <summary>
    /// Метод CreateBackButton создает корректную кнопку "Назад" с заданными callback данными.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод CreateBackButton создает кнопку с стандартным текстом "🔙 Назад".<br />
    /// Проверяет, что кнопка содержит переданные callback данные.<br />
    /// Проверяет корректность создания стандартной кнопки навигации назад.
    /// </remarks>
    [Test]
    public void CreateBackButtonWithCallbackDataShouldCreateCorrectBackButton()
    {
        // Arrange
        const string CallbackData = "menu_back";

        // Act
        var button = KeyboardFactory.CreateBackButton(CallbackData);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo("🔙 Назад"));
            Assert.That(button.CallbackData, Is.EqualTo("menu_back"));
        }
    }

    /// <summary>
    /// Метод CreateCallbackButton корректно обрабатывает пустые параметры без выброса исключений.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку пустой иконки с валидными текстом и callback данными.<br />
    /// Проверяет обработку пустого текста с валидными иконкой и callback данными.<br />
    /// Проверяет обработку пустых callback данных с валидными иконкой и текстом.<br />
    /// Проверяет устойчивость метода к граничным случаям входных данных.
    /// </remarks>
    /// <param name="icon">Иконка кнопки для тестирования обработки пустых значений</param>
    /// <param name="text">Текст кнопки для тестирования обработки пустых значений</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования обработки пустых значений</param>
    [Test]
    [TestCase("", "text", "callback")]
    [TestCase("🎯", "", "callback")]
    [TestCase("🎯", "text", "")]
    public void CreateCallbackButtonWithEmptyParametersShouldHandleGracefully(string icon, string text, string callbackData)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateCallbackButton(icon, text, callbackData));
    }

    /// <summary>
    /// Метод CreateCallbackButton (только текст) корректно обрабатывает пустые параметры без выброса исключений.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку пустого текста с валидными callback данными.<br />
    /// Проверяет обработку пустых callback данных с валидным текстом.<br />
    /// Проверяет устойчивость упрощенного метода к граничным случаям входных данных.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования обработки пустых значений</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования обработки пустых значений</param>
    [Test]
    [TestCase("", "callback")]
    [TestCase("text", "")]
    public void CreateCallbackButtonTextOnlyWithEmptyParametersShouldHandleGracefully(string text, string callbackData)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateCallbackButton(text, callbackData));
    }

    /// <summary>
    /// Метод CreateConfirmationRow корректно обрабатывает пустые параметры без выброса исключений.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку всех пустых параметров для создания строки подтверждения.<br />
    /// Проверяет устойчивость метода создания строки подтверждения к граничным случаям.<br />
    /// Проверяет, что метод не выбрасывает исключения при некорректных входных данных.
    /// </remarks>
    [Test]
    public void CreateConfirmationRowWithEmptyParametersShouldHandleGracefully()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateConfirmationRow("", "", "", ""));
    }

    /// <summary>
    /// Метод CreateBackButton корректно обрабатывает пустые callback данные без выброса исключений.
    /// </summary>
    /// <remarks>
    /// Проверяет обработку пустых callback данных для кнопки "Назад".<br />
    /// Проверяет устойчивость метода создания кнопки навигации к граничным случаям.<br />
    /// Проверяет, что метод не выбрасывает исключения при пустых callback данных.
    /// </remarks>
    [Test]
    public void CreateBackButtonWithEmptyCallbackDataShouldHandleGracefully()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateBackButton(""));
    }
}
