using System;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PingPong
{
    ClientSession _session;
    
    int _connectionLossCount = 0;
    public bool _isPong = false; //true이면 pong이 왔다는 뜻. false이면 pong이 안 왔다는 뜻.

    public PingPong(ClientSession session)
    {
        _session = session;
    }
    
    /// <summary>
    /// 클라이언트에게 ping을 보내는 함수. 3초간격으로 보냄 (helth check용)
    /// </summary>
    public void SendPing()
    {
        Util.PrintLog("핑보냄");
        DSC_PingPong sendPacket = new DSC_PingPong();
        _session.Send(sendPacket);
        
        JobTimer.Instance.Push(CheckPong, 3000); //3초 간격으로 확인
    }

    /// <summary>
    /// 클라이언트로부터 pong이 왔는지 확인하는 함수. 3회이상 실패하면 disconnect()
    /// </summary>
    public void CheckPong()
    {
        if (_isPong == false)
        {
            _connectionLossCount++;
            if (_connectionLossCount >= 3)
            {
                Util.PrintLog($"3회 핑퐁 실패 세션아이디:{_session.SessionId} ");
                /*Console.WriteLine($"3회 핑퐁 실패 세션아이디:{_session.SessionId} ");
                Debug.Log($"3회 핑퐁 실패 세션아이디:{_session.SessionId} ");*/
                _session.Disconnect();
                return;
            }
        }
        else
        {
            _connectionLossCount = 0;
        }
        
        _isPong = false;
        SendPing();
    }
    
}

/*

1. 해당 클라세션을 통해서 ping 보낸다(처음 시작은 onconnected함수에서. 보내는 함수는 pingpon클래스 안에 구현하돼, 그 함수 속에 보낸 후에 잡타이머에 3초후에 확인하는 함수를 등록한다)
1-1. 클라가 응답패킷을 보내면, 서버에서는 해당 클라세션의 bool변수를 true로 변환해준다.

2. 잡타이머에 클라로부터 pong이 왔는지 3초 뒤에 확인하도록 push한다. (ping보내는 함수 안에 push가 마지막에 포함되어있어야함.)

3. 5초 뒤에 확인했을때 해당 클라세션의 bool값이 false라면 clientsessiont의 connectionLossCount를 증가시킨다

4. connectionLossCount가 3이면 disconnect()

3-1. 5초 뒤에 확인했을때 해당 클라세션의 bool값이 true 라면 clientsession의 connectionLossCount를 0으로 초기화 시킨다

*/