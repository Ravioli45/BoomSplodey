using System.Text.Json;
using System.Text.Json.Serialization;

//[JsonDerivedType(typeof(LANMessage), typeDiscriminator: "base")]
[JsonDerivedType(typeof(SearchMessage), typeDiscriminator: "SEARCH")]
[JsonDerivedType(typeof(FoundMessage), typeDiscriminator: "FOUND")]
public class LANMessage
{
    public string GameID { get; set; }

    public override string ToString()
    {
        return $"{GetType().Name}{JsonSerializer.Serialize(this)}";
    }
}

public class SearchMessage : LANMessage{}

public class FoundMessage : LANMessage
{
    public string LobbyName { get; set; }
}

public static class LANMessageBuilder
{
    // this identifier intended to be unique for every game
    // using this protocol. helps games identify
    // which traffic is intended for them
    public const string GameID = "PlaceholderText";

    public static SearchMessage BuildSearchMessage()
    {
        return new SearchMessage
        {
            GameID = LANMessageBuilder.GameID,
        };
    }

    public static FoundMessage BuildFoundMessage(string lobbyname)
    {
        return new FoundMessage
        {
            GameID = LANMessageBuilder.GameID,
            LobbyName = lobbyname,
        };
    }
}
