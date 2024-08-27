using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;
using static UnityEditor.Progress;

public class Firework : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }


    //이 아이템만의 속성
    public float FlightHeight { get; set; }

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float flightHeight)
    {
        Init(itemId, playerId, englishName);
        FlightHeight = flightHeight;
    }

    public void Use(IMessage packet)
    {
        CDS_UseFireworkItem receivedPacket = packet as CDS_UseFireworkItem;

        DSC_UseFireworkItem useFireworkItemPacket = new DSC_UseFireworkItem();
        useFireworkItemPacket.PlayerId = PlayerID;
        useFireworkItemPacket.ItemId = ItemID;
        useFireworkItemPacket.FireworkStartingTransform = receivedPacket.FireworkStartingTransform;

        Player dediPlayer = Managers.Player._players[PlayerID].GetComponent<Player>();

        Vector3 playerPosition = dediPlayer.transform.position;
        Vector3 fireworkStartingPosition = new Vector3(receivedPacket.FireworkStartingTransform.Position.PosX, receivedPacket.FireworkStartingTransform.Position.PosY, receivedPacket.FireworkStartingTransform.Position.PosZ);

        //플레이어위치와 폭죽시작 위치가 일정범위 이내여야 함(핵 러프하게 검사)
        if (Vector3.Distance(playerPosition, fireworkStartingPosition) > 5f)
        {
            //핵의심. 무시
            Util.PrintLog($"The distance between the player and the firework start position is too far. Suspected cheat.");
            return;
        }

        //아이템 사용 패킷 브로드캐스트
        Managers.Player.Broadcast(useFireworkItemPacket);

        Util.PrintLog("Item Firework Use");

        //폭죽 오브젝트 파괴
        Destroy(gameObject);
    }

    public void OnHold()
    {

    }

    public void OnHit()
    {

    }
}