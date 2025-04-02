using FastEndpoints;
using PomogatorBot.Web.Services;

namespace PomogatorBot.Web.Endpoints;

public static class NotifyUsers
{
    public record Request(string Message, Subscribes Subscribes);

    public record Response(int TotalUsers, int SuccessfulSends, int FailedSends, DateTime Timestamp);

    public class Endpoint(IUserService userService) : Endpoint<Request, Response>
    {
        public override void Configure()
        {
            Post("/notify");
            AllowAnonymous();
        }

        public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var response = await userService.NotifyAsync(request.Message, request.Subscribes, cancellationToken);
            await SendOkAsync(new(response.TotalUsers, response.SuccessfulSends, response.FailedSends, DateTime.UtcNow), cancellationToken);
        }
    }
}
