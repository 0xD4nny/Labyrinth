namespace Labyrinth;
public enum Statustype
{
    Info = 0,
    Create = 1,
    SuccsessfullCMD = 2,
    Coordinates2 = 3,
    Map = 4,
    FailedCMD = 5,
    Won = 8,
    Coordinates = 9,
}
public class ServerResponse
{
    public readonly Statustype StatusType;
    public readonly string Message;

    public ServerResponse(Statustype statusType, string message)
    {
        StatusType = statusType;
        Message = message;
    }

    /// <summary>
    /// Splits the string-response in StatusType and Massage.
    /// </summary>
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