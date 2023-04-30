using Telegram.Bot.Types;

namespace Remindery.Commands;

public interface ITelegramCommands
{
    IEnumerable<BotCommand> AvailableCommands { get; }

    Task DispatchUpdate(Update update, CancellationToken ct);
}
