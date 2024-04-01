using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class PacketHandler
{
    //클라가 데디서버와 연결되어있음을 확인시키기 위해서 보내는 핑퐁 패킷을 처리
    public static void CDS_PingPongHandler(PacketSession session, IMessage packet)
    {
        CDS_PingPong pingPongPacket = packet as CDS_PingPong;
        ClientSession clientSession = session as ClientSession;

        clientSession.PingPong._isPong = true;
    }

    //데디서버 입장을 요청하는 패킷을 처리
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
    
    //클라에서 주기적으로 보내는 움직임동기화 패킷을 처리(핵 아닐시 고스트 위치 설정 + 다른 클라들에게 움직임 동기화패킷 보냄)
    public static void CDS_MoveHandler(PacketSession session, IMessage packet)
    {
        CDS_Move movePacket = packet as CDS_Move;
        ClientSession clientSession = session as ClientSession;
        
        Managers.Player.ProcessingCDSMove(clientSession.SessionId, movePacket);
    }
}