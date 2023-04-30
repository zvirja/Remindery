using Microsoft.Extensions.Options;
using Remindery.Configuration;
using Telegram.Bot;

namespace Remindery.Services;

internal class WebhookConfiguration: IHostedService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly DeploymentConfiguration _deploymentConfig;
    private readonly ILogger<WebhookConfiguration> _logger;
    private readonly BotConfiguration _botConfig;

    public WebhookConfiguration(ITelegramBotClient telegramBotClient,
        IOptions<BotConfiguration> botConfig,
        IOptions<DeploymentConfiguration> deploymentConfig,
        ILogger<WebhookConfiguration> logger)
    {
        _telegramBotClient = telegramBotClient;
        _botConfig = botConfig.Value;
        _deploymentConfig = deploymentConfig.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Uri url = new(_deploymentConfig.PublicUrl, $"webhook/{_botConfig.WebhookUrlSecret}");
        await _telegramBotClient.SetWebhookAsync(url.ToString(), cancellationToken: cancellationToken);

        _logger.LogInformation("Registered webhook url: {url}", url);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _telegramBotClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        _logger.LogInformation("Unregistered webhook url");
    }
}
