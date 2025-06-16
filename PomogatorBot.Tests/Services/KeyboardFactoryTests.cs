using PomogatorBot.Web.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Tests.Services;

[TestFixture]
public class KeyboardFactoryTests
{
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

    [Test]
    [TestCase("", "text", "callback")]
    [TestCase("🎯", "", "callback")]
    [TestCase("🎯", "text", "")]
    public void CreateCallbackButtonWithEmptyParametersShouldHandleGracefully(string icon, string text, string callbackData)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateCallbackButton(icon, text, callbackData));
    }

    [Test]
    [TestCase("", "callback")]
    [TestCase("text", "")]
    public void CreateCallbackButtonTextOnlyWithEmptyParametersShouldHandleGracefully(string text, string callbackData)
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateCallbackButton(text, callbackData));
    }

    [Test]
    public void CreateConfirmationRowWithEmptyParametersShouldHandleGracefully()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateConfirmationRow("", "", "", ""));
    }

    [Test]
    public void CreateBackButtonWithEmptyCallbackDataShouldHandleGracefully()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => KeyboardFactory.CreateBackButton(""));
    }
}
