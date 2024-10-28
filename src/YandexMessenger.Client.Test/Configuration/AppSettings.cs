using Microsoft.Extensions.Configuration;
using YandexMessenger.Client.Settings;

namespace YandexMessenger.Client.Test.Configuration;

public class AppSettings
{
    public static AppSettings Instance { get; set; } = new();
    public YandexMessengerApiSettings YandexApi { get; set; } = new();
    public static void InitFromFile(string[] args)
    {
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddUserSecrets<AppSettings>()
            .AddCommandLine(args)
            .Build()
            .Bind(Instance);
    }
}
