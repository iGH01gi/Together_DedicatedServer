using UnityEngine;

/// <summary>
/// 인게임에 등장하는 모든 게임 오브젝트들을 관리하는 매니저(스폰, 제거, 이동, 상태변경 등)
/// </summary>
public class ObjectManager
{
    public string _tempPlayerPrefabPath = "Player/Player";
    
    /// <summary>
    /// 데디서버 플레이어를 실제로 생성하는 함수
    /// </summary>
    /// <param name="dediPlayer">갖고 있어야할 플레이어 정보</param>
    /// <returns></returns>
    public GameObject SpawnPlayer(Player dediPlayer)
    {
        GameObject obj =Managers.Resource.Instantiate(_tempPlayerPrefabPath);
        Player dediPlayerComponent = obj.AddComponent<Player>();
        
        dediPlayerComponent.CopyFrom(dediPlayer);

        return obj;
    }

    /// <summary>
    /// 플레이어를 게임상에서 제거하는 함수 (Destroy처리)
    /// </summary>
    /// <param name="dediPlayer">Destroy할 플레이어 오브젝트</param>
    public void DespawnPlayer(GameObject dediPlayerObj)
    {
        Managers.Resource.Destroy(dediPlayerObj);
    }
}