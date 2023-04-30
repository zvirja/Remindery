using Telegram.Bot.Types;

namespace Remindery.Security;

public interface IChatAuthorization
{
    ValueTask<bool> IsAuthorized(Update update);
}
