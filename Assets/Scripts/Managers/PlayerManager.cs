using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    object _lock = new object();
    Dictionary<int,GameObject> _players = new Dictionary<int, GameObject>(); //key: 데디서버의 playerId, value: 플레이어 오브젝트
    Dictionary<int,GameObject> _ghosts = new Dictionary<int, GameObject>()   ; //key: 데디서버의 playerId, value: 고스트 오브젝트
    public string _tempPlayerPrefabPath = "Player/Player";
    public string _tempTargetGhost = "Player/TargetGhost";
    public Transform _spawnPointCenter;
    private bool[] _possibleSpawnPoint; //8개의 스폰포인트 중에 가능한 스폰포인트를 체크하는 배열

    public void Init()
    {
        _spawnPointCenter = GameObject.Find("Map/SpawnPoint").transform;
        _possibleSpawnPoint = new bool[8]{true,true,true,true,true,true,true,true};
    }
    
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
                //플레이어 정보 생성 + 실체 생성 + 관리목록에 추가
                GameObject newPlayerObj= SpawnPlayer(session, roomId, name);
                _players.Add(session.SessionId, newPlayerObj);
                
                //고스트 실체 생성 + 관리목록에 추가
                GameObject newGhost = SpawnGhost(session.SessionId);
                _ghosts.Add(session.SessionId,newGhost); //고스트목록에 추가

                
                //본인한테 입장 허용 패킷 보냄
                Player newPlayer = newPlayerObj.GetComponent<Player>();
                allowEnterPacket.MyDedicatedPlayerId = newPlayer.Info.PlayerId;
                _players.Values.ToList().ForEach(player => allowEnterPacket.Players.Add(player.GetComponent<Player>().Info));
                Dictionary<int,TransformInfo> playerTransforms = new Dictionary<int, TransformInfo>();
                foreach (KeyValuePair<int,GameObject> a in _players)
                {
                    GameObject playerObj = a.Value;
                    if (playerObj != null)
                    {
                        Player player = playerObj.GetComponent<Player>();
                        TransformInfo transformInfo = new TransformInfo();
                        transformInfo.PosX = playerObj.transform.position.x;
                        transformInfo.PosY = playerObj.transform.position.y;
                        transformInfo.PosZ = playerObj.transform.position.z;
                        transformInfo.RotX = playerObj.transform.rotation.x;
                        transformInfo.RotY = playerObj.transform.rotation.y;
                        transformInfo.RotZ = playerObj.transform.rotation.z;
                        transformInfo.RotW = playerObj.transform.rotation.w;
                        playerTransforms.Add(player.Info.PlayerId,transformInfo);
                    }
                }
                allowEnterPacket.PlayerTransforms.Add(playerTransforms);
                session.Send(allowEnterPacket);
                
                //다른 클라이언트에게 새로운 플레이어가 게임에 접속했음을 전송함
                informNewFaceInDedicatedServerPacket.NewPlayer = newPlayer.Info;
                TransformInfo newPlayerTransformInfo = new TransformInfo();
                newPlayerTransformInfo.PosX = newPlayerObj.transform.position.x;
                newPlayerTransformInfo.PosY = newPlayerObj.transform.position.y;
                newPlayerTransformInfo.PosZ = newPlayerObj.transform.position.z;
                newPlayerTransformInfo.RotX = newPlayerObj.transform.rotation.x;
                newPlayerTransformInfo.RotY = newPlayerObj.transform.rotation.y;
                newPlayerTransformInfo.RotZ = newPlayerObj.transform.rotation.z;
                newPlayerTransformInfo.RotW = newPlayerObj.transform.rotation.w;
                informNewFaceInDedicatedServerPacket.SpawnTransform = newPlayerTransformInfo;
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
    /// 플레이어id에 해당하는 게임오브젝트와 ghost를 destroy하고 플레이어 매니저에서 플레이어,고스트를 제거
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public bool Remove(int playerId)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(playerId, out GameObject playerObj) && _ghosts.TryGetValue(playerId, out GameObject ghostObj))
            {
                if(playerObj!=null)
                    DespawnPlayer(playerObj);
                if(ghostObj!=null)
                    DespawnGhost(ghostObj);
                
                return _players.Remove(playerId);
            }

            return false;
        }
    }
    /// <summary>
    /// 플레이어를 게임상에서 제거하는 함수 (Destroy처리)
    /// </summary>
    /// <param name="dediPlayer">Destroy할 플레이어 오브젝트</param>
    public void DespawnPlayer(GameObject dediPlayerObj)
    {
        Managers.Resource.Destroy(dediPlayerObj);
    }
    public void DespawnGhost(GameObject ghostObj)
    {
        Managers.Resource.Destroy(ghostObj);
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
    /// 데디서버 플레이어의 정보와 실체를 실제로 생성하는 함수(관리목록 추가는 안함)
    /// </summary>
    /// <param name="dediPlayer">갖고 있어야할 플레이어 정보</param>
    /// <returns></returns>
    public GameObject SpawnPlayer(ClientSession session,int roomId,string name)
    {
        Player newPlayer = new Player();
        newPlayer.Info.PlayerId = session.SessionId;
        newPlayer.Info.Name = name;
        newPlayer.RoomId = roomId;
        newPlayer.Session = session;
        
        GameObject obj =Managers.Resource.Instantiate(_tempPlayerPrefabPath);
        obj.name = $"Player_{newPlayer.Info.PlayerId}"; //플레이어 오브젝트 이름을 "Player_플레이어id"로 설정
        
        //가능한 spawnPoint들 중에 랜덤으로 1개 선택해서 위치 설정. 그리고 해당 spawnPoint는 사용불가로 설정
        List<int> trueIndices = new List<int>();
        
        for (int i = 0; i < _possibleSpawnPoint.Length; i++)
        {
            if (_possibleSpawnPoint[i])
            {
                trueIndices.Add(i);
            }
        }

        if (trueIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, trueIndices.Count); 
            //_spawnPointCenter의 randomIndex번째 자식의 transform을 가져옴
            Transform spawnPoint = _spawnPointCenter.GetChild(trueIndices[randomIndex]);
            //obj의 transform을 spawnPoint와 동일하게 설정
            obj.transform.position = spawnPoint.position;
            obj.transform.rotation = spawnPoint.rotation;
            
            Util.PrintLog($"{newPlayer.Info.PlayerId}번 플레이어는 {randomIndex}번째 스폰포인트에 스폰되었습니다");
        }
        else
        {
            Util.PrintLog("가능한 스폰포인트가 없습니다");
        }
        
        //플레이어 컴포넌트 추가
        Player dediPlayerComponent = obj.AddComponent<Player>();
        dediPlayerComponent.CopyFrom(newPlayer);

        return obj;
    }
    /// <summary>
    /// 플레이어id(=세션id)에 해당하는 고스트를 생성하는 함수. 위치도 초기 위치로 설정
    /// </summary>
    /// <param name="sessionId">대응되는 플레이어의 id</param>
    /// <returns></returns>
    public GameObject SpawnGhost(int playerId)
    {
        GameObject newGhost = Managers.Resource.Instantiate(_tempTargetGhost);
        newGhost.name = $"Ghost_{playerId}"; //고스트 오브젝트 이름을 "Ghost_플레이어id"로 설정
        //위치는 대응되는 플레이어의 초기 위치로 설정
        
        _players.TryGetValue(playerId, out GameObject playerObj);
        if (playerObj != null)
        {
            newGhost.transform.position = playerObj.transform.position;
            newGhost.transform.rotation = playerObj.transform.rotation;
        }

        return newGhost;
    }


    /// <summary>
    /// 팔로워가 따라갈 targetGhost를 설정함. (추측항법)
    /// 받은 위치에다가 키입력에따른 속도벡터만큼 이동한곳이 targetPos의 시작위치
    /// 그리고 속도벡터만큼 계속 targetPos를 이동시킴
    /// 팔로워는 targetPos를 계속 따라가게됨
    /// </summary>
    /// <param name="movePacket"></param>
    public void SetTargetGhost(int playerId,CDS_Move movePacket)
    {
        if (_ghosts.TryGetValue(playerId, out GameObject ghostObj))
        {
            float posX = movePacket.Transform.PosX;
            float posY = movePacket.Transform.PosY;
            float posZ = movePacket.Transform.PosZ;
            float rotX = movePacket.Transform.RotX;
            float rotY = movePacket.Transform.RotY;
            float rotZ = movePacket.Transform.RotZ;
            float rotW = movePacket.Transform.RotW;
            Quaternion localRotation = new Quaternion(rotX,rotY,rotZ,rotW);
            
            CharacterController ghostController = ghostObj.GetComponent<CharacterController>();
            ghostController.transform.position = new Vector3(posX,posY,posZ); //고스트 위치 순간이동
            ghostObj.transform.rotation = new Quaternion(rotX,rotY,rotZ,rotW); //고스트 회전
            
            //TODO : 눌린 방향키 해석해서 velocity구한다음에 이동시키는 코드 추가하기
            ghostObj.GetComponent<Ghost>().CalculateVelocity(movePacket.KeyboardInput, localRotation);
            if(_players.TryGetValue(playerId,out GameObject playerObj))
            {
                playerObj.GetComponent<Player>().SetGhostLastState(movePacket.KeyboardInput, localRotation);
            }
        }
        
    }
}
