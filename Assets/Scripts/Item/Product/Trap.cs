using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Trap : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    public float TrapDuration { get; set; }
    public float TrapRadius { get; set; }
    public float StunDuration { get; set; }

    public bool _isAlreadyTrapped = false; //이미 누군가가 트랩에 걸렸는지 여부


    private float _setTrapSeconds = 1f; //트랩 설치 시간

    public string _trapId; //덫 고유 아이디

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float trapDuration, float trapRadius, float stunDuration)
    {
        Init(itemId, playerId, englishName);
        TrapDuration = trapDuration;
        TrapRadius = trapRadius;
        StunDuration = stunDuration;
    }

    public void Use(IMessage packet)
    {
        CDS_UseTrapItem recvPacket = packet as CDS_UseTrapItem;

        //현재 플레이어의 위치와 recvPacket의 위치를 비교하여 일정범위 밖이라면  핵 의심
        Player dediPlayer = Managers.Player._players[PlayerID].GetComponent<Player>();
        Vector3 playerPosition = dediPlayer.transform.position;
        Vector3 trapPosition = new Vector3(recvPacket.TrapTransform.Position.PosX, recvPacket.TrapTransform.Position.PosY, recvPacket.TrapTransform.Position.PosZ);
        if (Vector3.Distance(playerPosition, trapPosition) > 3f)
        {
            //핵의심. 무시
            Util.PrintLog($"The distance between the player and the Trap position is too far. Suspected cheat.");
            Destroy(gameObject);
            return;
        }

        _trapId = recvPacket.TrapId;
        //덫 이름 변경
        gameObject.name = "Trap_" + _trapId;

        //트랩 설치 정보 브로드캐스트
        DSC_UseTrapItem useTrapItemPacket = new DSC_UseTrapItem()
        {
            PlayerId = PlayerID,
            ItemId = ItemID,
            TrapTransform = recvPacket.TrapTransform,
            TrapId = _trapId
        };
        Managers.Player.Broadcast(useTrapItemPacket);

        StartCoroutine(SetTrapDuringSeconds(_setTrapSeconds, recvPacket));

        

        Debug.Log("Item Trap Use");
    }

    IEnumerator SetTrapDuringSeconds(float seconds, CDS_UseTrapItem recvPacket)
    {
        yield return new WaitForSeconds(seconds);

        //패킷의 transform 위치로 gameObject의 위치를 변경
        transform.position = new Vector3(recvPacket.TrapTransform.Position.PosX, recvPacket.TrapTransform.Position.PosY, recvPacket.TrapTransform.Position.PosZ);
        transform.rotation = new Quaternion(recvPacket.TrapTransform.Rotation.RotX, recvPacket.TrapTransform.Rotation.RotY, recvPacket.TrapTransform.Rotation.RotZ, recvPacket.TrapTransform.Rotation.RotW);

        //MeshRenderer 활성화
        gameObject.GetComponent<MeshRenderer>().enabled = true;

        //SphereCollider 활성화
        gameObject.GetComponent<SphereCollider>().enabled = true;

        //TrapDuration 이후 덫 사라짐
        StartCoroutine(DestroyAfterSeconds(TrapDuration));
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "SurvivorTrigger" || other.gameObject.tag == "KillerTrigger")
        {
            //트랩이 설치된 위치에서 트리거가 발생하면 트랩이 터지게 함
            OnHit();
        }
    }

    /// <summary>
    /// 덫이 사람한테 닿았을때 처리(사라지기 관련)
    /// </summary>
    public void OnHit()
    {
        StopCoroutine("DestroyAfterSeconds");
        StartCoroutine(DestroyAfterSeconds(StunDuration));
    }
}