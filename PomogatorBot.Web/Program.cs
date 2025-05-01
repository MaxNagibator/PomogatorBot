using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using PomogatorBot.Web.CallbackQueries.Common;
using PomogatorBot.Web.Commands.Common;
using PomogatorBot.Web.Infrastructure;
using PomogatorBot.Web.Middlewares;
using PomogatorBot.Web.Services;
using Serilog;
using Serilog.Events;
using Telegram.Bot;

var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "verbose.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logPath,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}]({SourceContext}) {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Запуск веб-приложения");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddSerilog();
    builder.Services.AddHealthChecks();

    builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

    builder.Services.AddScoped(provider => provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

    builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(builder.Configuration.GetValue<string>("BotConfiguration:Token")!));

    builder.Services.AddHostedService<BotBackgroundService>();

    builder.Services.AddBotCommandHandlers(typeof(Program).Assembly)
        .AddScoped<CommandRouter>();

    builder.Services.AddBotCallbackQueryHandlers(typeof(Program).Assembly)
        .AddScoped<CallbackQueryRouter>();

    builder.Services.AddScoped<IKeyboardFactory, KeyboardFactory>()
        .AddScoped<IUserService, UserService>();

    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
        };
    });

    builder.Services.AddExceptionHandler<ExceptionHandler>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddFastEndpoints();
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseFastEndpoints();

    app.MapHealthChecks("/live");

    app.MapGet("/", async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine("wwwroot", "index.html"));
    });

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            Log.Information("Промигрировано");
        }
        catch (Exception ex)
        {
            throw new("migration fail", ex);
        }
    }

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Непредвиденная остановка приложения");
}
finally
{
    Log.CloseAndFlush();
}
