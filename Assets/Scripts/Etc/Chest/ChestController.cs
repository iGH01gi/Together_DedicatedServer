using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public List<GameObject> _chestList = new List<GameObject>(); //상자 리스트(인덱스는 상자의 고유 ID)
    public string _level1ChestPath = "Prefabs/Chest/Chest Standard";
    public string _level2ChestPath = "Prefabs/Chest/Chest Royal";
    public string _level3ChestPath = "Prefabs/Chest/Chest Mythical";
    GameObject _level1Chest; //레벨1 상자 프리팹
    GameObject _level2Chest; //레벨2 상자 프리팹
    GameObject _level3Chest; //레벨3 상자 프리팹
    Transform _chestsParent; //상자들이 실제로 생성될 부모 오브젝트

    float _noPointProbability = 0.15f; //꽝 상자 확률(1,2레벨 상자만 꽝이 있음)

    int _level1Point = 1; //레벨1 상자 포인트
    int _level2Point = 2; //레벨2 상자 포인트
    int _level3Point = 3; //레벨3 상자 포인트

    int _level1Count = 0; //레벨1 상자 개수
    int _level2Count = 0; //레벨2 상자 개수
    int _level3Count = 0; //레벨3 상자 개수

    public void Init()
    {
        //상자 프리팹 로드
        _level1Chest = Managers.Resource.Load<GameObject>(_level1ChestPath);
        _level2Chest = Managers.Resource.Load<GameObject>(_level2ChestPath);
        _level3Chest = Managers.Resource.Load<GameObject>(_level3ChestPath);

        //상자들이 실제로 생성될 부모 오브젝트 초기화
        _chestsParent = GameObject.Find("Map/Chests").transform;

        //상자 포인트 초기화
        _level1Point = 1;
        _level2Point = 2;
        _level3Point = 3;

        //상자 개수 카운트 초기화
        _level1Count = 0;
        _level2Count = 0;
        _level3Count = 0;
    }

    /// <summary>
    /// 낮 시작될때 이거 하나만 부르면 알아서 패킷 보내는것까지 다 처리됨
    /// </summary>
    public void ChestSetAllInOne()
    {
        ClearAllChest();
        SpawnAllChest();
        DSC_NewChestsInfo newChestsInfo = MakeDscNewChestsInfo();
        Managers.Player.Broadcast(newChestsInfo);
    }

    /// <summary>
    /// 매번 낮이 되면 초기화를 위해 호출되는 함수
    /// </summary>
    public void ClearAllChest()
    {
        foreach (var chest in _chestList)
        {
            Destroy(chest);
        }

        _chestList.Clear();

        _level1Count = 0;
        _level2Count = 0;
        _level3Count = 0;
    }

    /// <summary>
    /// 매번 낮이 되면 상자를 생성하기 위해 호출되는 함수
    /// </summary>
    public void SpawnAllChest()
    {
        if (_level1Chest == null || _level2Chest == null || _level3Chest == null)
        {
            _level1Chest = Managers.Resource.Load<GameObject>(_level1ChestPath);
            _level2Chest = Managers.Resource.Load<GameObject>(_level2ChestPath);
            _level3Chest = Managers.Resource.Load<GameObject>(_level3ChestPath);
        }

        if (_chestsParent == null)
        {
            _chestsParent = GameObject.Find("Map/Chests").transform;
        }

        //상자 생성 (레벨1:60%, 레벨2:30%, 레벨3:10%)
        int chestsCount = _chestsParent.childCount; //_chestsParent 자식 개수
        for (int i = 0; i < chestsCount; i++)
        {
            GameObject chest = null;
            Transform parent = _chestsParent.GetChild(i);

            //60%확률로 레벨1상자 생성, 30%확률로 레벨2상자 생성, 10%확률로 레벨3상자 생성
            int random = Random.Range(1, 11);
            int level = 0;
            if (random <= 6)
            {
                level = 1;
                _level1Count++;
                chest = Instantiate(_level1Chest);
                parent.name = $"Lv1Chest_{i}";
            }
            else if (random <= 9)
            {
                level = 2;
                _level2Count++;
                chest = Instantiate(_level2Chest);
                parent.name = $"Lv2Chest_{i}";
            }
            else
            {
                level = 3;
                _level3Count++;
                chest = Instantiate(_level3Chest);
                parent.name = $"Lv3Chest_{i}";
            }

            //생성한 상자 부모 설정
            chest.transform.SetParent(parent, false);
            chest.transform.localPosition = Vector3.zero;
            chest.transform.localRotation = Quaternion.identity;

            //1,2레벨 상자의 경우 _noPointProbability 확률로 꽝 상자 생성
            if (level == 1 || level == 2)
            {
                if (Random.Range(0f, 1f) < _noPointProbability) //꽝(0포인트)
                {
                    Chest chestScript = chest.GetComponent<Chest>();
                    if (chestScript == null)
                    {
                        chestScript = chest.AddComponent<Chest>();
                    }

                    chestScript.InitChest(i, level, 0);
                }
                else
                {
                    Chest chestScript = chest.GetComponent<Chest>();
                    if (chestScript == null)
                    {
                        chestScript = chest.AddComponent<Chest>();
                    }

                    chestScript.InitChest(i, level, level == 1 ? _level1Point : _level2Point);
                }
            }
            else //3레벨 상자의 경우 꽝 없음
            {
                Chest chestScript = chest.GetComponent<Chest>();
                if (chestScript == null)
                {
                    chestScript = chest.AddComponent<Chest>();
                }

                chestScript.InitChest(i, level, _level3Point);
            }

            //상자 리스트에 추가(인덱스는 상자의 고유 ID)
            _chestList.Add(chest);
        }
    }

    //DSC_NewChestsInfo 패킷을 생성(map사용)
    public DSC_NewChestsInfo MakeDscNewChestsInfo()
    {
        DSC_NewChestsInfo newChestsInfo = new DSC_NewChestsInfo();
        foreach (var chest in _chestList)
        {
            Chest chestScript = chest.GetComponent<Chest>();
            if (chestScript == null)
            {
                Util.PrintLog("chestScript is null");
                return null;
            }

            ChestInfo chestInfo = new ChestInfo
            {
                ChestId = chestScript._chestId,
                ChestLevel = chestScript._chestLevel,
                ChestPoint = chestScript._point
            };
            newChestsInfo.ChestsInfo.Add(chestInfo.ChestId, chestInfo);
        }

        return newChestsInfo;
    }

    /// <summary>
    /// 클라가 상자 열기를 시도할때 호출
    /// </summary>
    /// <param name="tryChestOpenPacket"></param>
    public void ClientTryChestOpen(CDS_TryChestOpen tryChestOpenPacket)
    {
        int dediPlayerId = tryChestOpenPacket.MyDediplayerId;
        int chestId = tryChestOpenPacket.ChestId;

        //상자 열기를 atomic 하게 시도. 여는데 성공했으면 열었다는 패킷을 모든 클라이언트에게 보냄
        Chest chestScript = _chestList[chestId].GetComponent<Chest>();
        if (chestScript.TryOpenChestAtomic())
        {
            //상자 열기가 성공했으므로, 해당 플레이어에게 포인트 추가 처리
            Managers.Player._players[dediPlayerId].GetComponent<Player>()._totalPoint += chestScript._point;
            int totalPoint = Managers.Player._players[dediPlayerId].GetComponent<Player>()._totalPoint;

            //상자 열기 성공 패킷 브로드캐스트
            DSC_ChestOpenSuccess chestOpenSuccess = new DSC_ChestOpenSuccess()
            {
                ChestId = chestId,
                PlayerId = dediPlayerId,
                GetPoint = chestScript._point,
                TotalPoint = totalPoint
            };
            Managers.Player.Broadcast(chestOpenSuccess);
        }
    }
}