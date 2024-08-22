using System;
using UnityEngine;

public class Dash : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }


    //이 아이템만의 속성
    public float DashDistance { get; set; }

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float dashDistance)
    {
        Init(itemId, playerId, englishName);
        DashDistance = dashDistance;
    }

    public void Use()
    {
        Util.PrintLog("Item Dash Use");
    }

    public void OnHold()
    {
        //예상 대시 경로, 또는 도착지점을 보여준다던지 하는 기능을 실행
    }

    public void OnHit()
    {
    }
}