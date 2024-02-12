using System;
using System.Net;
using System.Net.Sockets;

public class Listener
{
    Socket _listenSocket;
    private Func<Session> _sessionFactory; //인자는 없고 리턴이 session인 함수

    public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory += sessionFactory;

        //문지기 교육
        _listenSocket.Bind(endPoint);

        //영업시작
        //backlog: 최대 대기수
        _listenSocket.Listen(backlog);

        //문지기가 register명 만큼
        for (int i = 0; i < register; i++)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }
    }

    void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null; //깨끗한 상태로 다시 사용가능하게

        bool pending = _listenSocket.AcceptAsync(args);
        if (pending == false)
        {
            OnAcceptCompleted(null, args);
        }
    }

    void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            //TODO
            Session session = _sessionFactory.Invoke();
            session.Start(args.AcceptSocket);
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
        }
        else
            Console.WriteLine(args.SocketError.ToString());

        RegisterAccept(args);
    }

    public Socket Accept()
    {
        return _listenSocket.Accept();
    }
}