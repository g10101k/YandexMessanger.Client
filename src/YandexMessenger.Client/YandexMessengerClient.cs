using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YandexMessenger.Client.Exceptions;
using YandexMessenger.Client.Settings;

namespace YandexMessenger.Client;

public sealed class YandexMessengerClient : HttpClient
{
#pragma warning disable S1075
    private const string YandexUri = "https://botapi.messenger.yandex.net";
#pragma warning restore S1075

    private static readonly Lazy<JsonSerializerSettings> Settings = new(CreateSerializerSettings, true);

    private static JsonSerializerSettings CreateSerializerSettings()
    {
        var settings = new JsonSerializerSettings();
        UpdateJsonSerializerSettings(settings);
        return settings;
    }

    static void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    }

    private JsonSerializerSettings JsonSerializerSettings => Settings.Value;

    public YandexMessengerClient(string xOrgId, string oAuth)
    {
        BaseAddress = new Uri(YandexUri);
        DefaultRequestHeaders.Add("X-Org-ID", xOrgId);
        DefaultRequestHeaders.Add("Authorization", $"OAuth {oAuth}");
    }

    public YandexMessengerClient(YandexMessengerApiSettings settings)
    {
        BaseAddress = new Uri(YandexUri);
        DefaultRequestHeaders.Add("Authorization", $"OAuth {settings.OAuth}");
    }

    public async Task<SendTextResponse> SendText(SendMessageRequest request)
    {
        var s = JsonConvert.SerializeObject(request, Settings.Value);
        var response = await PostAsync($"/bot/v1/messages/sendText/", new StringContent(s, encoding: Encoding.UTF8,
            "application/json"
        ));
        var status = (int)response.StatusCode;

        if (status == 200)
        {
            var objectResponse = await ReadObjectResponseAsync<SendTextResponse>(response, CancellationToken.None)
                .ConfigureAwait(false);
            if (objectResponse.Object == null)
            {
                throw new ApiException("Response was null which was not expected.", status, objectResponse.Text,
                    null);
            }

            return objectResponse.Object;
        }

        var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        throw new ApiException("The HTTP status code of the response was not expected (" + status + ").", status,
            responseData, null);
    }

    private struct ObjectResponseResult<T>
    {
        public ObjectResponseResult(T responseObject, string responseText)
        {
            Object = responseObject;
            Text = responseText;
        }

        public T Object { get; }

        public string Text { get; }
    }

    private async Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(
        HttpResponseMessage? response,
        CancellationToken cancellationToken, bool readResponseAsString = false)
    {
        if (response == null)
        {
            return new ObjectResponseResult<T>(default(T), string.Empty);
        }

        try
        {
            if (readResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, exception);
                }
            }
            else
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var streamReader = new StreamReader(responseStream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = JsonSerializer.Create(JsonSerializerSettings);
                    var typedBody = serializer.Deserialize<T>(jsonTextReader);
                    return new ObjectResponseResult<T>(typedBody, string.Empty);
                }
            }
        }
        catch (JsonException exception)
        {
            var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
            throw new ApiException(message, (int)response.StatusCode, string.Empty, exception);
        }
    }
}