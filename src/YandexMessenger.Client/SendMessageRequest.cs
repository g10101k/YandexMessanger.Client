using Newtonsoft.Json;

namespace YandexMessenger.Client;

public class SendMessageRequest
{
    [JsonProperty("chat_id")] public string? ChatId { get; set; }
    [JsonProperty("login")] public string? Login { get; set; }
    [JsonProperty("text")] public string Text { get; set; } = default!;
}