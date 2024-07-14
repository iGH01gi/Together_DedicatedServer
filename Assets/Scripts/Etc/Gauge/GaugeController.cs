using System.Collections.Generic;
using UnityEngine;

public class GaugeController : MonoBehaviour
{
    public float _gaugeMax = 180; //게이지 최대값
    public float _defaultSurvivorGaugeDecreasePerSecond = 1; //기본 생존자 초당 게이지 감소량
    public float _defaultKillerGaugeDecreasePerSecond = 2; //기본 킬러의 초당 게이지 감소량

    /// <summary>
    /// <para>모든 플레이어의 gauge를 본인의 _gaugeDecreasePerSecond만큼 감소시킴.</para>
    /// <para>만약 감소시킨 결과가 0보다 작다면 0으로 설정.</para>
    /// <para>time.deltatime적용된 상태</para>
    /// </summary>
    public void DecreaseAllGaugeAuto()
    {
        foreach (KeyValuePair<int, GameObject> a in Managers.Player._players)
        {
            DecreaseGauge(a.Key, a.Value.GetComponent<Player>()._gaugeDecreasePerSecond * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 감소시킴. 만약 감소시킨 결과가 0보다 작다면 0으로 설정
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="amount">얼만큼 감소시킬 것인가</param>
    /// <returns>감소된 게이지 결과. 존재하지 않는 플레이어라면 -1 반환</returns>
    public float DecreaseGauge(int playerId,float amount)
    {
        //존재하지 않는 플레이어라면 -1 반환
        if (!Managers.Player._players.ContainsKey(playerId))
        {
            return -1;
        }
        
        Managers.Player._players[playerId].GetComponent<Player>()._gauge -= amount;
        if (Managers.Player._players[playerId].GetComponent<Player>()._gauge < 0)
        {
            Managers.Player._players[playerId].GetComponent<Player>()._gauge = 0;
        }
        
        return Managers.Player._players[playerId].GetComponent<Player>()._gauge;
    }
    
    
    /// <summary>
    /// 킬러의 gauge를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">얼마나 증가시킬것인지</param>
    public void IncreaseKillerGauge(float amount)
    {
        Player killer = Managers.Player.GetKillerPlayerComponent();
        
        if(killer!=null)
            IncreaseGauge(killer.Info.PlayerId,amount);
    }
    
    /// <summary>
    /// 킬러를 제외한 모든 플레이어의 게이지를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">얼마나 증가시킬 것인가</param>
    public void IncreaseAllSurvivorGauge(float amount)
    {
        foreach (KeyValuePair<int, GameObject> a in Managers.Player._players)
        {
            if (!a.Value.GetComponent<Player>()._isKiller)
            {
                IncreaseGauge(a.Key, amount);
            }
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="amount">얼만큼 증가시킬 것인가</param>
    /// <returns>증가된 게이지 결과. 존재하지 않는 플레이어라면 -1 반환</returns>
    public float IncreaseGauge(int playerId,float amount)
    {
        //존재하지 않는 플레이어라면 -1 반환
        if (!Managers.Player._players.ContainsKey(playerId))
        {
            return -1;
        }
        
        Managers.Player._players[playerId].GetComponent<Player>()._gauge += amount;
        if (Managers.Player._players[playerId].GetComponent<Player>()._gauge > _gaugeMax)
        {
            Managers.Player._players[playerId].GetComponent<Player>()._gauge = _gaugeMax;
        }
        
        return Managers.Player._players[playerId].GetComponent<Player>()._gauge;
    }
    
    
    /// <summary>
    /// 모든 플레이어의 게이지를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">설정할 게이지 값</param>
    public void SetAllGauge(float amount)
    {
        foreach (KeyValuePair<int, GameObject> a in Managers.Player._players)
        {
            SetGauge(a.Key, amount);
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="amount">설정할 게이지 값</param>
    /// <returns></returns>
    public float SetGauge(int playerId,float amount)
    {
        //존재하지 않는 플레이어라면 -1 반환
        if (!Managers.Player._players.ContainsKey(playerId))
        {
            return -1;
        }
        
        //만약 amount가 0보다 작다면 0으로 설정
        if (amount < 0)
        {
            amount = 0;
        }
        //만약 amount가 _gaugeMax보다 크다면 _gaugeMax로 설정
        if (amount > _gaugeMax)
        {
            amount = _gaugeMax;
        }
        
        Managers.Player._players[playerId].GetComponent<Player>()._gauge = amount;
        
        return amount;
    }
    
    
    /// <summary>
    /// 특정 플레이어의 게이지를 반환함. 존재하지 않는 플레이어라면 -1 반환
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public float GetGauge(int playerId)
    {
        if (Managers.Player._players.ContainsKey(playerId))
        {
            return Managers.Player._players[playerId].GetComponent<Player>()._gauge;
        }

        return -1;
    }
    
    /// <summary>
    /// player들을 순회하면서 게이지가 0인 플레이어가 있는지 확인하고, 있다면 해당 playerId를 반환. 없다면 -1 반환
    /// </summary>
    /// <returns></returns>
    public int CheckZeroGauge()
    {
        foreach (KeyValuePair<int, GameObject> a in Managers.Player._players)
        {
            if (a.Value.GetComponent<Player>()._gauge <= 0)
            {
                return a.Key;
            }
        }

        return -1;
    }
    
    /// <summary>
    /// 특정 플레이어의 _gaugeDecreasePerSecond를 반환함
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <returns></returns>
    public float GetGaugeDecreasePerSecond(int playerId)
    {
        if (Managers.Player._players.ContainsKey(playerId))
        {
            return Managers.Player._players[playerId].GetComponent<Player>()._gaugeDecreasePerSecond;
        }

        return -1;
    }
    
    /// <summary>
    /// 특정 플레이어의 _gaugeDecreasePerSecond를 설정함
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="gaugeDecreasePerSecond">초당 게이지 감소량</param>
    public void SetGaugeDecreasePerSecond(int playerId,float gaugeDecreasePerSecond)
    {
        if (Managers.Player._players.ContainsKey(playerId))
        {
            Managers.Player._players[playerId].GetComponent<Player>()._gaugeDecreasePerSecond = gaugeDecreasePerSecond;
        }
    }

}