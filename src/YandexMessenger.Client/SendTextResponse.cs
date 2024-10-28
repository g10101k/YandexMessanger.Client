using Newtonsoft.Json;

namespace YandexMessenger.Client;

public class SendTextResponse 
{
    [JsonProperty("message_id")] public long MessageId { get; set; }
    [JsonProperty("ok")] public bool Ok { get; set; }
}