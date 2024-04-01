
using System;
using System.Collections.Generic;

public class SessionManager
{
    int _sessionId = 0;
    
    public Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
    object _lock = new object();

    public void Init()
    {
        JobTimer.Instance.Push(Managers.Session.CheckSessionNum); //세션 갯수 1초마다 확인
    }

    public ClientSession Generate()
    {
        lock (_lock)
        {
            int sessionId = ++_sessionId;

            ClientSession session = new ClientSession();
            session.SessionId = sessionId;
            _sessions.Add(sessionId, session);

            Console.WriteLine($"Connected sessionId : {sessionId}");
            Console.WriteLine($"Connected session count : {_sessions.Count}");

            return session;
        }
    }

    public ClientSession Find(int id)
    {
        lock (_lock)
        {
            ClientSession session = null;
            _sessions.TryGetValue(id, out session);
            return session;
        }
    }

    public void Remove(ClientSession session)
    {
        lock (_lock)
        {
            _sessions.Remove(session.SessionId);
        }
    }
    
    public void CheckSessionNum()
    {
        Util.PrintLog($"session num : {Managers.Session._sessions.Count}");
        JobTimer.Instance.Push(CheckSessionNum,1000); //1초 간격으로 
    }
}