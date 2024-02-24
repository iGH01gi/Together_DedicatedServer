using Google.Protobuf.Protocol;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerInfo Info { get; set; } = new PlayerInfo();
    public int RoomId { get; set; }
    public ClientSession Session { get; set; }

    public void CopyFrom(Player dediPlayer)
    {
        Info.PlayerId = dediPlayer.Info.PlayerId;
        Info.Name = dediPlayer.Info.Name;
        Info.Transform = dediPlayer.Info.Transform;
        RoomId = dediPlayer.RoomId;
        Session = dediPlayer.Session;
    }
}