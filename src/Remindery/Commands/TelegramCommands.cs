using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Remindery.Commands;

internal class TelegramCommands : ITelegramCommands
{
    private static class Constants
    {
        public const string StartCmd = "/start";
        public const string HelpCmd = "/help";
    }

    private readonly IMediator _mediator;

    public TelegramCommands(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IEnumerable<BotCommand> AvailableCommands { get; } = new BotCommand[]
    {
        new() { Command = Constants.HelpCmd, Description = "Get usage info" },
    };

    public async Task DispatchUpdate(Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message)
        {
            return;
        }

        var message = update.Message!;
        bool IsTextMessage(string msg) => message.Type == MessageType.Text && message.Text == msg;

        if (IsTextMessage(Constants.StartCmd) || IsTextMessage(Constants.HelpCmd))
        {
            await _mediator.Send(new HelpCmdRequest(message.Chat), ct);
            return;
        }

        await _mediator.Send(new UnknownCmdRequest(update), ct);
    }
}
