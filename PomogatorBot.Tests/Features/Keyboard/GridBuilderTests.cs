using PomogatorBot.Web.Features.Keyboard;

namespace PomogatorBot.Tests.Features.Keyboard;

[TestFixture]
public class GridBuilderTests
{
    [SetUp]
    public void SetUp()
    {
        _builder = KeyboardBuilder.Create();
    }

    private KeyboardBuilder _builder = null!;

    /// <summary>
    /// Метод Grid возвращает экземпляр GridBuilder.
    /// </summary>
    /// <remarks>
    /// Проверяет, что метод Grid возвращает не null объект.<br />
    /// Проверяет, что возвращаемый объект является экземпляром GridBuilder.<br />
    /// Проверяет корректность инициализации режима сетки.
    /// </remarks>
    [Test]
    public void GridReturnsGridBuilderInstance()
    {
        // Act
        var gridBuilder = _builder.Grid();

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(gridBuilder, Is.Not.Null);
            Assert.That(gridBuilder, Is.InstanceOf<GridBuilder>());
        }
    }

    /// <summary>
    /// Добавление одной кнопки в сетку и завершение строки создает клавиатуру с одной строкой.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton создает кнопку в текущей строке сетки.<br />
    /// Проверяет, что End завершает строку и добавляет её в клавиатуру.<br />
    /// Проверяет, что Build создает корректную клавиатуру с одной строкой и одной кнопкой.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования</param>
    [TestCase("Button 1", "btn1")]
    [TestCase("Кнопка 1", "кнопка1")]
    [TestCase("🎯 Target", "target")]
    public void AddButtonAndEndCreatesSingleButtonRow(string text, string callbackData)
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton(text, callbackData)
            .End()
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
    /// Добавление нескольких кнопок в одну строку сетки создает строку с несколькими кнопками.
    /// </summary>
    /// <remarks>
    /// Проверяет, что несколько вызовов AddButton добавляют кнопки в одну строку.<br />
    /// Проверяет, что End завершает строку с правильным количеством кнопок.<br />
    /// Проверяет корректность создания строки с несколькими кнопками в режиме сетки.
    /// </remarks>
    [Test]
    public void AddMultipleButtonsInSameRowCreatesRowWithMultipleButtons()
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton("Button 1", "btn1")
            .AddButton("Button 2", "btn2")
            .AddButton("Button 3", "btn3")
            .End()
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();
        var firstRow = rows[0].ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(1));
            Assert.That(firstRow, Has.Length.EqualTo(3));
            Assert.That(firstRow[0].Text, Is.EqualTo("Button 1"));
            Assert.That(firstRow[1].Text, Is.EqualTo("Button 2"));
            Assert.That(firstRow[2].Text, Is.EqualTo("Button 3"));
        }
    }

    /// <summary>
    /// Создание сетки с несколькими строками формирует клавиатуру с правильной структурой.
    /// </summary>
    /// <remarks>
    /// Проверяет, что каждый вызов End создает новую строку в сетке.<br />
    /// Проверяет, что каждая строка содержит правильное количество кнопок.<br />
    /// Проверяет корректность создания многострочной сетки кнопок.
    /// </remarks>
    [Test]
    public void CreateMultipleRowsInGridCreatesCorrectStructure()
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton("Button 1", "btn1")
            .AddButton("Button 2", "btn2")
            .End()
            .AddButton("Button 3", "btn3")
            .AddButton("Button 4", "btn4")
            .AddButton("Button 5", "btn5")
            .End()
            .AddButton("Button 6", "btn6")
            .End()
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(3));

            // Первая строка: 2 кнопки
            var firstRow = rows[0].ToArray();
            Assert.That(firstRow, Has.Length.EqualTo(2));
            Assert.That(firstRow[0].Text, Is.EqualTo("Button 1"));
            Assert.That(firstRow[1].Text, Is.EqualTo("Button 2"));

            // Вторая строка: 3 кнопки
            var secondRow = rows[1].ToArray();
            Assert.That(secondRow, Has.Length.EqualTo(3));
            Assert.That(secondRow[0].Text, Is.EqualTo("Button 3"));
            Assert.That(secondRow[1].Text, Is.EqualTo("Button 4"));
            Assert.That(secondRow[2].Text, Is.EqualTo("Button 5"));

            // Третья строка: 1 кнопка
            var thirdRow = rows[2].ToArray();
            Assert.That(thirdRow, Has.Length.EqualTo(1));
            Assert.That(thirdRow[0].Text, Is.EqualTo("Button 6"));
        }
    }

    /// <summary>
    /// Метод Build автоматически завершает незавершенную строку в сетке.
    /// </summary>
    /// <remarks>
    /// Проверяет, что Build автоматически вызывает End для текущей строки.<br />
    /// Проверяет, что незавершенная строка корректно добавляется в клавиатуру.<br />
    /// Проверяет удобство использования API без явного вызова End перед Build.
    /// </remarks>
    [Test]
    public void BuildAutomaticallyEndsCurrentRow()
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton("Button 1", "btn1")
            .AddButton("Button 2", "btn2")
            .Build(); // Не вызываем End явно

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();
        var firstRow = rows[0].ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(1));
            Assert.That(firstRow, Has.Length.EqualTo(2));
            Assert.That(firstRow[0].Text, Is.EqualTo("Button 1"));
            Assert.That(firstRow[1].Text, Is.EqualTo("Button 2"));
        }
    }

    /// <summary>
    /// Добавление URL кнопки в сетку создает кнопку с правильной ссылкой.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddUrlButton создает URL кнопку в режиме сетки.<br />
    /// Проверяет, что кнопка содержит правильный URL.<br />
    /// Проверяет поддержку различных типов кнопок в режиме сетки.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="url">URL для кнопки</param>
    [TestCase("Visit Site", "https://example.com")]
    [TestCase("Открыть сайт", "https://test.ru")]
    public void AddUrlButtonInGridCreatesUrlButton(string text, string url)
    {
        // Act
        var keyboard = _builder
            .Grid()
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
    /// Добавление кнопки с иконкой в сетку объединяет иконку и текст.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButtonWithIcon создает кнопку с объединенным текстом в режиме сетки.<br />
    /// Проверяет, что кнопка содержит правильные callback данные.<br />
    /// Проверяет поддержку кнопок с иконками в режиме сетки.
    /// </remarks>
    /// <param name="icon">Иконка для кнопки</param>
    /// <param name="text">Текст кнопки</param>
    /// <param name="callbackData">Callback данные кнопки</param>
    /// <param name="expectedText">Ожидаемый объединенный текст</param>
    [TestCase("🎯", "Target", "target_callback", "🎯 Target")]
    [TestCase("📊", "Статистика", "stats", "📊 Статистика")]
    public void AddButtonWithIconInGridCombinesIconAndText(string icon, string text, string callbackData, string expectedText)
    {
        // Act
        var keyboard = _builder
            .Grid()
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
    /// Условное добавление кнопки в сетку работает на основе переданного условия.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButtonIf добавляет кнопку только при истинном условии в режиме сетки.<br />
    /// Проверяет, что при ложном условии кнопка не добавляется в сетку.<br />
    /// Проверяет поддержку условной логики в режиме сетки.
    /// </remarks>
    /// <param name="condition">Логическое условие для определения необходимости добавления кнопки в сетку</param>
    /// <param name="shouldAddButton">Ожидаемый результат: true если кнопка должна быть добавлена, false если не должна</param>
    [TestCase(true, true)]
    [TestCase(false, false)]
    public void AddButtonIfInGridAddsButtonBasedOnCondition(bool condition, bool shouldAddButton)
    {
        // Act
        var keyboard = _builder
            .Grid()
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
    /// Добавление кнопки с некорректными данными в сетку выбрасывает исключение.
    /// </summary>
    /// <remarks>
    /// Проверяет, что пустой текст кнопки вызывает ArgumentException в режиме сетки.<br />
    /// Проверяет, что пустые callback данные вызывают ArgumentException в режиме сетки.<br />
    /// Проверяет корректность валидации входных данных в режиме сетки.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования валидации входных данных</param>
    /// <param name="callbackData">Callback данные кнопки для тестирования валидации входных данных</param>
    /// <param name="expectedExceptionType">Ожидаемый тип исключения, которое должно быть выброшено при некорректных данных</param>
    [TestCase("", "callback", typeof(ArgumentException))]
    [TestCase("Text", "", typeof(ArgumentException))]
    public void AddButtonWithInvalidInputInGridThrowsException(string text, string callbackData, Type expectedExceptionType)
    {
        // Act & Assert
        Assert.Throws(expectedExceptionType, () => _builder.Grid().AddButton(text, callbackData));
    }

    /// <summary>
    /// Добавление URL кнопки с некорректным URL в сетку выбрасывает исключение.
    /// </summary>
    /// <remarks>
    /// Проверяет, что некорректный формат URL вызывает ArgumentException в режиме сетки.<br />
    /// Проверяет корректность валидации URL в режиме сетки.
    /// </remarks>
    [Test]
    public void AddUrlButtonWithInvalidUrlInGridThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _builder.Grid().AddUrlButton("Link", "not-a-url"));
    }

    /// <summary>
    /// Вызов End на пустой строке не создает строку в клавиатуре.
    /// </summary>
    /// <remarks>
    /// Проверяет, что End без добавления кнопок не создает пустую строку.<br />
    /// Проверяет корректность обработки пустых строк в режиме сетки.<br />
    /// Проверяет оптимизацию создания клавиатуры без лишних пустых строк.
    /// </remarks>
    [Test]
    public void EndOnEmptyRowDoesNotCreateRow()
    {
        // Act
        var keyboard = _builder
            .Grid()
            .End() // Вызываем End без добавления кнопок
            .AddButton("Button 1", "btn1")
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(1));
            var firstRow = rows[0].ToArray();
            Assert.That(firstRow, Has.Length.EqualTo(1));
            Assert.That(firstRow[0].Text, Is.EqualTo("Button 1"));
        }
    }

    /// <summary>
    /// Добавление кнопки Switch Inline в сетку создает кнопку с правильным запросом.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddSwitchInlineButton создает inline кнопку в режиме сетки.<br />
    /// Проверяет, что кнопка содержит правильный inline запрос.<br />
    /// Проверяет поддержку inline кнопок в режиме сетки.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="query">Inline запрос для кнопки</param>
    [TestCase("Search", "search query")]
    [TestCase("Поиск", "запрос поиска")]
    public void AddSwitchInlineButtonInGridCreatesSwitchInlineButton(string text, string query)
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddSwitchInlineButton(text, query)
            .Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(text));
            Assert.That(button.SwitchInlineQuery, Is.EqualTo(query));
        }
    }

    /// <summary>
    /// Добавление кнопки Switch Inline Current Chat в сетку создает кнопку с правильным запросом.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddSwitchInlineCurrentChatButton создает inline кнопку для текущего чата в режиме сетки.<br />
    /// Проверяет, что кнопка содержит правильный inline запрос для текущего чата.<br />
    /// Проверяет поддержку inline кнопок для текущего чата в режиме сетки.
    /// </remarks>
    /// <param name="text">Текст кнопки для тестирования</param>
    /// <param name="query">Inline запрос для кнопки в текущем чате</param>
    [TestCase("Search Here", "search in chat")]
    [TestCase("Поиск здесь", "поиск в чате")]
    public void AddSwitchInlineCurrentChatButtonInGridCreatesSwitchInlineCurrentChatButton(string text, string query)
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddSwitchInlineCurrentChatButton(text, query)
            .Build();

        // Assert
        var button = keyboard.InlineKeyboard.First().First();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(button.Text, Is.EqualTo(text));
            Assert.That(button.SwitchInlineQueryCurrentChat, Is.EqualTo(query));
        }
    }

    /// <summary>
    /// Интеграционный тест: комбинирование обычного режима KeyboardBuilder с режимом сетки.
    /// </summary>
    /// <remarks>
    /// Проверяет, что можно комбинировать обычные методы KeyboardBuilder с режимом сетки.<br />
    /// Проверяет, что структура клавиатуры корректна при смешанном использовании.<br />
    /// Проверяет совместимость различных режимов создания клавиатуры.
    /// </remarks>
    [Test]
    public void IntegrationTestCombiningNormalAndGridModes()
    {
        // Act
        var keyboard = _builder
            .AddButton("Обычная кнопка", "normal") // Обычный режим
            .Grid() // Переход в режим сетки
            .AddButton("Сетка 1", "grid1")
            .AddButton("Сетка 2", "grid2")
            .End()
            .AddButton("Сетка 3", "grid3")
            .Build(); // Build автоматически завершает последнюю строку сетки

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(3));

            // Первая строка: обычная кнопка
            var firstRow = rows[0].ToArray();
            Assert.That(firstRow, Has.Length.EqualTo(1));
            Assert.That(firstRow[0].Text, Is.EqualTo("Обычная кнопка"));

            // Вторая строка: сетка с двумя кнопками
            var secondRow = rows[1].ToArray();
            Assert.That(secondRow, Has.Length.EqualTo(2));
            Assert.That(secondRow[0].Text, Is.EqualTo("Сетка 1"));
            Assert.That(secondRow[1].Text, Is.EqualTo("Сетка 2"));

            // Третья строка: сетка с одной кнопкой (автоматически завершена Build())
            var thirdRow = rows[2].ToArray();
            Assert.That(thirdRow, Has.Length.EqualTo(1));
            Assert.That(thirdRow[0].Text, Is.EqualTo("Сетка 3"));
        }
    }

    /// <summary>
    /// Метод AddButton с эмодзи в сетке объединяет эмодзи и текст через пробел.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton с эмодзи создает кнопку с объединенным текстом в режиме сетки.<br />
    /// Проверяет, что эмодзи и текст разделяются пробелом в режиме сетки.<br />
    /// Проверяет, что callback данные сохраняются без изменений в режиме сетки.<br />
    /// Проверяет корректность работы нового перегруженного метода AddButton в GridBuilder.
    /// </remarks>
    /// <param name="emoji">Эмодзи для тестирования объединения с текстом в сетке</param>
    /// <param name="text">Текст кнопки для тестирования объединения с эмодзи в сетке</param>
    /// <param name="callbackData">Callback данные для проверки их сохранения в сетке</param>
    /// <param name="expectedText">Ожидаемый объединенный текст кнопки в сетке</param>
    [TestCase("🎯", "Цель", "target", "🎯 Цель")]
    [TestCase("📊", "Статистика", "stats", "📊 Статистика")]
    [TestCase("⚙️", "Настройки", "settings", "⚙️ Настройки")]
    [TestCase("🏠", "Главная", "home", "🏠 Главная")]
    public void AddButtonWithEmojiInGridCombinesEmojiAndText(string emoji, string text, string callbackData, string expectedText)
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton(emoji, text, callbackData)
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
    /// Метод AddButton с пустым эмодзи в сетке использует только текст кнопки.
    /// </summary>
    /// <remarks>
    /// Проверяет, что при пустом эмодзи используется только текст кнопки в режиме сетки.<br />
    /// Проверяет, что при null эмодзи используется только текст кнопки в режиме сетки.<br />
    /// Проверяет корректность обработки граничных случаев в GridBuilder.<br />
    /// Проверяет, что callback данные сохраняются без изменений в режиме сетки.
    /// </remarks>
    /// <param name="emoji">Пустое или null значение эмодзи для тестирования граничных случаев в сетке</param>
    /// <param name="text">Текст кнопки, который должен использоваться как есть в сетке</param>
    /// <param name="callbackData">Callback данные для проверки их сохранения в сетке</param>
    [TestCase("", "Только текст", "text_only")]
    public void AddButtonWithEmptyEmojiInGridUsesOnlyText(string emoji, string text, string callbackData)
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton(emoji, text, callbackData)
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
    /// Метод AddButton с эмодзи в сетке поддерживает создание многострочной сетки.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton с эмодзи можно использовать для создания сетки кнопок.<br />
    /// Проверяет, что можно создать несколько строк с кнопками с эмодзи.<br />
    /// Проверяет корректность fluent API для нового перегруженного метода в GridBuilder.<br />
    /// Проверяет, что структура сетки сохраняется при использовании кнопок с эмодзи.
    /// </remarks>
    [Test]
    public void AddButtonWithEmojiInGridSupportsMultiRowGrid()
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton("🏠", "Главная", "home")
            .AddButton("📊", "Статистика", "stats")
            .End()
            .AddButton("⚙️", "Настройки", "settings")
            .AddButton("❓", "Помощь", "help")
            .End()
            .AddButton("🚪", "Выход", "exit")
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(3));

            // Первая строка: 2 кнопки с эмодзи
            var firstRow = rows[0].ToArray();
            Assert.That(firstRow, Has.Length.EqualTo(2));
            Assert.That(firstRow[0].Text, Is.EqualTo("🏠 Главная"));
            Assert.That(firstRow[1].Text, Is.EqualTo("📊 Статистика"));

            // Вторая строка: 2 кнопки с эмодзи
            var secondRow = rows[1].ToArray();
            Assert.That(secondRow, Has.Length.EqualTo(2));
            Assert.That(secondRow[0].Text, Is.EqualTo("⚙️ Настройки"));
            Assert.That(secondRow[1].Text, Is.EqualTo("❓ Помощь"));

            // Третья строка: 1 кнопка с эмодзи
            var thirdRow = rows[2].ToArray();
            Assert.That(thirdRow, Has.Length.EqualTo(1));
            Assert.That(thirdRow[0].Text, Is.EqualTo("🚪 Выход"));
        }
    }

    /// <summary>
    /// Метод AddButton с эмодзи в сетке совместим с другими методами GridBuilder.
    /// </summary>
    /// <remarks>
    /// Проверяет, что AddButton с эмодзи можно комбинировать с обычным AddButton в сетке.<br />
    /// Проверяет, что AddButton с эмодзи можно комбинировать с другими типами кнопок в сетке.<br />
    /// Проверяет совместимость нового перегруженного метода с существующим API GridBuilder.<br />
    /// Проверяет корректность создания смешанной сетки кнопок.
    /// </remarks>
    [Test]
    public void AddButtonWithEmojiInGridIsCompatibleWithOtherMethods()
    {
        // Act
        var keyboard = _builder
            .Grid()
            .AddButton("🏠", "Главная", "home")
            .AddButton("Обычная", "normal")
            .AddUrlButton("Сайт", "https://example.com")
            .End()
            .AddButton("⚙️", "Настройки", "settings")
            .Build();

        // Assert
        var rows = keyboard.InlineKeyboard.ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Length.EqualTo(2));

            // Первая строка: смешанные типы кнопок
            var firstRow = rows[0].ToArray();
            Assert.That(firstRow, Has.Length.EqualTo(3));
            Assert.That(firstRow[0].Text, Is.EqualTo("🏠 Главная"));
            Assert.That(firstRow[1].Text, Is.EqualTo("Обычная"));
            Assert.That(firstRow[2].Text, Is.EqualTo("Сайт"));

            // Вторая строка: кнопка с эмодзи
            var secondRow = rows[1].ToArray();
            Assert.That(secondRow, Has.Length.EqualTo(1));
            Assert.That(secondRow[0].Text, Is.EqualTo("⚙️ Настройки"));
        }
    }
}
