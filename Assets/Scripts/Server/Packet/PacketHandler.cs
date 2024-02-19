using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class PacketHandler
{
    public static void CDS_PingPongHandler(PacketSession session, IMessage packet)
    {
        CDS_PingPong pingPongPacket = packet as CDS_PingPong;
        ClientSession clientSession = session as ClientSession;

        clientSession.PingPong._isPong = true;
    }

    public static void CDS_AllowEnterGameHandler(PacketSession session, IMessage packet)
    {
        Util.PrintLog("CDS_AllowEnterGameHandler");
        
        CDS_AllowEnterGame allowEnterGamePacket = packet as CDS_AllowEnterGame;
        ClientSession clientSession = session as ClientSession;

        //PlayerManager를 통해서 해당 플레이어 정보를 생성하고 등록함. (이때 데디playerId는 데디세션id와 동일)
        //이때, 내부적으로 해당 플레이어 정보를 ClientSession도 들고있게 함
        //마지막으로 내부적으로 DSC_InformNewFaceInDedicatedServer을 모든 클라이언트에게 보내서 해당 플레이어가 데디서버에 접속하였음을 알림 
        int roomId = allowEnterGamePacket.RoomId;
        string name = allowEnterGamePacket.Name;
        Managers.Player.AddPlayer(clientSession, roomId, name);
    }
}