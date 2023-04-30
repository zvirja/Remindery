using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Remindery.Commands;

internal record HelpCmdRequest(Chat Chat) : IRequest;

internal class HelpCmdHandler : IRequestHandler<HelpCmdRequest>
{
    private readonly ITelegramBotClient _botClient;

    public HelpCmdHandler(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task Handle(HelpCmdRequest request, CancellationToken cancellationToken)
    {
        var msg = $"Remindery v{BotVersion.Current.AppVersion} ({BotVersion.Current.GitSha})";

        await _botClient.SendTextMessageAsync(request.Chat.Id, msg, cancellationToken: cancellationToken);
    }
}
