using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class KillerManager
{
    private string _jsonPath = Application.dataPath + "/Data/Killer/Killers.json";
    private Dictionary<int, KillerFactory> _killerFactories; //key: 킬러Id, value: 킬러 팩토리 객체
    public Dictionary<int, IKiller> _killers; //key: 킬러Id, value: 킬러 객체(킬러별 데이터 저장용. 전시품이라고 생각)
    private static string _killersDataJson; //json이 들어 있게 됨(파싱 해야 함)


    public void Init()
    {
        InitKillerFactories();
        LoadKillerData();
    }
    
    
    /// <summary>
    /// 킬러 팩토리 초기화
    /// </summary>
    public void InitKillerFactories()
    {
        _killerFactories = new Dictionary<int, KillerFactory>();
        _killers = new Dictionary<int, IKiller>();
        
        //아이템 팩토리 생성
        _killerFactories.Add(1, new TheHeartlessFactory());
    }
    
    /// <summary>
    /// 킬러 데이터를 로드후 파싱
    /// </summary>
    public void LoadKillerData()
    {
        if (File.Exists(_jsonPath))
        {
            string dataAsJson = File.ReadAllText(_jsonPath);
            _killersDataJson = dataAsJson;
        }
        else
        {
            Debug.LogError("Cannot find file at " + _jsonPath);
            return;
        }
        
        //파싱
        ParseKillerData();
    }
    
    /// <summary>
    /// json파일을 이미 받은 상태에서 킬러 데이터를 파싱
    /// </summary>
    private void ParseKillerData()
    {
        var killersData = JObject.Parse(_killersDataJson)["Killers"];
        _killers = new Dictionary<int, IKiller>();
        
        foreach (var killerData in killersData)
        {
            IKiller killer = null;
            string className = killerData["EnglishName"].ToString();
            
            Type type = Type.GetType(className);
            if (type != null)
            {
                killer = (IKiller)killerData.ToObject(type);
            }
            
            if (killer != null)
            {
                _killers.Add(killer.Id, killer);
            }
        }
    }

    /// <summary>
    /// 킬러 데이터 json 반환
    /// </summary>
    /// <returns>string형식의 json 킬러 데이터</returns>
    public string GetKillersDataJson()
    {
        return _killersDataJson;
    }
}

