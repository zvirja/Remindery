using Telegram.Bot.Types;

namespace Remindery.Configuration;

internal interface IConfigStore
{
    public Task<ChatId[]> GetAllowedChatIds();

    public Task<ChatId?> GetAdminChatId();

    public Task<Version?> GetLastAppVersion();
    public Task SetLastAppVersion(Version version);
}
