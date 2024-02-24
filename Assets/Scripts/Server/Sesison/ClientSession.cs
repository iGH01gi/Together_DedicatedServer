using System;
using System.Net;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class ClientSession : PacketSession
{
    public Player MyPlayer { get; set; } //실제 하이레키에 존재하는 컴포넌트아님. 순수 데이터 클래스만 들고있는것임. (실제 컴포넌트는 플레이어매니저에서 관리)
    public int SessionId { get; set; } // 플레이어id에서도 이 값을 똑같이 사용함
    
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
        Util.PrintLog($"OnConnected : {endPoint}");
        
        if (PacketManager.Instance.CustomHandler == null)
        {
            PacketManager.Instance.CustomHandler = (s, m, i) =>
            {
                PacketQueue.Instance.Push(s, i, m);
            };
        }
        
        PingPong = new PingPong(this);
        PingPong.SendPing();
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        if (MyPlayer != null)
        {
            Managers.Player.LeaveGame(MyPlayer.Session.SessionId); //플레이어매니저에서 정리 + 다른 클라이언트들한테 알림
        }
        Managers.Session.Remove(this); //세션매니저에서 정리
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {
        //Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
}