
using YandexMessenger.Client.Test.Configuration;

namespace YandexMessenger.Client.Test;

public static class Program
{
    public static void Main(string[] args)
    {
        AppSettings.InitFromFile(args);

        using var client = new YandexMessengerClient(AppSettings.Instance.YandexApi);
        client.SendText(new SendMessageRequest()
        {
            Login = "igor.tyulyakov@indusoft.ru",
            Text = "Test message"
        }).Wait();
    }
}