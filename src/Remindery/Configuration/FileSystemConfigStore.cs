﻿using System.Reflection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace Remindery.Configuration;

internal class FileSystemConfigStore : IConfigStore
{
    private readonly DeploymentConfiguration _deploymentConfig;
    private readonly SemaphoreSlim _storeLock = new(1, 1);

    public FileSystemConfigStore(IOptions<DeploymentConfiguration> deploymentConfig)
    {
        _deploymentConfig = deploymentConfig.Value;
    }

    public async Task<ChatId[]> GetAllowedChatIds()
    {
        var config = await GetConfig();
        return config.AllowedChatIds.Select(x => new ChatId(x)).ToArray();
    }

    public async Task<ChatId?> GetAdminChatId()
    {
        var config = await GetConfig();
        return config.AdminChatId != null ? new ChatId(config.AdminChatId) : null;
    }

    public async Task<Version?> GetLastAppVersion()
    {
        var config = await GetConfig();
        if (config.LastVersion is { } lastVersion)
            return Version.Parse(lastVersion);

        return null;
    }

    public async Task SetLastAppVersion(Version version)
    {
        await StoreConfig(config =>
        {
            config.LastVersion = version.ToString();
        });
    }

    private async Task<Config> GetConfig()
    {
        var configPath = GetConfigPath();
        if (!File.Exists(configPath))
            return new();

        var fileContents = await File.ReadAllTextAsync(configPath);
        return JsonConvert.DeserializeObject<Config>(fileContents)!;
    }


    private async Task StoreConfig(Action<Config> modifyConfig)
    {
        await _storeLock.WaitAsync();
        try
        {
            var config = await GetConfig();
            modifyConfig(config);
            await File.WriteAllTextAsync(GetConfigPath(), JsonConvert.SerializeObject(config, Formatting.Indented));
        }
        finally
        {
            _storeLock.Release();
        }
    }

    private string GetConfigPath()
    {
        return string.IsNullOrEmpty(_deploymentConfig.ConfigStore)
            ? Path.Join(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location), "config.json")
            : Path.Join(_deploymentConfig.ConfigStore, "config.json");
    }

    private class Config
    {
        public string[] AllowedChatIds { get; set; } = Array.Empty<string>();

        public string? LastVersion { get; set; }

        public string? AdminChatId { get; set; }
    }
}
