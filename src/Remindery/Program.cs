using System.Reflection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using Remindery.Commands;
using Remindery.Configuration;
using Remindery.Interactivity;
using Remindery.Security;
using Remindery.Services;
using Serilog;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration);
    lc.WriteTo.Console();
});

builder.Services.AddOptions<BotConfiguration>().BindConfiguration(BotConfiguration.SectionName)
    .Configure(config => { config.WebhookUrlSecret ??= Guid.NewGuid().ToString("N"); });
builder.Services.AddOptions<DeploymentConfiguration>().BindConfiguration(DeploymentConfiguration.SectionName);
builder.Services.AddOptions<DebugConfiguration>().BindConfiguration(DebugConfiguration.SectionName);

builder.Services.AddHostedService<CommandConfiguration>();
builder.Services.AddHostedService<WebhookConfiguration>();
builder.Services.AddHostedService<UpdateNotifier>();

builder.Services.AddHttpClient<ITelegramBotClient, TelegramBotClient>((httpClient, sp) =>
    new TelegramBotClient(sp.GetRequiredService<IOptions<BotConfiguration>>().Value.BotToken, httpClient));

builder.Services.AddSingleton<ITelegramCommands, TelegramCommands>();
builder.Services.AddSingleton<IInteractionManager, InteractionManager>();
builder.Services.AddSingleton<IConfigStore, FileSystemConfigStore>();
builder.Services.AddSingleton<IChatAuthorization, ChatAuthorization>();

builder.Services.AddMediatR(cfx => cfx.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpLogging(c => c.LoggingFields = HttpLoggingFields.All);

var app = builder.Build();

var deployUrlSubPath = app.Services.GetRequiredService<IOptions<DeploymentConfiguration>>().Value.PublicUrl.LocalPath;
if (!string.IsNullOrEmpty(deployUrlSubPath))
{
    app.UsePathBase(deployUrlSubPath);
    app.Logger.LogInformation("Configured requests path base: {url}", deployUrlSubPath);
}

if (app.Services.GetRequiredService<IOptions<DebugConfiguration>>().Value.LogHttpRequests)
{
    app.UseHttpLogging();
    app.Logger.LogInformation("Enabled HTTP requests logging");
}

app.UseRouting();

app.MapControllers();

app.Run();
