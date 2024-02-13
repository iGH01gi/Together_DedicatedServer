using System;
using System.Net;
using System.Net.Sockets;

public class Connector
{
    private Func<Session> _sessionFactory;

    public void Connect(IPEndPoint endPoint,Func<Session> sessionFactory, int count=1)
    {
        //다수 클라이언트 테스트를 위해서 count만큼 가상의 count개의 클라이언트 생성
        for (int i = 0; i < count; i++)
        {
            //휴대폰 설정
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory= sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket; //리스너와 달리 전역변수 사용안하고 유저토큰으로 소켓 넘겨줌

            RegisterConnect(args);
        }
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket socket = args.UserToken as Socket;
        if (socket == null)
            return;

        bool pending = socket.ConnectAsync(args);
        if (pending == false)
        {
            OnConnectCompleted(null,args);
        }
    }

    void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session= _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
        }
    }
}