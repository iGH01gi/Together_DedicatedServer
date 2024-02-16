using System;
using System.Net;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class ClientSession : PacketSession
{
    public Player MyPlayer { get; set; }
    public int SessionId { get; set; }
    
    public PingPong PingPong { get; set; }

    public void Send(IMessage packet)
    {
        /*string msgName = packet.Descriptor.Name.Replace("_", String.Empty);
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId),msgName);*/
        string[] parts = packet.Descriptor.Name.Split('_');
        parts[0] = char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower();
        string msgName = string.Join("_", parts);
        msgName = msgName.Replace("_", "");
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId),msgName);

        ushort size = (ushort)packet.CalculateSize();
        byte[] sendBuffer = new byte[size + 4];
        Array.Copy(BitConverter.GetBytes((ushort)size + 4), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

        Send(new ArraySegment<byte>(sendBuffer));
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        
        PingPong = new PingPong(this);
        PingPong.SendPing();
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        SessionManager.Instance.Remove(this);
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {
        //Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
}