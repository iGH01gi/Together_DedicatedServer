
using System;
using Google.Protobuf.Protocol;
using UnityEngine;

/// <summary>
/// 시간과 관련된 기능을 관리하는 클래스 
/// </summary>
public class TimeManager : MonoBehaviour
{
    private int _daySeconds = 60; //낮 시간(초)
    private int _nightSeconds = 200; //밤 시간(초)
    private float _currentTimer = 0f; //현재 시간(초)
    
    private float _syncPacketTimer = 0f; //동기화 패킷을 위한 타이머
    private float _syncPacketInterval = 5f; //동기화 패킷을 보내는 간격(초)

    private int _dayNightInterval = 2;//낮, 밤 사이 전환 간격(초)
    
    private bool _isDay = false; 
    private bool _isNight = false;
    private void Update()
    {
        TimerLogic();
    }

    /// <summary>
    /// 낮 타이머 시작 + 시작 패킷 보냄
    /// </summary>
    public void DayTimerStart()
    {
        _isNight = false;
        _isDay = true;
        _currentTimer = _daySeconds;
        _syncPacketTimer = 0f;
        
        DSC_DayTimerStart dayTimerStartPacket = new DSC_DayTimerStart();
        dayTimerStartPacket.DaySeconds = _daySeconds;
        Managers.Player.Broadcast(dayTimerStartPacket);
    }
    
    /// <summary>
    /// 밤 타이머 시작 + 시작 패킷 보냄
    /// </summary>
    public void NightTimerStart()
    {
        _isDay = false;
        _isNight = true;
        _currentTimer = _nightSeconds;
        _syncPacketTimer = 0f;
        
        DSC_NightTimerStart nightTimerStartPacket = new DSC_NightTimerStart();
        nightTimerStartPacket.NightSeconds = _nightSeconds;
        Managers.Player.Broadcast(nightTimerStartPacket);
    }
    /// <summary>
    /// 타이머 종료
    /// </summary>
    private void TimerStop()
    {
        _isDay = false;
        _isNight = false;
        _currentTimer = 0;
        _syncPacketTimer = 0;
    }

    /// <summary>
    /// 5초 간격으로 동기화 패킷을 보냄. 0초가 되면 끝났다는 패킷 보냄
    /// </summary>
    private void TimerLogic()
    {
        if (_isDay)
        {
            _currentTimer -= Time.deltaTime;
            if (_currentTimer <= 0) //낮이 끝났다는 패킷을 보내고 타이머를 멈춤 + n초후 밤 타이머 시작과 해당 패킷 전송
            {
                DSC_DayTimerEnd dayTimerEndPacket = new DSC_DayTimerEnd();
                Managers.Player.Broadcast(dayTimerEndPacket);
                TimerStop();
                
                JobTimer.Instance.Push(() =>
                {
                    NightTimerStart();
                }, _dayNightInterval);
            }
            else
            {
                _syncPacketTimer += Time.deltaTime;
                if (_syncPacketTimer >= _syncPacketInterval) //5초마다 동기화 패킷을 보냄
                {
                    DSC_DayTimerSync dayTimerSyncPacket = new DSC_DayTimerSync();
                    dayTimerSyncPacket.CurrentServerTimer = _currentTimer;
                    Managers.Player.Broadcast(dayTimerSyncPacket);
                    _syncPacketTimer = 0;
                }
            }
        }
        else if (_isNight)
        {
            _currentTimer -= Time.deltaTime;
            if (_currentTimer <= 0) //밤이 끝났다는 패킷을 보내고 타이머를 멈춤 + n초후 낮 타이머 시작과 해당 패킷 전송
            {
                DSC_NightTimerEnd nightTimerEndPacket = new DSC_NightTimerEnd();
                Managers.Player.Broadcast(nightTimerEndPacket);
                TimerStop();
                
                JobTimer.Instance.Push(() =>
                {
                    DayTimerStart();
                }, _dayNightInterval);
            }
            else
            {
                _syncPacketTimer += Time.deltaTime;
                if (_syncPacketTimer >= _syncPacketInterval) //5초마다 동기화 패킷을 보냄
                {
                    DSC_NightTimerSync nightTimerSyncPacket = new DSC_NightTimerSync();
                    nightTimerSyncPacket.CurrentServerTimer = _currentTimer;
                    Managers.Player.Broadcast(nightTimerSyncPacket);
                    _syncPacketTimer = 0;
                }
            }
        }
    }
}
