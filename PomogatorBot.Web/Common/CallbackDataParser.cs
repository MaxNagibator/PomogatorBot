namespace PomogatorBot.Web.Common;

// TODO: Подумать. Возможно пройдёт обкатку
public static class CallbackDataParser
{
    public static bool TryParseWithPrefix(string callbackData, string prefix, out string value)
    {
        value = string.Empty;

        if (string.IsNullOrEmpty(callbackData) || string.IsNullOrEmpty(prefix))
        {
            return false;
        }

        if (callbackData.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        value = callbackData[prefix.Length..];
        return true;
    }

    public static string CreateWithPrefix(string prefix, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        ArgumentException.ThrowIfNullOrEmpty(value);

        return prefix + value;
    }

    public static bool TryParseWithMultiplePrefixes(
        string callbackData,
        IReadOnlyDictionary<string, string> prefixes,
        out string action,
        out string value)
    {
        action = string.Empty;
        value = string.Empty;

        if (string.IsNullOrEmpty(callbackData) || prefixes.Count == 0)
        {
            return false;
        }

        foreach (var (prefix, actionName) in prefixes)
        {
            if (TryParseWithPrefix(callbackData, prefix, out var extractedValue))
            {
                action = actionName;
                value = extractedValue;
                return true;
            }
        }

        return false;
    }
}
