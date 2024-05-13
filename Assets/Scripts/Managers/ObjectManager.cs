using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인게임에 등장하는 모든 게임 오브젝트들을 관리하는 매니저(스폰, 제거, 이동, 상태변경 등)
/// </summary>
public class ObjectManager : MonoBehaviour
{
    #region Chest관련

    public List<GameObject> _chestList = new List<GameObject>(); //상자 리스트(인덱스는 상자의 고유 ID)
    public string _level1ChestPath = "Prefabs/Chest/Chest Standard";
    public string _level2ChestPath = "Prefabs/Chest/Chest Royal";
    public string _level3ChestPath = "Prefabs/Chest/Chest Mythical";
    GameObject _level1Chest; //레벨1 상자 프리팹
    GameObject _level2Chest; //레벨2 상자 프리팹
    GameObject _level3Chest; //레벨3 상자 프리팹
    Transform CastleChests; //CastleChests 상자들의 부모 오브젝트
    Transform TownChests; //TownChests 상자들의 부모 오브젝트
    Transform CaveChests; //CaveChests 상자들의 부모 오브젝트
    Transform BeachChests; //BeachChests 상자들의 부모 오브젝트
    Transform RoyalChests; //RoyalChests 상자들의 부모 오브젝트
    float _noPointProbability = 0.15f; //꽝 상자 확률(1,2레벨 상자만 꽝이 있음)
    
    public void SpawnAllChest()
    {
        _level1Chest = Managers.Resource.Load<GameObject>(_level1ChestPath);
        _level2Chest = Managers.Resource.Load<GameObject>(_level2ChestPath);
        _level3Chest = Managers.Resource.Load<GameObject>(_level3ChestPath);
        
        if(_level1Chest == null || _level2Chest == null || _level3Chest == null)
        {
            Util.PrintLog("상자 프리팹 로드 실패");
            return;
        }
        
        //CastleChests 상자 생성 (레벨1:90%, 레벨2:10%)
        CastleChests =  GameObject.Find("Map/Chests/CastleChests").transform;
        int CastleChestsCount = CastleChests.childCount; //CastleChests의 자식 개수
        int CastleLevl1ChestCount = (int)(CastleChestsCount * 0.9f); //레벨1 상자는 전체의 90%
        int CastleLevl2ChestCount = CastleChestsCount - CastleLevl1ChestCount; //레벨2 상자는 나머지
        Debug.Log("CastleChestsCount : " + CastleChestsCount);
        Debug.Log("CastleLevl1ChestCount : " + CastleLevl1ChestCount);
        Debug.Log("CastleLevl2ChestCount : " + CastleLevl2ChestCount);
        for (int i = 0; i < CastleChestsCount; i++)
        {
            //골고루 레벨1상자와 레벨2상자를 배치
            if (CastleLevl1ChestCount == 0)
            {
                CastleLevl2ChestCount--;
                GameObject chest = Instantiate(_level2Chest);
                Transform parent = CastleChests.GetChild(i);
                chest.transform.SetParent(parent, false);
                chest.transform.localPosition = Vector3.zero;
                chest.transform.localRotation = Quaternion.identity;
                parent.name = $"Lv2Chest_{i}";
                
                //_noPointProbability 확률로 꽝 상자 생성
                if (Random.Range(0f, 1f) < _noPointProbability)
                {
                    Chest chestScript = chest.GetComponent<Chest>();
                    if(chestScript == null)
                    {
                        chestScript = chest.AddComponent<Chest>();
                    }
                    chestScript.InitChest(i, 2, 0);
                }
                else
                {
                    Chest chestScript = chest.GetComponent<Chest>();
                    if(chestScript == null)
                    {
                        chestScript = chest.AddComponent<Chest>();
                    }
                    chestScript.InitChest(i, 2, 2);
                }
                
                //상자 리스트에 추가(인덱스는 상자의 고유 ID)
                _chestList.Add(chest);
            }
            else if(CastleLevl2ChestCount == 0)
            {
                CastleLevl1ChestCount--;
                GameObject chest = Instantiate(_level1Chest);
                Transform parent = CastleChests.GetChild(i);
                chest.transform.SetParent(parent, false);
                chest.transform.localPosition = Vector3.zero;
                chest.transform.localRotation = Quaternion.identity;
                parent.name = $"Lv1Chest_{i}";
                
                //_noPointProbability 확률로 꽝 상자 생성
                if (Random.Range(0f, 1f) < _noPointProbability)
                {
                    Chest chestScript = chest.GetComponent<Chest>();
                    if(chestScript == null)
                    {
                        chestScript = chest.AddComponent<Chest>();
                    }
                    chestScript.InitChest(i, 1, 0);
                }
                else
                {
                    Chest chestScript = chest.GetComponent<Chest>();
                    if(chestScript == null)
                    {
                        chestScript = chest.AddComponent<Chest>();
                    }
                    chestScript.InitChest(i, 1, 1);
                }
                
                //상자 리스트에 추가(인덱스는 상자의 고유 ID)
                _chestList.Add(chest);
            }
            else
            {
                int random = Random.Range(1, 11);
                if(random <= 9 && CastleLevl1ChestCount > 0)
                {
                    CastleLevl1ChestCount--;
                    GameObject chest = Instantiate(_level1Chest);
                    Transform parent = CastleChests.GetChild(i);
                    chest.transform.SetParent(parent, false);
                    chest.transform.localPosition = Vector3.zero;
                    chest.transform.localRotation = Quaternion.identity;
                    parent.name = $"Lv1Chest_{i}";
                    
                    //_noPointProbability 확률로 꽝 상자 생성
                    if (Random.Range(0f, 1f) < _noPointProbability)
                    {
                        Chest chestScript = chest.GetComponent<Chest>();
                        if(chestScript == null)
                        {
                            chestScript = chest.AddComponent<Chest>();
                        }
                        chestScript.InitChest(i, 1, 0);
                    }
                    else
                    {
                        Chest chestScript = chest.GetComponent<Chest>();
                        if(chestScript == null)
                        {
                            chestScript = chest.AddComponent<Chest>();
                        }
                        chestScript.InitChest(i, 1, 1);
                    }
                    
                    //상자 리스트에 추가(인덱스는 상자의 고유 ID)
                    _chestList.Add(chest);
                }
                else if(random == 10 && CastleLevl2ChestCount > 0)
                {
                    CastleLevl2ChestCount--;
                    GameObject chest = Instantiate(_level2Chest);
                    Transform parent = CastleChests.GetChild(i);
                    chest.transform.SetParent(parent, false);
                    chest.transform.localPosition = Vector3.zero;
                    chest.transform.localRotation = Quaternion.identity;
                    parent.name = $"Lv2Chest_{i}";
                    
                    //_noPointProbability 확률로 꽝 상자 생성
                    if (Random.Range(0f, 1f) < _noPointProbability)
                    {
                        Chest chestScript = chest.GetComponent<Chest>();
                        if(chestScript == null)
                        {
                            chestScript = chest.AddComponent<Chest>();
                        }
                        chestScript.InitChest(i, 2, 0);
                    }
                    else
                    {
                        Chest chestScript = chest.GetComponent<Chest>();
                        if(chestScript == null)
                        {
                            chestScript = chest.AddComponent<Chest>();
                        }
                        chestScript.InitChest(i, 2, 2);
                    }
                    
                    //상자 리스트에 추가(인덱스는 상자의 고유 ID)
                    _chestList.Add(chest);
                }
            }
            
        }
        
    }

    #endregion
    
}
    
