namespace PomogatorBot.Web.Services;

public record NotifyResponse(int TotalUsers, int SuccessfulSends, int FailedSends);
