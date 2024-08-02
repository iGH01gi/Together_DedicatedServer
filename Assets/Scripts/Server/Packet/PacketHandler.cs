using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Unity.Profiling.LowLevel.Unsafe;

public class PacketHandler
{
    //클라가 데디서버와 연결되어있음을 확인시키기 위해서 보내는 핑퐁 패킷을 처리
    public static void CDS_PingPongHandler(PacketSession session, IMessage packet)
    {
        CDS_PingPong pingPongPacket = packet as CDS_PingPong;
        ClientSession clientSession = session as ClientSession;

        clientSession.PingPong._isPong = true;
    }
    
    //방장이 데디서버에게 방 정보를 보내는 패킷을 처리
    public static void CDS_InformRoomInfoHandler(PacketSession session, IMessage packet)
    {
        CDS_InformRoomInfo informRoomInfoPacket = packet as CDS_InformRoomInfo;
        ClientSession clientSession = session as ClientSession;
        
        Managers.Player.SetRoomInfo(informRoomInfoPacket.RoomId, informRoomInfoPacket.PlayerNum);
        
        Util.PrintLog($"방장이 알려준 방정보 : 방번호 {informRoomInfoPacket.RoomId}, 방인원 {informRoomInfoPacket.PlayerNum}명");
        
        //방장이 방 정보를 알려줬으면 3초 후에 게임 시작 패킷을 모두에게 보냄.(이 패킷을 받은 클라는  3,2,1,카운트 후 GameStart를 띄우고  게임을 시작함)
        //이때 상자 생성패킷도 함께 보냄
        JobTimer.Instance.Push(() =>
        {
            //상자 생성 및 정보 전송
            //Managers.Object._chestController.ChestSetAllInOne();
            
            //시작 패킷 전송
            DSC_StartGame startGamePacket = new DSC_StartGame();
            //startGamePacket.Items에 Application.dataPath+"/Data/Item/Items.json 내용을 string으로 보냄
            startGamePacket.Items = Managers.Item.GetItemsDataJson();
            //startGamePacket.Killers에 Application.dataPath+"/Data/Killer/Killers.json 내용을 string으로 보냄
            startGamePacket.Killers = Managers.Killer.GetKillersDataJson();
            Managers.Player.Broadcast(startGamePacket);
        }, 3000);
        
        JobTimer.Instance.Push(() =>
        {
            //낮 시작 패킷 전송
            Managers.Time.DayTimerStart();
        }, 6000);
        
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
        Managers.Player.AddPlayer(clientSession, roomId, name); //임시테스트
        
        
    }
    
    //클라에서 주기적으로 보내는 움직임동기화 패킷을 처리(핵 아닐시 고스트 위치 설정 + 다른 클라들에게 움직임 동기화패킷 보냄)
    public static void CDS_MoveHandler(PacketSession session, IMessage packet)
    {
        CDS_Move movePacket = packet as CDS_Move;
        ClientSession clientSession = session as ClientSession;
        
        Managers.Player._playerMoveController.ProcessingCDSMove(clientSession.SessionId, movePacket);
        //10ms 레이턴시 가정하고 테스트
        /*JobTimer.Instance.Push(() =>
        {
            Managers.Player._playerMoveController.ProcessingCDSMove(clientSession.SessionId, movePacket);
        }, 10);*/
       //Managers.Player._playerMoveController.ProcessingCDSMove(clientSession.SessionId, movePacket);
    }
    
    //클라에서 상자 열기를 요청하는 패킷을 처리
    public static void CDS_TryChestOpenHandler(PacketSession session, IMessage packet)
    {
        CDS_TryChestOpen tryChestOpenPacket = packet as CDS_TryChestOpen;
        ClientSession clientSession = session as ClientSession;
        
        Managers.Object._chestController.ClientTryChestOpen(tryChestOpenPacket);
    }
    
    //클라에서 타임스탬프를 요청하는 패킷을 처리
    public static void CDS_RequestTimestampHandler(PacketSession session, IMessage packet)
    {
        CDS_RequestTimestamp requestTimestampPacket = packet as CDS_RequestTimestamp;
        ClientSession clientSession = session as ClientSession;
        
        DSC_ResponseTimestamp sendPacket = new DSC_ResponseTimestamp();
        sendPacket.Timestamp = DateTime.UtcNow.ToTimestamp();
        clientSession.Send(sendPacket);
    }
    
    //클라에서 클린즈 사용 권한을 요청하는 패킷을 처리
    public static void CDS_RequestCleansePermissionHandler(PacketSession session, IMessage packet)
    {
        CDS_RequestCleansePermission requestCleansePermissionPacket = packet as CDS_RequestCleansePermission;
        ClientSession clientSession = session as ClientSession;
        
        int dediPlayerId =  requestCleansePermissionPacket.MyDediplayerId;
        int cleanseId = requestCleansePermissionPacket.CleanseId;
        
        Managers.Object._cleanseController.ClientTryCleanse(dediPlayerId, cleanseId);
    }
    
    //클라에서 클린즈 취소를 알린 패킷을 처리
    public static void CDS_CleanseQuitHandler(PacketSession session, IMessage packet)
    {
        CDS_CleanseQuit cleanseQuitPacket = packet as CDS_CleanseQuit;
        ClientSession clientSession = session as ClientSession;
        
        int dediPlayerId = cleanseQuitPacket.MyDediplayerId;
        int cleanseId = cleanseQuitPacket.CleanseId;
        
        Managers.Object._cleanseController.ClientQuitCleanse(dediPlayerId, cleanseId);
    }
    
    public static void CDS_CleanseSuccessHandler(PacketSession session, IMessage packet)
    {
        CDS_CleanseSuccess cleanseSuccessPacket = packet as CDS_CleanseSuccess;
        ClientSession clientSession = session as ClientSession;
        
        int dediPlayerId = cleanseSuccessPacket.MyDediplayerId;
        int cleanseId = cleanseSuccessPacket.CleanseId;
        
        Managers.Object._cleanseController.ClientCleanseSuccess(dediPlayerId, cleanseId);
    }
}