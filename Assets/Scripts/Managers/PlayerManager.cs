using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    object _lock = new object();
    Dictionary<int,GameObject> _players = new Dictionary<int, GameObject>(); //key: 데디서버의 playerId, value: 플레이어 정보
    public string _tempPlayerPrefabPath = "Player/Player";
    
    /// <summary>
    /// 플레이어를 스폰하고 정보를 저장함. 모든이에게 추가된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="playerId">독자적인 id</param>
    public void AddPlayer(ClientSession session,int roomId,string name)
    {
        DSC_AllowEnterGame allowEnterPacket = new DSC_AllowEnterGame();
        DSC_InformNewFaceInDedicatedServer informNewFaceInDedicatedServerPacket = new DSC_InformNewFaceInDedicatedServer();
        
        lock (_lock)
        {
            if (!_players.ContainsKey(session.SessionId))//이미 생성돼있는 플레이어가 아닐때
            {
                //플레이어 생성
                Player newPlayer = new Player();
                newPlayer.Info.PlayerId = session.SessionId;
                newPlayer.Info.Name = name;
                newPlayer.Info.Transform = null;
                newPlayer.RoomId = roomId;
                newPlayer.Session = session;
   
                //플레이어 관리목록에 추가
                GameObject newPlayerObj= SpawnPlayer(newPlayer);
                newPlayerObj.name = $"Player_{newPlayer.Info.PlayerId}"; //플레이어 오브젝트 이름을 "Player_플레이어id"로 설정
                _players.Add(newPlayer.Info.PlayerId, newPlayerObj);
                
                //본인한테 입장 허용 패킷 보냄
                allowEnterPacket.MyDedicatedPlayerId = newPlayer.Info.PlayerId;
                _players.Values.ToList().ForEach(player => allowEnterPacket.Players.Add(player.GetComponent<Player>().Info));
                session.Send(allowEnterPacket);
                
                //다른 클라이언트에게 새로운 플레이어가 게임에 접속했음을 전송함
                informNewFaceInDedicatedServerPacket.NewPlayer = newPlayer.Info;
                foreach (KeyValuePair<int,GameObject> a in _players)
                {
                    GameObject existingPlayerObj = a.Value;
                    if (existingPlayerObj != null && existingPlayerObj != newPlayerObj)
                    {
                        existingPlayerObj.GetComponent<Player>().Session.Send(informNewFaceInDedicatedServerPacket);
                    }
                }
            }

        }
    }
    
    /// <summary>
    /// 플레이어id에 해당하는 게임오브젝트를 destroy하고 플레이어 매니저에서 플레이어를 제거
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool Remove(int playerId)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(playerId, out GameObject playerObj))
            {
                if(playerObj!=null)
                    DespawnPlayer(playerObj);
                
                return _players.Remove(playerId);
            }

            return false;
        }
    }
    

    /// <summary>
    /// 플레이어 destroy하고 플레이어 매니저에서 플레이어를 제거하고, 모든 클라이언트에게 제거된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="playerId">제거해야할 플레이어id</param>
    public void LeaveGame(int playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            if (Remove(playerId))
            {
                DSC_InformLeaveDedicatedServer informLeaveDedicatedServerPacket = new DSC_InformLeaveDedicatedServer();
                informLeaveDedicatedServerPacket.LeavePlayerId = playerId;
                foreach (GameObject playerObj in _players.Values)
                {
                    if (playerObj != null)
                    {
                        ClientSession session = playerObj.GetComponent<Player>().Session;
                        session.Send(informLeaveDedicatedServerPacket); 
                    }
                }
            }
        }
    }
    
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
