﻿#nullable disable warnings

namespace Remindery.Configuration;

public class BotConfiguration
{
    public const string SectionName = "Bot";

    public string BotToken { get; set; }

    public string WebhookUrlSecret { get; set; }
}