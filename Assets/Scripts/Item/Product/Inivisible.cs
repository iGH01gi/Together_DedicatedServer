using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Invisible : MonoBehaviour, IItem
{
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    public float InvisibleSeconds { get; set; }

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float invisibleSeconds)
    {
        Init(itemId, playerId, englishName);
        InvisibleSeconds = invisibleSeconds;
    }

    public void Use(IMessage packet)
    {
        //투명 아이템 시작 패킷 브로드캐스트
        DSC_UseInvisibleItem useInvisibleItemPacket = new DSC_UseInvisibleItem()
        {
            PlayerId = PlayerID,
            ItemId = ItemID,
        };
        Managers.Player.Broadcast(useInvisibleItemPacket);

        Util.PrintLog($"Player{PlayerID} Use Item Invisible");
    }

    public void OnHit()
    {

    }
}