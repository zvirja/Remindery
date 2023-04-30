using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Remindery.Commands;
using Remindery.Configuration;
using Remindery.Interactivity;
using Remindery.Security;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Remindery.Controllers;

public class TelegramWebhook : Controller
{
    private readonly IChatAuthorization _chatAuthorization;
    private readonly ILogger<TelegramWebhook> _logger;
    private readonly BotConfiguration _botConfig;

    public TelegramWebhook(IOptions<BotConfiguration> botConfiguration, IChatAuthorization chatAuthorization, ILogger<TelegramWebhook> logger)
    {
        _botConfig = botConfiguration.Value;
        _chatAuthorization = chatAuthorization;
        _logger = logger;
    }

    [HttpPost("webhook/{signature}")]
    public async Task<IActionResult> HandleUpdate(string signature, [FromBody] Update update,
        [FromServices] ITelegramCommands updateDispatcher, [FromServices] IInteractionManager interactionManager, [FromServices] ITelegramBotClient botClient)
    {
        if (!string.Equals(_botConfig.WebhookUrlSecret, signature, StringComparison.Ordinal))
        {
            _logger.LogWarning("Rejected webhook request with wrong signature. Actual: {actual}, Expected: {expected}", signature, _botConfig.WebhookUrlSecret);
            return NotFound();
        }

        var chatId = update.TryGetChatId();
        _logger.LogDebug("Received telegram update from chat {chatId}", chatId);

        if (!await _chatAuthorization.IsAuthorized(update))
        {
            _logger.LogWarning("Denied authorization for chat {chat id}", update.TryGetChatId());
            return Ok();
        }

        try
        {
            if (interactionManager.ResumeInteraction(update))
            {
                return Ok();
            }

            await updateDispatcher.DispatchUpdate(update, ct: default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle Telegram Update");
            if (chatId is not null)
            {
                await botClient.SendTextMessageAsync(chatId, $"❌ Failed to handle update: {ex.Message}");
            }
        }

        return Ok();
    }
}
