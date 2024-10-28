namespace YandexMessenger.Client.Exceptions;

/// <summary>
/// Исключение при выполнении запроса
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; private set; }

    public string? Response { get; private set; }

    /// <inheritdoc />
    public ApiException(string message, int statusCode, string? response, Exception? innerException)
        : base(
            message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null)
                ? "(null)"
                : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
    {
        StatusCode = statusCode;
        Response = response;
    }

    public override string ToString()
    {
        return $"HTTP Response: \n\n{Response}\n\n{base.ToString()}";
    }
}
