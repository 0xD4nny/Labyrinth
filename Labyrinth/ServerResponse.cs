namespace Labyrinth;

public class ServerResponse
{
    public readonly Statustype StatusType;
    public readonly string Message;

    public ServerResponse(Statustype statusType, string message)
    {
        StatusType = statusType;
        Message = message;
    }

    public static ServerResponse ParseResponse(string? response)
    {
        if (response is null)
            throw new ArgumentNullException(nameof(response), "response can't be null. Disconnected?");

        return new ServerResponse(ConvertResponse(response.Substring(0, 1)), response.Substring(2));
    }

    private static Statustype ConvertResponse(string response)
    {
        return (Statustype)(response[0] - 48);
    }

}