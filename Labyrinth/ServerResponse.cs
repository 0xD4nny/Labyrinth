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
class ServerResponse
{
    public readonly Statustype StatusType;
    public readonly string Message;

    public ServerResponse(Statustype statusType, string message)
    {
        StatusType = statusType;
        Message = message;
    }


    /// <summary>
    /// Splits the string-response in StatusType and Message.
    /// </summary>
    public static ServerResponse ParseResponse(string? response)
    {
        if (response is null)
            throw new ArgumentNullException(nameof(response), "response can't be null. Disconnected?");

        return new ServerResponse(ConvertResponse(response.Substring(0, 1)), response.Substring(2));
    }

    /// <summary>
    /// Converts the first character of the response string into a Statustype enumeration.
    /// </summary>
    private static Statustype ConvertResponse(string response)
    {
        // Subtract 48 from the ASCII value of the first character to convert it to its corresponding integer (0-9)
        return (Statustype)(response[0] - 48);
    }

}