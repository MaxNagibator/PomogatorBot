using PomogatorBot.Web.Common.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;

namespace PomogatorBot.Tests.Common.Keyboard;

[TestFixture]
public class KeyboardBuilderTests
{
    [SetUp]
    public void SetUp()
    {
        _builder = KeyboardBuilder.Create();
    }

    private KeyboardBuilder _builder = null!;

    /// <summary>
    /// Метод Create возвращает новый экземпляр KeyboardBuilder.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод Create возвращает не null объект.<br />
    /// Проверяет, что возвращаемый объект является экземпляром KeyboardBuilder.<br />
    /// Проверяет корректность создания строителя клавиатуры по умолчанию.
    /// </remarks>
    [Test]
    public void CreateReturnsNewInstance()
    {
        // Act
        var builder = KeyboardBuilder.Create();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(builder, Is.Not.Null);
            Assert.That(builder, Is.InstanceOf<KeyboardBuilder>());
        }
    }

    /// <summary>
    /// Метод Create с параметрами использует пользовательские настройки.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод Create принимает пользовательские опции.<br />
    /// Проверяет, что возвращается валидный экземпляр строителя.<br />
    /// Проверяет корректность создания строителя с настройками.
    /// </remarks>
    [Test]
    public void CreateWithOptionsUsesCustomOptions()
    {
        // Arrange
        var options = new KeyboardBuilderOptions { MaxButtonsPerRow = 5 };

        // Act
        var builder = KeyboardBuilder.Create(options);

        // Assert
        Assert.That(builder, Is.Not.Null);
    }

    /// <summary>
    /// Добавление одной кнопки создает строку с единственной кнопкой.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddButton создает клавиатуру с одной строкой.<br />
    /// Проверяет, что строка содержит ровно одну кнопку.<br />
    /// Проверяет, что текст кнопки соответствует переданному параметру.<br />
    /// Проверяет, что callback данные кнопки соответствуют переданному параметру.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования</param>
    [TestCase("Test", "test_callback")]
    [TestCase("Кнопка", "button_callback")]
    [TestCase("🎯 Target", "target")]
    public void AddButtonCreatesSingleButtonRow(string text, string callbackData)
    {
        // Act
        var keyboard = _builder
            .AddButton(text, callbackData)
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();
        var firstRow = rows[0].ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(1));
            Assert.That(firstRow, Has.Length.EqualTo(1));
            Assert.That(firstRow[0].Text, Is.EqualTo(text));
            Assert.That(firstRow[0].CallbackData, Is.EqualTo(callbackData));
        }
    }

    /// <summary>
    /// Добавление строки кнопок создает одну строку с несколькими кнопками.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddButtonRow создает клавиатуру с одной строкой.<br />
    /// Проверяет, что строка содержит правильное количество кнопок.<br />
    /// Проверяет, что текст каждой кнопки соответствует переданным параметрам.
    /// </remarks>
    [Test]
    public void AddButtonRowCreatesMultipleButtonsInOneRow()
    {
        // Act
        var keyboard = _builder
            .AddButtonRow(("Button1", "callback1"), ("Button2", "callback2"))
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();
        var firstRow = rows[0].ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(1));
            Assert.That(firstRow, Has.Length.EqualTo(2));
            Assert.That(firstRow[0].Text, Is.EqualTo("Button1"));
            Assert.That(firstRow[1].Text, Is.EqualTo("Button2"));
        }
    }

    /// <summary>
    /// Метод Build создает объект InlineKeyboardMarkup.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод Build возвращает экземпляр InlineKeyboardMarkup.<br />
    /// Проверяет, что созданная клавиатура содержит кнопки.
    /// </remarks>
    [Test]
    public void BuildCreatesInlineKeyboardMarkup()
    {
        // Act
        var keyboard = _builder
            .AddButton("Test", "test")
            .Build();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(keyboard, Is.InstanceOf<InlineKeyboardMarkup>());
            Assert.That(keyboard.InlineKeyboard.Any(), Is.True);
        }
    }

    /// <summary>
    /// Добавление callback кнопки создает кнопку с правильными данными.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddCallbackButton создает кнопку с правильным текстом.<br />
    /// Проверяет, что кнопка содержит правильные callback данные.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования</param>
    [TestCase("Callback", "callback_data")]
    [TestCase("Действие", "action_data")]
    public void AddCallbackButtonCreatesCallbackButton(string text, string callbackData)
    {
        // Act
        var keyboard = _builder
            .AddCallbackButton(text, callbackData)
            .Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(text));
            Assert.That(button.CallbackData, Is.EqualTo(callbackData));
        }
    }

    /// <summary>
    /// Добавление URL кнопки создает кнопку с правильной ссылкой.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddUrlButton создает кнопку с правильным текстом.<br />
    /// Проверяет, что кнопка содержит правильный URL.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="url">URL для кнопки</param>
    [TestCase("Visit Site", "https://example.com")]
    [TestCase("Открыть сайт", "https://test.ru")]
    public void AddUrlButtonCreatesUrlButton(string text, string url)
    {
        // Act
        var keyboard = _builder
            .AddUrlButton(text, url)
            .Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(text));
            Assert.That(button.Url, Is.EqualTo(url));
        }
    }

    /// <summary>
    /// Добавление кнопки с иконкой объединяет иконку и текст.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddButtonWithIcon создает кнопку с объединенным текстом.<br />
    /// Проверяет, что кнопка содержит правильные callback данные.
    /// </remarks>
    /// <param name="icon">Иконка для кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Callback данные кнопки</param>
    /// <param name="expectedText">Ожидаемый объединенный текст</param>
    [TestCase("🎯", "Target", "target_callback", "🎯 Target")]
    [TestCase("📊", "Статистика", "stats", "📊 Статистика")]
    public void AddButtonWithIconCombinesIconAndText(string icon, string text, string callbackData, string expectedText)
    {
        // Act
        var keyboard = _builder
            .AddButtonWithIcon(icon, text, callbackData)
            .Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(expectedText));
            Assert.That(button.CallbackData, Is.EqualTo(callbackData));
        }
    }

    /// <summary>
    /// Условное добавление кнопки работает на основе переданного условия.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод AddButtonIf добавляет кнопку только при истинном условии.<br />
    /// Проверяет, что при ложном условии кнопка не добавляется.<br />
    /// Проверяет, что добавленная кнопка имеет правильный текст.
    /// </remarks>
    /// <param name="condition">Логическое условие для определения необходимости добавления кнопки в клавиатуру</param>
    /// <param name="shouldAddButton">Ожидаемый результат: true если кнопка должна быть добавлена, false если не должна</param>
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void AddButtonIfAddsButtonBasedOnCondition(bool condition, bool shouldAddButton)
    {
        // Act
        var keyboard = _builder
            .AddButtonIf(condition, "Conditional", "conditional_callback")
            .Build();

        // Assert
        Assert.That(keyboard.InlineKeyboard.Any(), Is.EqualTo(shouldAddButton));

        if (shouldAddButton)
        {
            Assert.That(keyboard.InlineKeyboard.First().First().Text, Is.EqualTo("Conditional"));
        }
    }

    /// <summary>
    /// Добавление кнопки с некорректными данными выбрасывает исключение.
    /// </summary>
    /// <remarks>
    /// Проверяет, что пустой текст кнопки вызывает ArgumentException.<br />
    /// Проверяет, что пустые callback данные вызывают ArgumentException.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования валидации входных данных</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования валидации входных данных</param>
    /// <param name="expectedExceptionType">Ожидаемый тип исключения, которое должно быть выброшено при некорректных данных</param>
    [TestCase("", "callback", typeof(ArgumentException))]
    [TestCase("Text", "", typeof(ArgumentException))]
    public void AddButtonWithInvalidInputThrowsException(string text, string callbackData, Type expectedExceptionType)
    {
        // Act & Assert
        Assert.Throws(expectedExceptionType, () => _builder.AddButton(text, callbackData));
    }

    /// <summary>
    /// Добавление URL кнопки с некорректным URL выбрасывает исключение.
    /// </summary>
    /// <remarks>
    /// Проверяет, что некорректный формат URL вызывает ArgumentException.
    /// </remarks>
    [Test]
    public void AddUrlButtonWithInvalidUrlThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.AddUrlButton("Link", "not-a-url"));
    }

    /// <summary>
    /// Свойство RowCount возвращает правильное количество строк.
    /// </summary>
    /// <remarks>
    /// Проверяет, что RowCount корректно подсчитывает количество строк в клавиатуре.
    /// </remarks>
    [Test]
    public void RowCountReturnsCorrectCount()
    {
        // Act
        _builder
            .AddButton("Button1", "callback1")
            .AddButtonRow(("Button2", "callback2"), ("Button3", "callback3"));

        // Assert
        Assert.That(_builder.RowCount, Is.EqualTo(2));
    }

    /// <summary>
    /// Свойство ButtonCount возвращает правильное количество кнопок.
    /// </summary>
    /// <remarks>
    /// Проверяет, что ButtonCount корректно подсчитывает общее количество кнопок в клавиатуре.
    /// </remarks>
    [Test]
    public void ButtonCountReturnsCorrectCount()
    {
        // Act
        _builder
            .AddButton("Button1", "callback1")
            .AddButtonRow(("Button2", "callback2"), ("Button3", "callback3"));

        // Assert
        Assert.That(_builder.ButtonCount, Is.EqualTo(3));
    }

    /// <summary>
    /// Метод Clear удаляет все кнопки из строителя.
    /// </summary>
    /// <remarks>
    /// Проверяет, что после вызова Clear количество строк равно нулю.<br />
    /// Проверяет, что после вызова Clear количество кнопок равно нулю.
    /// </remarks>
    [Test]
    public void ClearRemovesAllButtons()
    {
        // Arrange
        _builder.AddButton("Button", "callback");

        // Act
        _builder.Clear();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_builder.RowCount, Is.Zero);
            Assert.That(_builder.ButtonCount, Is.Zero);
        }
    }

    /// <summary>
    /// Метод AddButton с эмодзи объединяет эмодзи и текст через пробел.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton с эмодзи создает кнопку с объединенным текстом.<br />
    /// Проверяет, что эмодзи и текст разделяются пробелом.<br />
    /// Проверяет, что callback данные сохраняются без изменений.<br />
    /// Проверяет корректность работы нового перегруженного метода AddButton.
    /// </remarks>
    /// <param name="emoji">Эмодзи для тестирования объединения с текстом</param>
    /// <param name="text">Текст кнопки для тестирования объединения с эмодзи</param>
    /// <param name="callbackData">Callback данные для проверки их сохранения</param>
    /// <param name="expectedText">Ожидаемый объединенный текст кнопки</param>
    [TestCase("🎯", "Цель", "target", "🎯 Цель")]
    [TestCase("📊", "Статистика", "stats", "📊 Статистика")]
    [TestCase("⚙️", "Настройки", "settings", "⚙️ Настройки")]
    [TestCase("🏠", "Главная", "home", "🏠 Главная")]
    public void AddButtonWithEmojiCombinesEmojiAndText(string emoji, string text, string callbackData, string expectedText)
    {
        // Act
        var keyboard = _builder.AddButton(emoji, text, callbackData).Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(expectedText));
            Assert.That(button.CallbackData, Is.EqualTo(callbackData));
        }
    }

    /// <summary>
    /// Метод AddButton с пустым эмодзи использует только текст кнопки.
    /// </summary>
    /// <remarks>
    /// Проверяет, что при пустом эмодзи используется только текст кнопки.<br />
    /// Проверяет, что при null эмодзи используется только текст кнопки.<br />
    /// Проверяет корректность обработки граничных случаев в новом перегруженном методе.<br />
    /// Проверяет, что callback данные сохраняются без изменений.
    /// </remarks>
    /// <param name="emoji">Пустое значение эмодзи для тестирования граничных случаев</param>
    /// <param name="text">Текст кнопки, который должен использоваться как есть</param>
    /// <param name="callbackData">Callback данные для проверки их сохранения</param>
    [TestCase("", "Только текст", "text_only")]
    public void AddButtonWithEmptyEmojiUsesOnlyText(string emoji, string text, string callbackData)
    {
        // Act
        var keyboard = _builder.AddButton(emoji, text, callbackData).Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(text));
            Assert.That(button.CallbackData, Is.EqualTo(callbackData));
        }
    }

    /// <summary>
    /// Метод AddButton с пробелами в эмодзи объединяет пробелы и текст.
    /// </summary>
    /// <remarks>
    /// Проверяет, что пробелы в эмодзи не считаются пустыми и объединяются с текстом.<br />
    /// Проверяет соответствие поведения с KeyboardFactory.CreateCallbackButton.<br />
    /// Проверяет корректность обработки пробельных символов в эмодзи.
    /// </remarks>
    [Test]
    public void AddButtonWithSpacesInEmojiCombinesSpacesAndText()
    {
        // Act
        var keyboard = _builder.AddButton("   ", "Текст", "callback").Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo("    Текст")); // 3 пробела + 1 пробел + текст
            Assert.That(button.CallbackData, Is.EqualTo("callback"));
        }
    }

    /// <summary>
    /// Метод AddButton с эмодзи поддерживает цепочку методов.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton с эмодзи возвращает экземпляр KeyboardBuilder.<br />
    /// Проверяет, что можно создать цепочку из нескольких кнопок с эмодзи.<br />
    /// Проверяет корректность fluent API для нового перегруженного метода.<br />
    /// Проверяет, что каждая кнопка создается как отдельная строка.
    /// </remarks>
    [Test]
    public void AddButtonWithEmojiSupportsMethodChaining()
    {
        // Act
        var keyboard = _builder
            .AddButton("🎯", "Первая", "first")
            .AddButton("📊", "Вторая", "second")
            .AddButton("⚙️", "Третья", "third")
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(3));
            Assert.That(rows[0].First().Text, Is.EqualTo("🎯 Первая"));
            Assert.That(rows[1].First().Text, Is.EqualTo("📊 Вторая"));
            Assert.That(rows[2].First().Text, Is.EqualTo("⚙️ Третья"));
        }
    }

    /// <summary>
    /// Метод AddButton с эмодзи применяет валидацию к объединенному тексту.
    /// </summary>
    /// <remarks>
    /// Проверяет, что валидация применяется к итоговому тексту кнопки.<br />
    /// Проверяет, что слишком длинный объединенный текст вызывает исключение.<br />
    /// Проверяет корректность работы валидации в новом перегруженном методе.<br />
    /// Проверяет, что пустые callback данные вызывают исключение.
    /// </remarks>
    [Test]
    public void AddButtonWithEmojiValidatesCombinedText()
    {
        // Arrange
        var longText = new string('A', 100); // Создаем очень длинный текст
        var emoji = "🎯";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.AddButton(emoji, longText, "callback"));
        Assert.Throws<ArgumentException>(() => _builder.AddButton(emoji, "Text", ""));
    }

    /// <summary>
    /// Метод AddButton с эмодзи совместим с другими методами KeyboardBuilder.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton с эмодзи можно комбинировать с обычным AddButton.<br />
    /// Проверяет, что AddButton с эмодзи можно комбинировать с AddButtonRow.<br />
    /// Проверяет совместимость нового перегруженного метода с существующим API.<br />
    /// Проверяет корректность создания смешанной клавиатуры.
    /// </remarks>
    [Test]
    public void AddButtonWithEmojiIsCompatibleWithOtherMethods()
    {
        // Act
        var keyboard = _builder
            .AddButton("🏠", "Главная", "home")
            .AddButton("Обычная кнопка", "normal")
            .AddButtonRow(("Кнопка 1", "btn1"), ("Кнопка 2", "btn2"))
            .AddButton("⚙️", "Настройки", "settings")
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(4));
            Assert.That(rows[0].First().Text, Is.EqualTo("🏠 Главная"));
            Assert.That(rows[1].First().Text, Is.EqualTo("Обычная кнопка"));
            Assert.That(rows[2].ToArray(), Has.Length.EqualTo(2));
            Assert.That(rows[3].First().Text, Is.EqualTo("⚙️ Настройки"));
        }
    }
}
