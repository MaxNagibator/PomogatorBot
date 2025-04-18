﻿namespace PomogatorBot.Web.Commands.Common;

public class CommandRouter
{
    private readonly Dictionary<string, IBotCommandHandler> _handlers;
    private readonly IBotCommandHandler _defaultHandler;

    public CommandRouter(IEnumerable<IBotCommandHandler> handlers)
    {
        var commandHandlers = handlers as IBotCommandHandler[] ?? handlers.ToArray();

        _handlers = commandHandlers
            .Where(x => string.IsNullOrEmpty(x.Command) == false)
            .ToDictionary(x => '/' + x.Command.ToLowerInvariant());

        _defaultHandler = commandHandlers.FirstOrDefault(x => x is DefaultCommandHandler)
                          ?? throw new InvalidOperationException("Default command handler not found");
    }

    public IBotCommandHandler? GetHandler(string command)
    {
        var key = command.Split(' ')[0].ToLowerInvariant();

        return _handlers.TryGetValue(key, out var handler)
            ? handler
            : null;
    }

    public IBotCommandHandler GetHandlerWithDefault(string command)
    {
        var handler = GetHandler(command);
        handler ??= _defaultHandler;
        return handler;
    }
}
