
using System;
using UnityEngine;

public class LogicManager
{
    private float _tick = 0.05f; //초당 20회(이 주기마다 동기화, 클라와 맞춰야 함)
    private float _timer = 0.0f;
    
    /// <summary>
    /// 플레이어 움직임 정보 보내는 이벤트
    /// </summary>
    public event Action SendPlayerMoveEvent;
    
    public void Update()
    {
        //TODO: 다른 플레이어 움직임 동기화 패킷 받아서 먼저 처리
        
        _timer += Time.deltaTime;
        if(_timer >= _tick)
        {
           //플레이어들의 움직임 정보 보냄(DRM)
            _timer = 0;
        }
    }
}