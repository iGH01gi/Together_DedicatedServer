using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class CleanseController : MonoBehaviour
{
    [FormerlySerializedAs("_cleanseObjectPath")] public string _cleanseParentPath = "Map/Cleanses"; //클린즈들이 모여있는 부모 게임오브젝트 경로
    public GameObject _cleanseParent; //클린즈들이 실제로 생성될 부모 오브젝트
    public List<GameObject> _cleansetList = new List<GameObject>(); //클린즈 리스트(인덱스는 클린즈 고유 ID)
    public float _cleansePoint = 20; //클린즈로 올라갈 게이지 정도
    public float _cleanseDurationSeconds = 3; //정화하는데 걸리는 시간(초 단위)
    public float _cleanseCoolTimeSeconds = 3; //클린즈를 사용한 후 쿨타임(초 단위)

    /// <summary>
    /// 클린즈 컴포넌트 붙이고 초기화
    /// </summary>
    public void Init()
    {
        //clenaseObjectPath 산하에 있는 자식들을 모두 가져와서 클린즈 리스트에 추가
        Transform cleansesParent = GameObject.Find(_cleanseParentPath).transform;
        foreach (Transform cleanse in cleansesParent)
        {
            _cleansetList.Add(cleanse.gameObject);
        }
        
        //cleanseList에 Cleanse.cs 컴포넌트 추가하고 초기화하기
        for (int i = 0; i < _cleansetList.Count; i++)
        {
            GameObject targetCleanse = _cleansetList[i];
            Cleanse cleanse = Util.GetOrAddComponent<Cleanse>(targetCleanse);
            
            TransformInfo transformInfo = new TransformInfo();
            transformInfo.Position = new PositionInfo();
            transformInfo.Position.PosX = targetCleanse.transform.position.x;
            transformInfo.Position.PosY = targetCleanse.transform.position.y;
            transformInfo.Position.PosZ = targetCleanse.transform.position.z;
            transformInfo.Rotation = new RotationInfo();
            transformInfo.Rotation.RotX = targetCleanse.transform.rotation.x;
            transformInfo.Rotation.RotY = targetCleanse.transform.rotation.y;
            transformInfo.Rotation.RotZ = targetCleanse.transform.rotation.z;
            transformInfo.Rotation.RotW = targetCleanse.transform.rotation.w;
            
            cleanse.InitCleanse(i, transformInfo, _cleansePoint, _cleanseDurationSeconds, _cleanseCoolTimeSeconds);
        }
        
        //_cleanseParentPath에 해당하는 게임오브젝트 active false해서 꺼놓기
        _cleanseParent = GameObject.Find(_cleanseParentPath);
        _cleanseParent.SetActive(false);
    }

    /// <summary>
    /// 밤이 되기전에 클린즈 정보 초기화
    /// </summary>
    public void ResetCleanses()
    {
        foreach (GameObject cleanse in _cleansetList)
        {
            Cleanse cleanseComponent = cleanse.GetComponent<Cleanse>();
            if (cleanseComponent != null)
            {
                cleanseComponent.ResetCleanse();
            }
        }
    }
    
    /// <summary>
    /// 최초 클린즈 설정 정보를 브로드캐스트 함
    /// </summary>
    public void SendAllCleanseInfo()
    {
        DSC_NewCleansesInfo newCleansesInfo = new DSC_NewCleansesInfo();
        
        foreach (GameObject cleanse in _cleansetList)
        {
            Cleanse cleanseComponent = cleanse.GetComponent<Cleanse>();
            if (cleanseComponent != null)
            {
                newCleansesInfo.Cleanses.Add(new CleanseInfo
                {
                    CleanseId = cleanseComponent._cleanseId,
                    CleanseTransform = cleanseComponent._transformInfo,
                    CleansePoint = cleanseComponent._cleansePoint,
                    CleanseDurationSeconds = cleanseComponent._cleanseDurationSeconds,
                    CleanseCoolTimeSeconds = cleanseComponent._cleanseCoolTimeSeconds
                });
            }
        }      
        
        Managers.Player.Broadcast(newCleansesInfo);
    }

    /// <summary>
    /// 클라가 클린즈를 사용 시도할때 호출
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="cleanseId">클린즈id</param>
    public void ClientTryCleanse(int playerId,int cleanseId)
    {
        //해당 플레이어가 킬러면 정화 불가능처리
        if (Managers.Player.IsKiller(playerId))
            return;
        
        //clenaseId를 가지고 있는 클린즈가 사용 가능한지 확인
        Cleanse cleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
        if (cleanse.IsAvailable())
        {
            cleanse.StartCleansing(playerId);
            
            //특정유저가 특정클린즈 사용 가능하다는 패킷 브로드캐스트
            DSC_GiveCleansePermission giveCleansePermission = new DSC_GiveCleansePermission();
            giveCleansePermission.PlayerId = playerId;
            giveCleansePermission.CleanseId = cleanseId;
            Managers.Player.Broadcast(giveCleansePermission);
        }
    }
    
    /// <summary>
    /// 클라가 클린즈 사용 중단할때 호출
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="cleanseId">클린즈id</param>
    public void ClientQuitCleanse(int playerId,int cleanseId)
    {
        Cleanse cleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
        cleanse.QuitCleansing();
        
        //특정유저가 특정클린즈 사용 중단했다는 패킷 브로드캐스트
        DSC_CleanseQuit cleanseQuit = new DSC_CleanseQuit();
        cleanseQuit.PlayerId = playerId;
        cleanseQuit.CleanseId = cleanseId;
        Managers.Player.Broadcast(cleanseQuit);
    }
    
    /// <summary>
    /// 클라가 클린즈 성공했을때 호출, 모두에게 성공 사실 브로드캐스트
    /// </summary>
    /// <param name="playerId">클린즈 성공한 플레이어id</param>
    /// <param name="cleanseId">정화된 클린즈id</param>
    public void ClientCleanseSuccess(int playerId ,int cleanseId)
    {
        Cleanse cleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
        cleanse.CleanseSuccess(playerId);
        
        //특정유저가 특정클린즈 성공했다는 패킷 브로드캐스트
        DSC_CleanseSuccess cleanseSuccess = new DSC_CleanseSuccess();
        cleanseSuccess.PlayerId = playerId;
        cleanseSuccess.CleanseId = cleanseId;
        cleanseSuccess.Gauge = Managers.Game._gaugeController.IncreaseGauge(playerId, cleanse._cleansePoint);
        Managers.Player.Broadcast(cleanseSuccess);
    }
}