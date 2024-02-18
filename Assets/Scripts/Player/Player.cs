using Google.Protobuf.Protocol;

public class Player
{
    public PlayerInfo Info { get; set; } = new PlayerInfo();
    public int RoomId { get; set; }
    public ClientSession Session { get; set; }
}