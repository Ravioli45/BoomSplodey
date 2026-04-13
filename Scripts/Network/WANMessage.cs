using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Godot;
using System.Text;

public class LobbyInfo
{
    public string LobbyName { get; set; }
    public ushort Port { get; set; }

    public override string ToString()
    {
        return $"{GetType().Name}{{LobbyName:{LobbyName}, Port:{Port}}}";
    }
}

[JsonDerivedType(typeof(FetchMessage), typeDiscriminator: "FETCH")]
[JsonDerivedType(typeof(ListMessage), typeDiscriminator: "LIST")]
[JsonDerivedType(typeof(HostMessage), typeDiscriminator: "HOST")]
[JsonDerivedType(typeof(JoinMessage), typeDiscriminator: "JOINED")]
[JsonDerivedType(typeof(CreatedMessage), typeDiscriminator: "CREATED")]
[JsonDerivedType(typeof(NotCreatedMessage), typeDiscriminator: "NOTCREATED")]
public class WANMessage { }

public class FetchMessage : WANMessage
{
    public override string ToString()
    {
        return $"{GetType().Name}{JsonSerializer.Serialize(this)}";
    }
}
public class ListMessage : WANMessage
{
    public List<LobbyInfo> Games { get; set; }

    public override string ToString()
    {
        string repr = $"{GetType().Name}[";
        foreach (LobbyInfo info in Games)
        {
            repr += info.ToString() + " ";
        }
        repr += "]";
        return repr;
    }
}
public class HostMessage : WANMessage
{
    public string LobbyName { get; set; }
}
public class JoinMessage : WANMessage
{
    public ushort Port { get; set; }
}
public class CreatedMessage : WANMessage
{
    public ushort Port { get; set; }

    public override string ToString()
    {
        return $"{GetType().Name}{{Port:{Port}}}";
    }
}
public class NotCreatedMessage : WANMessage { }

class WANMessageStream
{
    enum MessageStreamState
    {
        WAITING_FOR_LENGTH,
        WAITING_FOR_DATA,
    }
    private MessageStreamState State = MessageStreamState.WAITING_FOR_LENGTH;
    private uint DataLength = 0;
    private Queue<WANMessage> MessageQueue = [];
    private StreamPeerTcp Stream = new();
    private JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, IncludeFields = true };

    public WANMessageStream()
    {
        Stream.BigEndian = true;
    }

    public void DisconnectFromHost() => Stream.DisconnectFromHost();
    public StreamPeerSocket.Status GetStatus() => Stream.GetStatus();
    public Error Bind(int port, string host = "*") => Stream.Bind(port, host);
    public Error ConnectToHost(string host, int port) => Stream.ConnectToHost(host, port);
    public string GetConnectedHost() => Stream.GetConnectedHost();
    public int GetConnectedPort() => Stream.GetConnectedPort();
    public int GetLocalPort() => Stream.GetLocalPort();
    public void SetNoDelay(bool enabled) => Stream.SetNoDelay(enabled);

    public Error Poll()
    {
        Error internal_poll = Stream.Poll();
        //if (internal_poll != Error.Ok)
        //{
        //    return internal_poll;
        //}

        if (Stream.GetStatus() == StreamPeerSocket.Status.Connected)
        {
            switch (State)
            {
                case MessageStreamState.WAITING_FOR_LENGTH:
                    if (Stream.GetAvailableBytes() >= 4)
                    {
                        DataLength = Stream.GetU32();
                        State = MessageStreamState.WAITING_FOR_DATA;
                    }
                    break;
                case MessageStreamState.WAITING_FOR_DATA:
                    if (Stream.GetAvailableBytes() >= DataLength)
                    {
                        string data = Stream.GetUtf8String((int)DataLength);
                        State = MessageStreamState.WAITING_FOR_LENGTH;
                        //GD.Print("Received: " + data);
                        try
                        {
                            WANMessage message = JsonSerializer.Deserialize<WANMessage>(data, JsonOptions);
                            MessageQueue.Enqueue(message);
                        }
                        catch { }
                    }
                    break;
            }
        }

        //return Error.Ok;
        return internal_poll;
    }

    public bool TryRecvNextMessage(out WANMessage message)
    {
        if (MessageQueue.Count > 0)
        {
            message = MessageQueue.Dequeue();
            return true;
        }
        else
        {
            message = null;
            return false;
        }
    }

    public Error SendMessage(WANMessage message)
    {
        string data = JsonSerializer.Serialize<WANMessage>(message, JsonOptions);
        GD.Print("sending: " + data);
        byte[] bytes = Encoding.UTF8.GetBytes(data);

        Stream.PutU32((uint)bytes.Length);
        return Stream.PutData(bytes);
    }
}
