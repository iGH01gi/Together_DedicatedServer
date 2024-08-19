
using System;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 시간과 관련된 기능을 관리하는 클래스 
/// </summary>
public class TimeManager : MonoBehaviour
{
    private int _daySeconds = 50; //낮 시간(초)
    private int _nightSeconds = 5; //밤 시간(초)
    private float _currentTimer = 0f; //현재 시간(초)
    
    public bool _isDay = false;
    public bool _isNight = false;
    
    private float _timerSyncPacketTimer = 0f; //타이머 동기화 패킷을 위한 타이머
    private float _timerSyncPacketInterval = 5f; //타이머 동기화 패킷을 보내는 간격(초)
    private int _dayNightInterval = 3000; //낮, 밤 사이 전환 간격(ms)
    
    private bool _isGaugeStart = false; //게이지 시작 여부
    private float _gaugeSyncPacketTimer = 0f; //게이지 동기화 패킷을 위한 타이머
    private float _gaugeSyncPacketInterval = 5f; //게이지 동기화 패킷을 보내는 간격(초)

    

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
        
        Managers.Player.ResetPlayerOnDayStart(); //낮이 시작되면 플레이어들의 상태를 초기화
        
        DSC_DayTimerStart dayTimerStartPacket = new DSC_DayTimerStart();
        dayTimerStartPacket.DaySeconds = _daySeconds;
        Managers.Player.Broadcast(dayTimerStartPacket);
        
        //상자 생성 및 정보 전송
        Managers.Object._chestController.ChestSetAllInOne();
        
        //클린즈 끄기
        Managers.Object._cleanseController._cleanseParent.SetActive(false);
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
        
        //게이지 시작 + 게이지에 필요한 정보 세팅
        GaugeStart();
        
        //클린즈 켜기
        Managers.Object._cleanseController._cleanseParent.SetActive(true);
        //클린즈 리셋
        Managers.Object._cleanseController.ResetCleanses();
        
        DSC_NightTimerStart nightTimerStartPacket = new DSC_NightTimerStart();
        nightTimerStartPacket.NightSeconds = _nightSeconds;
        nightTimerStartPacket.GaugeMax = Managers.Game._gaugeController._gaugeMax;
        //Manager.Player의 모든 플레이어id를 key로, value를 _gaugeDecreasePerSecond로
        foreach (int playerId in Managers.Player._players.Keys)
        {
            nightTimerStartPacket.PlayerGaugeDecreasePerSecond.Add(playerId, Managers.Game._gaugeController.GetGaugeDecreasePerSecond(playerId));
        }
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
            if (_currentTimer <= 0) //낮이 끝났다는 패킷을 보내고 타이머를 멈춤 + n초후 밤 타이머 시작과 해당 패킷 전송 + 킬러 선정하고 알림
            {
                Util.PrintLog($"day timer end");
                DSC_DayTimerEnd dayTimerEndPacket = new DSC_DayTimerEnd();
                
                //킬러 초기화 + 선정 + 어떤 킬러인지 결정
                Managers.Player.ClearKiller();
                Tuple<int,int> killerIdAndType = Managers.Player.RandomSelectKiller();
                dayTimerEndPacket.KillerPlayerId = killerIdAndType.Item1;
                dayTimerEndPacket.KillerType = killerIdAndType.Item2;
                
                Managers.Player.Broadcast(dayTimerEndPacket);
                TimerStop();
                
                //상자 삭제
                Managers.Object._chestController.ClearAllChest();
                
                //클린즈 켜기
                Managers.Object._cleanseController._cleanseParent.SetActive(true);

                //클린즈 정보 보냄
                Managers.Object._cleanseController.SendAllCleanseInfo();

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
            if (_currentTimer <= 0) //밤이 끝났다는 패킷을 보내고 타이머를 멈춤 + n초후 낮 타이머 시작과 해당 패킷 전송 + 킬러가 죽었다는 정보도 보냄 + 게이지 멈춤
            {
                TimerStop();
                GaugeStop();
                
                Util.PrintLog($"night timer end. {Managers.Player.GetKillerId()} is dead. last killer");
                DSC_NightTimerEnd nightTimerEndPacket = new DSC_NightTimerEnd();
                nightTimerEndPacket.DeathCause = DeathCause.TimeOver;
                nightTimerEndPacket.DeathPlayerId = Managers.Player.GetKillerId();
                nightTimerEndPacket.KillerPlayerId = Managers.Player.GetKillerId();
            
                //TODO: 사망해서 오브젝트,고스트 삭제 처리등등... 추가로 여기다가 해줘야 함
            
                Managers.Player.Broadcast(nightTimerEndPacket);
            
                //일정 시간 후에 낮 시작
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
    /// 밤 게이지 시작 + 게이지에 필요한 정보 세팅
    /// </summary>
    public void GaugeStart()
    {
        _gaugeSyncPacketTimer = 0;
        _isGaugeStart = true;
        
        //모든 플레이어의 게이지를 최대로 설정
        Managers.Game._gaugeController.SetAllGauge(Managers.Game._gaugeController._gaugeMax);
        
        //킬러가아닌 플레이어들에게 기본 생존차 초당 게이지 감소량 적용. 킬러에겐 기본 킬러 초당 게이지 감소량 적용
        foreach (int playerId in Managers.Player._players.Keys)
        {
            Managers.Game._gaugeController.SetGaugeDecreasePerSecond(playerId, Managers.Player.IsKiller(playerId) ? Managers.Game._gaugeController._defaultKillerGaugeDecreasePerSecond : Managers.Game._gaugeController._defaultSurvivorGaugeDecreasePerSecond);
        }
        
        //TODO: 특정 플레이어의 초기 초당 게이지 감소량을 조절해야 한다면 여기서 조절하기!
        
    }
    
    /// <summary>
    /// 밤 게이지 종료
    /// </summary>
    public void GaugeStop()
    {
        _gaugeSyncPacketTimer = 0;
        _isGaugeStart = false;
        
        //모든 플레이어의 게이지를 0으로 설정
        Managers.Game._gaugeController.SetAllGauge(0);
    }
    
    /// <summary>
    /// 밤 게이지 로직. 5초 간격으로 동기화패킷. 누군가 게이지가 다 닳았다면 밤이 끝났다는 패킷을 보냄
    /// </summary>
    private void GaugeLogic()
    {
        if (!_isGaugeStart) //게이지가 시작되지 않았다면
            return;
        
        //시간에 따른 킬러,생존자 게이지 자동감소 적용
        Managers.Game._gaugeController.DecreaseAllGaugeAuto();
        
        //게이지 동기화 패킷 처리
        _gaugeSyncPacketTimer += Time.deltaTime;
        if (_gaugeSyncPacketTimer >= _gaugeSyncPacketInterval)
        {
            DSC_GaugeSync gaugeSyncPacket = new DSC_GaugeSync();
            //Manager.Player의 모든 플레이어id를 key로, value를 해당 플레이어의 현재 게이지로
            foreach (int playerId in Managers.Player._players.Keys)
            {
                gaugeSyncPacket.PlayerGauges.Add(playerId, Managers.Game._gaugeController.GetGauge(playerId));
            }
            //Manager.Player의 모든 플레이어id를 key로, value를 _gaugeDecreasePerSecond로
            foreach (int playerId in Managers.Player._players.Keys)
            {
                gaugeSyncPacket.PlayerGaugeDecreasePerSecond.Add(playerId, Managers.Game._gaugeController.GetGaugeDecreasePerSecond(playerId));
            }
            Managers.Player.Broadcast(gaugeSyncPacket);
            _gaugeSyncPacketTimer = 0;
        }
        
        
        int zeroGaugePlayerId = Managers.Game._gaugeController.CheckZeroGauge();
        if (zeroGaugePlayerId != -1) //누군가의 게이지가 다 닳아서 사망 처리. 밤 끝났다는 패킷에 넣어서 보냄
        {
            Util.PrintLog($"player {zeroGaugePlayerId} is dead. gauge is 0");
            GaugeStop();
            TimerStop();
            
            DSC_NightTimerEnd nightTimerEndPacket = new DSC_NightTimerEnd();
            nightTimerEndPacket.DeathCause = DeathCause.GaugeOver;
            nightTimerEndPacket.DeathPlayerId = zeroGaugePlayerId;
            nightTimerEndPacket.KillerPlayerId = Managers.Player.GetKillerId();
            
            //TODO: 사망해서 오브젝트,고스트 삭제 처리등등... 추가로 여기다가 해줘야 함
            
            Managers.Player.Broadcast(nightTimerEndPacket);
            
            //일정 시간 후에 낮 시작
            JobTimer.Instance.Push(() =>
            {
                DayTimerStart();
            }, _dayNightInterval);
        }
        
    }

    #endregion
    
}
