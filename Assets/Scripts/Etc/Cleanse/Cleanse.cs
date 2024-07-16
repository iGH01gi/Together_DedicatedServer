using System;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Cleanse : MonoBehaviour
{
    public int _cleanseId = 0; // 클린즈의 고유 ID (0부터 시작)
    public TransformInfo _transformInfo = new TransformInfo(); // 클린즈의 위치 정보
    public float _cleansePoint = 0; //클린즈로 올라갈 게이지 정도
    public float _cleanseDurationSeconds = 0; //정화하는데 걸리는 시간
    public float _cleanseCoolTimeSeconds = 0; //클린즈를 사용한 후 쿨타임
    private bool _isCoolTime = false; // 현재 쿨타임중인지 여부
    private bool _isCleansing = false; // 현재 누군가가 클린징 중인지 여부
    
    private int _lastCleanserId = -1; //마지막으로 클린징 성공한 플레이어id. (초기값 -1)
    private DateTime _lastCleanseTime = DateTime.MinValue; //마지막으로 클린징 성공한 시간.(utc기준) (초기값 DateTime.MinValue)
    
    private float _coolTimeTimer = 0f; //쿨타임을 계산하기 위한 타이머

    private void Update()
    {
        if (_isCoolTime)
        {
            //쿨타임중이면 시간이 흐르고, 쿨타임이 끝나면 사용 가능 상태로 변경
            _coolTimeTimer += Time.deltaTime;
            if (_coolTimeTimer >= _cleanseCoolTimeSeconds)
            {
                _isCoolTime = false;
                _isCleansing = false;
                _coolTimeTimer = 0f;
                
                //이 클린즈의 쿨타임이 끝났다는 것을 모두에게 알림
                DSC_CleanseCooltimeFinish cleanseCooltimeFinish = new DSC_CleanseCooltimeFinish();
                cleanseCooltimeFinish.CleanseId = _cleanseId;
                Managers.Player.Broadcast(cleanseCooltimeFinish);
            }
        }
    }

    /// <summary>
    /// 클린즈 정보 초기화
    /// </summary>
    /// <param name="cleanseId">클린즈id</param>
    /// <param name="transformInfo">위치,회전 정보</param>
    /// <param name="point">클린즈로 올라갈 게이지 정도</param>
    /// <param name="durationSeconds">정화하는데 걸리는 시간</param>
    /// <param name="coolTimeSeconds">클린즈를 사용한 후 쿨타임</param>
    public void InitCleanse(int cleanseId, TransformInfo transformInfo, float point, float durationSeconds, float coolTimeSeconds)
    {
        _cleanseId = cleanseId;
        _transformInfo = transformInfo;
        _cleansePoint = point;
        _cleanseDurationSeconds = durationSeconds;
        _cleanseCoolTimeSeconds = coolTimeSeconds;
        _isCoolTime = false;
        _isCleansing = false;
        _lastCleanserId = -1;
        _lastCleanseTime = DateTime.MinValue;
        _coolTimeTimer = 0f;
    }

    /// <summary>
    /// 밤이 시작되기전에 클린즈 리셋
    /// </summary>
    public void ResetCleanse()
    {
        _isCoolTime = false;
        _isCleansing = false;
        _lastCleanserId = -1;
        _lastCleanseTime = DateTime.MinValue;
        _coolTimeTimer = 0f;
    }
    
    /// <summary>
    /// 현재 이 클린즈가 사용 가능한지 여부(쿨타임, 사용중 고려)
    /// </summary>
    /// <returns></returns>
    public bool IsAvailable()
    {
        return !_isCoolTime && !_isCleansing;
    }
    
    /// <summary>
    /// 누군가가 클린징 시작했을때 처리
    /// </summary>
    /// <param name="cleanserId"></param>
    public void StartCleansing(int cleanserId)
    {
        _isCleansing = true;
    }
    
    /// <summary>
    /// 누군가가 클린징 시도하다가 취소했을때 처리
    /// </summary>
    public void QuitCleansing()
    {
        _isCleansing = false;
    }
    
    /// <summary>
    /// 누군가가 클린징 성공했을때 처리
    /// </summary>
    /// <param name="playerId">클린징 성공한 플레이어id</param>
    public void CleanseSuccess(int playerId)
    {
        _isCoolTime = true;
        _isCleansing = false;
        _lastCleanserId = playerId;
        _lastCleanseTime = DateTime.UtcNow;
    }
    
}