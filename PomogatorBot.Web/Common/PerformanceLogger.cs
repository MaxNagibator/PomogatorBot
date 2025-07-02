using System.Diagnostics;

namespace PomogatorBot.Web.Common;

public static class PerformanceLogger
{
    public static async Task<T> LogExecutionTimeAsync<T>(
        ILogger logger,
        string operationType,
        string operationName,
        long userId,
        Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Начало обработки {OperationType}: {OperationName} для пользователя {UserId}",
            operationType, operationName, userId);

        try
        {
            var result = await operation();
            stopwatch.Stop();

            logger.LogInformation("Завершение обработки {OperationType}: {OperationName} для пользователя {UserId}. " + "Время выполнения: {ElapsedMs} мс",
                operationType, operationName, userId, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogError(exception, "Ошибка при обработке {OperationType}: {OperationName} для пользователя {UserId}. " + "Время выполнения: {ElapsedMs} мс",
                operationType, operationName, userId, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public static string GetCommandDescription(string commandText, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return "[пустая команда]";
        }

        var command = commandText.Split(' ')[0];

        return command.Length > maxLength
            ? command[..maxLength] + "..."
            : command;
    }

    public static string GetCallbackDescription(string callbackData, int maxLength = 50)
    {
        if (string.IsNullOrWhiteSpace(callbackData))
        {
            return "[пустые данные колбэка]";
        }

        return callbackData.Length > maxLength
            ? callbackData[..maxLength] + "..."
            : callbackData;
    }
}
