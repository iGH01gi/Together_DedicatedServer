using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerManager : MonoBehaviour
{
    public PlayerMoveController _playerMoveController;
    
    object _lock = new object();
    public Dictionary<int,GameObject> _players = new Dictionary<int, GameObject>(); //key: 데디서버의 playerId, value: 플레이어 오브젝트
    public Dictionary<int,GameObject> _ghosts = new Dictionary<int, GameObject>()   ; //key: 데디서버의 playerId, value: 고스트 오브젝트
    public string _tempPlayerPrefabPath = "Player/Player";
    public string _tempTargetGhost = "Player/TargetGhost";
    public Transform _spawnPointCenter;
    private bool[] _possibleSpawnPoint; //8개의 스폰포인트 중에 가능한 스폰포인트를 체크하는 배열

    public int _configuredPlayerCount = 8; //방장이 알려준 플레이어 수(대기할때 사용됨, 기본값은 8, 3초까지 기다림)
    public int _roomId = -1; //방장이 알려준 방id(혹시 몰라서 들고있음, 기본값은 -1)

    public void Init()
    {
        _playerMoveController = new PlayerMoveController();
        _spawnPointCenter = GameObject.Find("Map/SpawnPoint").transform;
        _possibleSpawnPoint = new bool[8]{true,true,true,true,true,true,true,true};
    }
    
    /// <summary>
    /// 방장이 보내준 방 정보 저장
    /// </summary>
    /// <param name="roomId">방 번호</param>
    /// <param name="playerCount">데디서버에 접속될 플레이어 수</param>
    public void SetRoomInfo(int roomId,int playerCount)
    {
        _roomId = roomId;
        _configuredPlayerCount = playerCount;
    }
    
    //모든 플레이어에게 패킷 전송
    public void Broadcast(IMessage packet)
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            Managers.Session._sessions.TryGetValue(a.Key, out ClientSession session);
            if (session != null)
            {
                session.Send(packet);
            }
        }
    }
    
    /// <summary>
    /// 플레이어를 스폰하고 정보를 저장함. 모든이에게 추가된 플레이어의 정보를 알림
    /// </summary>
    /// <param name="session"></param>
    /// <param name="roomId"></param>
    /// <param name="name"></param>
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
                        PositionInfo positionInfo = new PositionInfo();
                        RotationInfo rotationInfo = new RotationInfo();
                        
                        positionInfo.PosX = playerObj.transform.position.x;
                        positionInfo.PosY = playerObj.transform.position.y;
                        positionInfo.PosZ = playerObj.transform.position.z;
                        rotationInfo.RotX = playerObj.transform.rotation.x;
                        rotationInfo.RotY = playerObj.transform.rotation.y;
                        rotationInfo.RotZ = playerObj.transform.rotation.z;
                        rotationInfo.RotW = playerObj.transform.rotation.w;
                        
                        transformInfo.Position = positionInfo;
                        transformInfo.Rotation = rotationInfo;
                        
                        playerTransforms.Add(player.Info.PlayerId,transformInfo);
                    }
                }
                allowEnterPacket.PlayerTransforms.Add(playerTransforms);
                session.Send(allowEnterPacket);
                
                //다른 클라이언트에게 새로운 플레이어가 게임에 접속했음을 전송함
                informNewFaceInDedicatedServerPacket.NewPlayer = newPlayer.Info;
                TransformInfo newPlayerTransformInfo = new TransformInfo();
                PositionInfo newPlayerPositionInfo = new PositionInfo();
                RotationInfo newPlayerRotationInfo = new RotationInfo();
                
                newPlayerPositionInfo.PosX = newPlayerObj.transform.position.x;
                newPlayerPositionInfo.PosY = newPlayerObj.transform.position.y;
                newPlayerPositionInfo.PosZ = newPlayerObj.transform.position.z;
                newPlayerRotationInfo.RotX = newPlayerObj.transform.rotation.x;
                newPlayerRotationInfo.RotY = newPlayerObj.transform.rotation.y;
                newPlayerRotationInfo.RotZ = newPlayerObj.transform.rotation.z;
                newPlayerRotationInfo.RotW = newPlayerObj.transform.rotation.w;
                
                newPlayerTransformInfo.Position = newPlayerPositionInfo;
                newPlayerTransformInfo.Rotation = newPlayerRotationInfo;
                
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
    /// <param name="dediPlayerObj">Destroy할 플레이어 오브젝트</param>
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
    /// <param name="session"></param>
    /// <param name="roomId"></param>
    /// <param name="name"></param>
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
            
            Util.PrintLog($"{newPlayer.Info.PlayerId}번 플레이어는 {trueIndices[randomIndex]}번째 스폰포인트에 스폰되었습니다");
            
            //해당 스폰포인트는 사용불가로 설정
            _possibleSpawnPoint[trueIndices[randomIndex]] = false;
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
    /// <param name="playerId">대응되는 플레이어의 id</param>
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
    /// 랜덤으로 킬러를 선택하고 해당 플레이어의 isKiller를 true로 설정
    /// </summary>
    /// <returns>선정된 킬러id</returns>
    public int RandomSelectKiller()
    {
        List<int> playerIds = new List<int>(_players.Keys);
        int randomIndex = Random.Range(0, playerIds.Count);
        
        int killerId = playerIds[randomIndex];
        //해당 player.cs에 있는 isKiller를 true로 설정
        _players[killerId].GetComponent<Player>()._isKiller = true;
        
        return killerId;
    }

    /// <summary>
    /// 킬러를 해제하는 함수. 모든 플레이어의 isKiller를 false로 설정
    /// </summary>
    public void ClearKiller()
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            a.Value.GetComponent<Player>()._isKiller = false;
        }
    }
    
    //킬러의 Player컴포넌트를 반환함. 킬러가 없다면 null 반환
    public Player GetKillerPlayerComponent()
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            if (a.Value.GetComponent<Player>()._isKiller)
            {
                return a.Value.GetComponent<Player>();
            }
        }

        return null;
    }
    
    /// <summary>
    /// 킬러의 플레이어id를 반환함. 킬러가 없다면 -1 반환
    /// </summary>
    /// <returns></returns>
    public int GetKillerId()
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            if (a.Value.GetComponent<Player>()._isKiller)
            {
                return a.Key;
            }
        }

        return -1;
    }
    
    /// <summary>
    /// 킬러인지 아닌지 확인하는 함수
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns>킬러면 true. 아니면 false</returns>
    public bool IsKiller(int playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            return _players[playerId].GetComponent<Player>()._isKiller;
        }

        return false;
    }

    
    
    #region 밤 게이지 관련
    
    /// <summary>
    /// 킬러의 게이지를 감소시킴. 만약 감소시킨 결과가 0보다 작다면 0으로 설정
    /// </summary>
    /// <param name="amount"></param>
    public void DecreaseKillerGauge(float amount)
    { 
        Player killer = GetKillerPlayerComponent();
        
        if(killer!=null)
            DecreaseGauge(killer.Info.PlayerId,amount);
    }

    /// <summary>
    /// 킬러를 제외한 모든 플레이어의 게이지를 감소시킴. 만약 감소시킨 결과가 0보다 작다면 0으로 설정
    /// </summary>
    /// <param name="amount">얼마나 감소시킬것인지</param>
    public void DecreaseAllSurvivorGauge(float amount)
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            if (!a.Value.GetComponent<Player>()._isKiller)
            {
                DecreaseGauge(a.Key, amount);
            }
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 감소시킴. 만약 감소시킨 결과가 0보다 작다면 0으로 설정
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="amount">얼만큼 감소시킬 것인가</param>
    /// <returns>감소된 게이지 결과. 존재하지 않는 플레이어라면 -1 반환</returns>
    public float DecreaseGauge(int playerId,float amount)
    {
        //존재하지 않는 플레이어라면 -1 반환
        if (!_players.ContainsKey(playerId))
        {
            return -1;
        }
        
        _players[playerId].GetComponent<Player>()._gauge -= amount;
        if (_players[playerId].GetComponent<Player>()._gauge < 0)
        {
            _players[playerId].GetComponent<Player>()._gauge = 0;
        }
        
        return _players[playerId].GetComponent<Player>()._gauge;
    }
    
    
    /// <summary>
    /// 킬러의 gauge를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">얼마나 증가시킬것인지</param>
    public void IncreaseKillerGauge(float amount)
    {
        Player killer = GetKillerPlayerComponent();
        
        if(killer!=null)
            IncreaseGauge(killer.Info.PlayerId,amount);
    }
    
    /// <summary>
    /// 킬러를 제외한 모든 플레이어의 게이지를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">얼마나 증가시킬 것인가</param>
    public void IncreaseAllSurvivorGauge(float amount)
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            if (!a.Value.GetComponent<Player>()._isKiller)
            {
                IncreaseGauge(a.Key, amount);
            }
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="amount">얼만큼 증가시킬 것인가</param>
    /// <returns>증가된 게이지 결과. 존재하지 않는 플레이어라면 -1 반환</returns>
    public float IncreaseGauge(int playerId,float amount)
    {
        //존재하지 않는 플레이어라면 -1 반환
        if (!_players.ContainsKey(playerId))
        {
            return -1;
        }
        
        _players[playerId].GetComponent<Player>()._gauge += amount;
        if (_players[playerId].GetComponent<Player>()._gauge > Managers.Time._gaugeMax)
        {
            _players[playerId].GetComponent<Player>()._gauge = Managers.Time._gaugeMax;
        }
        
        return _players[playerId].GetComponent<Player>()._gauge;
    }
    
    
    /// <summary>
    /// 모든 플레이어의 게이지를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">설정할 게이지 값</param>
    public void SetAllGauge(float amount)
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            SetGauge(a.Key, amount);
        }
    }
    /// <summary>
    /// 특정 플레이어의 gauge를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="amount">설정할 게이지 값</param>
    /// <returns></returns>
    public float SetGauge(int playerId,float amount)
    {
        //존재하지 않는 플레이어라면 -1 반환
        if (!_players.ContainsKey(playerId))
        {
            return -1;
        }
        
        //만약 amount가 0보다 작다면 0으로 설정
        if (amount < 0)
        {
            amount = 0;
        }
        //만약 amount가 _gaugeMax보다 크다면 _gaugeMax로 설정
        if (amount > Managers.Time._gaugeMax)
        {
            amount = Managers.Time._gaugeMax;
        }
        
        _players[playerId].GetComponent<Player>()._gauge = amount;
        
        return amount;
    }
    
    
    /// <summary>
    /// 특정 플레이어의 게이지를 반환함. 존재하지 않는 플레이어라면 -1 반환
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    public float GetGauge(int playerId)
    {
        if (_players.ContainsKey(playerId))
        {
            return _players[playerId].GetComponent<Player>()._gauge;
        }

        return -1;
    }
    
    /// <summary>
    /// player들을 순회하면서 게이지가 0인 플레이어가 있는지 확인하고, 있다면 해당 playerId를 반환. 없다면 -1 반환
    /// </summary>
    /// <returns></returns>
    public int CheckZeroGauge()
    {
        foreach (KeyValuePair<int, GameObject> a in _players)
        {
            if (a.Value.GetComponent<Player>()._gauge <= 0)
            {
                return a.Key;
            }
        }

        return -1;
    }

    #endregion

}


