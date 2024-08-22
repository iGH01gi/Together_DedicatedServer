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
        //대시 시간(애니메이션 재생 시간) (무적시간이기도 함)
        float dashTime = 0.24f;

        //고스트 따라가기 기능 멈춤(대시 사용을 위해서)
        GameObject playerObjet = Managers.Player._players[PlayerID];
        Player player = playerObjet.GetComponent<Player>();
        player._isFollowGhostOn = false;

        //DashDistance 만큼 앞으로 대시
        gameObject.transform.position += gameObject.transform.forward * DashDistance;
    }

    public void OnHold()
    {
        //예상 대시 경로, 또는 도착지점을 보여준다던지 하는 기능을 실행
    }

    public void OnHit()
    {
    }
}