using System;
using System.Net;

public class NetworkManager
{
    static Listener _listener = new Listener();
    
    public void Init() //데디케이티드 서버 정보... 원래는 이런 고정이 아니라 게임룸서버에 의해서 동적으로 설정되어야함.
    {
        //DNS
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

        _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
        Console.WriteLine("Listening...");
    }
}