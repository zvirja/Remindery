using Telegram.Bot.Types;

namespace Remindery.Interactivity;

public interface IInteractionManager
{
    bool ResumeInteraction(Update update);

    Task<Update> AwaitNextUpdate(ChatId chatId);
}
