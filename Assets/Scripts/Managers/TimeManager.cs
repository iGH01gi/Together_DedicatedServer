﻿
using System;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 시간과 관련된 기능을 관리하는 클래스 
/// </summary>
public class TimeManager : MonoBehaviour
{
    private int _daySeconds = 60; //낮 시간(초)
    private int _nightSeconds = 200; //밤 시간(초)
    private float _currentTimer = 0f; //현재 시간(초)
    
    private bool _isDay = false; 
    private bool _isNight = false;
    
    private float _timerSyncPacketTimer = 0f; //타이머 동기화 패킷을 위한 타이머
    private float _timerSyncPacketInterval = 5f; //타이머 동기화 패킷을 보내는 간격(초)
    private int _dayNightInterval = 3000;//낮, 밤 사이 전환 간격(ms)
    
    private bool _isGaugeStart = false; //게이지 시작 여부
    private float _gaugeSyncPacketTimer = 0f; //게이지 동기화 패킷을 위한 타이머
    private float _gaugeSyncPacketInterval = 5f; //게이지 동기화 패킷을 보내는 간격(초)
    public float _gaugeMax = 180; //게이지 최대값
    private float _survivorGaugeDecreasePerSecond = 1; //생존자 초당 게이지 감소량
    private float _killerGaugeDecreasePerSecond = 2; //킬러의 초당 게이지 감소량
    

    private void Update()
    {
        TimerLogic(); //낮,밤 타이머 로직

        GaugeLogic(); //밤 생명력 게이지 로직
    }

    #region 타이머 관련

     /// <summary>
    /// 낮 타이머 시작 + 시작 패킷 보냄
    /// </summary>
    public void DayTimerStart()
    {
        Util.PrintLog("day timer start");
        _isNight = false;
        _isDay = true;
        _currentTimer = _daySeconds;
        _timerSyncPacketTimer = 0f;
        
        Managers.Player.ClearKiller();//킬러 초기화
        
        DSC_DayTimerStart dayTimerStartPacket = new DSC_DayTimerStart();
        dayTimerStartPacket.DaySeconds = _daySeconds;
        Managers.Player.Broadcast(dayTimerStartPacket);
    }
    
    /// <summary>
    /// 밤 타이머 시작 + 시작 패킷 보냄
    /// </summary>
    public void NightTimerStart()
    {
        Util.PrintLog("night timer start");
        _isDay = false;
        _isNight = true;
        _currentTimer = _nightSeconds;
        _timerSyncPacketTimer = 0f;
        
        GaugeStart();
        
        DSC_NightTimerStart nightTimerStartPacket = new DSC_NightTimerStart();
        nightTimerStartPacket.NightSeconds = _nightSeconds;
        nightTimerStartPacket.GaugeMax = _gaugeMax;
        nightTimerStartPacket.SurvivorGaugeDecreasePerSecond = _survivorGaugeDecreasePerSecond;
        nightTimerStartPacket.KillerGaugeDecreasePerSecond = _killerGaugeDecreasePerSecond;
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
        _timerSyncPacketTimer = 0;
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
                Util.PrintLog($"day timer end");
                DSC_DayTimerEnd dayTimerEndPacket = new DSC_DayTimerEnd();
                
                //킬러 초기화 + 선정
                Managers.Player.ClearKiller();
                int killerId = Managers.Player.RandomSelectKiller();
                dayTimerEndPacket.KillerPlayerId = killerId;
                
                Managers.Player.Broadcast(dayTimerEndPacket);
                TimerStop();
                
                JobTimer.Instance.Push(() =>
                {
                    NightTimerStart();
                }, _dayNightInterval);
            }
            else
            {
                _timerSyncPacketTimer += Time.deltaTime;
                if (_timerSyncPacketTimer >= _timerSyncPacketInterval) //5초마다 동기화 패킷을 보냄
                {
                    Util.PrintLog($"day timer {_currentTimer}s left");
                    DSC_DayTimerSync dayTimerSyncPacket = new DSC_DayTimerSync();
                    dayTimerSyncPacket.CurrentServerTimer = _currentTimer;
                    Managers.Player.Broadcast(dayTimerSyncPacket);
                    _timerSyncPacketTimer = 0;
                }
            }
        }
        else if (_isNight)
        {
            _currentTimer -= Time.deltaTime;
            if (_currentTimer <= 0) //밤이 끝났다는 패킷을 보내고 타이머를 멈춤 + n초후 낮 타이머 시작과 해당 패킷 전송
            {
                Util.PrintLog($"night timer end");
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
                _timerSyncPacketTimer += Time.deltaTime;
                if (_timerSyncPacketTimer >= _timerSyncPacketInterval) //5초마다 동기화 패킷을 보냄
                {
                    Util.PrintLog($"night timer {_currentTimer}s left");
                    DSC_NightTimerSync nightTimerSyncPacket = new DSC_NightTimerSync();
                    nightTimerSyncPacket.CurrentServerTimer = _currentTimer;
                    Managers.Player.Broadcast(nightTimerSyncPacket);
                    _timerSyncPacketTimer = 0;
                }
            }
        }
    }

    #endregion
    
    
    #region 밤 게이지 관련
    
    /// <summary>
    /// 밤 게이지 시작
    /// </summary>
    public void GaugeStart()
    {
        _gaugeSyncPacketTimer = 0;
        _isGaugeStart = true;
        
        //모든 플레이어의 게이지를 최대로 설정
        Managers.Player.SetAllGauge(_gaugeMax);
    }
    
    /// <summary>
    /// 밤 게이지 종료
    /// </summary>
    public void GaugeStop()
    {
        _gaugeSyncPacketTimer = 0;
        _isGaugeStart = false;
        
        //모든 플레이어의 게이지를 0으로 설정
        Managers.Player.SetAllGauge(0);
    }
    
    /// <summary>
    /// 밤 게이지 로직. 5초 간격으로 동기화패킷. 누군가 게이지가 다 닳았다면 밤이 끝났다는 패킷을 보냄
    /// </summary>
    private void GaugeLogic()
    {
        if (!_isGaugeStart) //게이지가 시작되지 않았다면
            return;
        
        //시간에 따른 킬러,생존자 게이지 자동감소 적용
        float killerMinusGauge = _killerGaugeDecreasePerSecond * Time.deltaTime;
        float survivorMinusGauge = _survivorGaugeDecreasePerSecond * Time.deltaTime;
        Managers.Player.DecreaseKillerGauge(killerMinusGauge);
        Managers.Player.IncreaseAllSurvivorGauge(survivorMinusGauge);
        
        //게이지 동기화 패킷 처리
        _gaugeSyncPacketTimer += Time.deltaTime;
        if (_gaugeSyncPacketTimer >= _gaugeSyncPacketInterval)
        {
            DSC_GaugeSync gaugeSyncPacket = new DSC_GaugeSync();
            //Manager.Player의 모든 플레이어id를 key로, value를 해당 플레이어의 현재 게이지로
            foreach (int playerId in Managers.Player._players.Keys)
            {
                gaugeSyncPacket.PlayerGauges.Add(playerId, Managers.Player.GetGauge(playerId));
            }
            //Manager.Player의 모든 플레이어id를 key로, value를 killer일경우 _killerGaugeDecreasePerSecond, 아닐경우 _survivorGaugeDecreasePerSecond로
            foreach (int playerId in Managers.Player._players.Keys)
            {
                gaugeSyncPacket.PlayerGaugeDecreasePerSecond.Add(playerId, Managers.Player.IsKiller(playerId) ? _killerGaugeDecreasePerSecond : _survivorGaugeDecreasePerSecond);
            }
            Managers.Player.Broadcast(gaugeSyncPacket);
            _gaugeSyncPacketTimer = 0;
        }
        
        
        int zeroGaugePlayerId = Managers.Player.CheckZeroGauge();
        if (zeroGaugePlayerId != -1) //누군가의 게이지가 다 닳아서 사망 처리.
        {
            Util.PrintLog($"player {zeroGaugePlayerId} is dead. gauge is 0");
            GaugeStop();
            DSC_PlayerDeath playerDeathPacket = new DSC_PlayerDeath();
            playerDeathPacket.PlayerId = zeroGaugePlayerId;
            playerDeathPacket.DeathCause = DeathCause.GaugeOver;
            Managers.Player.Broadcast(playerDeathPacket);
        }
        else if (_currentTimer <= 0) //누군가의 게이지가 닳기전 밤 종료. 마지막 킬러 사망 처리
        {
            Util.PrintLog($"night timer end. {Managers.Player.GetKillerId()} is dead. last killer");
            GaugeStop();
            DSC_PlayerDeath playerDeathPacket = new DSC_PlayerDeath();
            playerDeathPacket.PlayerId = Managers.Player.GetKillerId();
            playerDeathPacket.DeathCause = DeathCause.TimeOver;
            Managers.Player.Broadcast(playerDeathPacket);
        }
        
    }

    #endregion


    
}
