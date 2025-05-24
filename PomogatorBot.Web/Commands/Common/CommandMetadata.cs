namespace PomogatorBot.Web.Commands.Common;

public record CommandMetadata(string Command, string Description, bool RequiresAdmin = false);
