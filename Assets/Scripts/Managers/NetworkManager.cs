using System;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf;

public class NetworkManager
{
    static Listener _listener = new Listener();
    
    public void Init() //데디케이티드 서버 정보... 원래는 이런 고정이 아니라 게임룸서버에 의해서 동적으로 설정되어야함.
    {
        //DNS
        /*string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];*/
        IPAddress ipAddr = IPAddress.Any;
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

        _listener.Init(endPoint, () => { return Managers.Session.Generate(); });
        Util.PrintLog("Listening...");
    }
    
    /// <summary>
    /// 패킷큐에서 지속적으로 패킷을 뽑아서 처리하는 함수 (클라들로부터 받은걸 처리) 
    /// 매 프레임마다 큐에 있는 모든걸 꺼내기 위해 PopAll() 사용
    /// 실제 뽑는건 메인쓰레드가 Managers의 Update에서 처리
    /// </summary>
    public void Update()
    {
        List<PacketMessage> list = PacketQueue.Instance.PopAll();
        foreach (PacketMessage packet in list)
        {
            Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
            if (handler != null)
                handler.Invoke(packet.Session, packet.Message);
        }	
    }
}